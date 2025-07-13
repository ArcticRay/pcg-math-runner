using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.Linq;

public class SplineManagerNew : MonoBehaviour
{
    [Header("Spline Settings")]
    public float laneOffset = 20f;

    public SplineContainer centerSplineContainer;
    public SplineContainer leftSplineContainer;
    public SplineContainer rightSplineContainer;

    private SplineContainer[] splineContainers;


    [SerializeField] private float spawnHeight = 100f;
    [SerializeField] private Vector3 obstacleScale = new Vector3(10f, 10f, 10f);
    public GameObject[] dynamicObstaclePrefabs;
    public GameObject[] obstaclePrefabs;
    private int obstaclesOnFail = 5;

    public int MasterPointCount;

    [Header("Runtime Visualization")]
    [SerializeField] LineRenderer centerLine, leftLine, rightLine;
    [SerializeField] GameObject handlePrefab;
    [SerializeField] int samplesPerCurve = 500;

    [SerializeField] float elevationOffset = 2f;

    private List<GameObject> handles = new List<GameObject>();

    [Header("Debug")]
    public bool drawDebugSplines = true;
    public List<Vector3> masterPoints = new List<Vector3>();

    private List<Vector3> leftPoints = new List<Vector3>();
    private List<Vector3> rightPoints = new List<Vector3>();


    private WorldGenerationParameters parameters;

    public static SplineManagerNew Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        splineContainers = new[]
        {
            centerSplineContainer,
            leftSplineContainer,
            rightSplineContainer
        };
    }

    private void Start()
    {
        parameters = WorldGenerationParameterSerialization.GetWorldGenerationParameters();
        laneOffset = 20f;
        MasterPointCount = masterPoints.Count;

    }

    public (Vector3, Vector3, float) GetSegment(float t)
    {
        float index = (MasterPointCount - 1) * t;
        int previousIndex = (int)Math.Floor(index);
        int nextIndex = (int)Math.Ceiling(index);

        Vector3 previousPoint = masterPoints[previousIndex];
        Vector3 nextPoint = masterPoints[nextIndex];
        float weight = index - previousIndex;

        return (previousPoint, nextPoint, weight);
    }

    public void UseMasterControlPoints(List<Vector3> ControlPoints)
    {
        masterPoints.Clear();
        masterPoints = ControlPoints;
        GenerateParallelSplines();

        IEnumerable<float3> knotPositions = masterPoints.Select(v => new float3(v.x, v.y, v.z));
        Spline centerSpline = new Spline(knotPositions, TangentMode.AutoSmooth, false);

        knotPositions = leftPoints.Select(v => new float3(v.x, v.y, v.z));
        Spline leftSpline = new Spline(knotPositions, TangentMode.AutoSmooth, false);

        knotPositions = rightPoints.Select(v => new float3(v.x, v.y, v.z));
        Spline rightSpline = new Spline(knotPositions, TangentMode.AutoSmooth, false);

        centerSplineContainer.RemoveSplineAt(0);
        leftSplineContainer.RemoveSplineAt(0);
        rightSplineContainer.RemoveSplineAt(0);

        centerSplineContainer.AddSpline(centerSpline);
        leftSplineContainer.AddSpline(leftSpline);
        rightSplineContainer.AddSpline(rightSpline);

        // centerSplineContainer.a
        UpdateLines();
        SpawnHandles(centerSplineContainer);
    }

    public void GenerateParallelSplines()
    {
        leftPoints.Clear();
        rightPoints.Clear();

        int samplesPerCurve = 20;
        int totalSamples = (masterPoints.Count - 1) * samplesPerCurve;

        for (int i = 0; i <= totalSamples; i++)
        {
            float t = i / (float)totalSamples;
            Vector3 centerPos = GetPositionOnMaster(t);
            Vector3 tang = GetTangentOnMaster(t).normalized;
            Vector3 side = Vector3.Cross(tang, Vector3.up).normalized;

            Vector3 leftPos = centerPos - side * laneOffset;
            Vector3 rightPos = centerPos + side * laneOffset;

            leftPoints.Add(leftPos);
            rightPoints.Add(rightPos);
        }
    }

    public void ClearParallelSplines()
    {
        masterPoints.Clear();
        leftPoints.Clear();
        rightPoints.Clear();
    }

    public Vector3 GetPositionOnMaster(float t)
    {
        return GetCatmullRomPosition(masterPoints, t);
    }
    public Vector3 GetTangentOnMaster(float t)
    {
        return GetCatmullRomTangent(masterPoints, t);
    }

    private Vector3 GetCatmullRomPosition(List<Vector3> pts, float t)
    {
        if (pts.Count < 4)
        {
            // Fallback to simple interpolation if not enough points
            return Vector3.Lerp(pts[0], pts[pts.Count - 1], t);
        }

        int segments = pts.Count - 1;
        float scaledT = t * segments;
        int i = Mathf.FloorToInt(scaledT);
        float f = scaledT - i;

        i = Mathf.Clamp(i, 0, pts.Count - 2);

        int i0 = Mathf.Clamp(i - 1, 0, pts.Count - 1);
        int i1 = i;
        int i2 = i + 1;
        int i3 = Mathf.Clamp(i + 2, 0, pts.Count - 1);

        return CatmullRom(pts[i0], pts[i1], pts[i2], pts[i3], f);
    }

    private Vector3 GetCatmullRomTangent(List<Vector3> pts, float t)
    {
        if (pts.Count < 4) return Vector3.forward;

        int segments = pts.Count - 1;
        float scaledT = t * segments;
        int i = Mathf.FloorToInt(scaledT);
        float f = scaledT - i;

        i = Mathf.Clamp(i, 0, pts.Count - 2);

        int i0 = Mathf.Clamp(i - 1, 0, pts.Count - 1);
        int i1 = i;
        int i2 = i + 1;
        int i3 = Mathf.Clamp(i + 2, 0, pts.Count - 1);

        return CatmullRomTangent(pts[i0], pts[i1], pts[i2], pts[i3], f);
    }

    private Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float f)
    {
        float f2 = f * f;
        float f3 = f2 * f;

        // 0.5 * (2p1 + (-p0 + p2)f + (2p0 - 5p1 + 4p2 - p3)f^2 + ...)
        return 0.5f * (
            (2.0f * p1) +
            (-p0 + p2) * f +
            (2.0f * p0 - 5.0f * p1 + 4.0f * p2 - p3) * f2 +
            (-p0 + 3.0f * p1 - 3.0f * p2 + p3) * f3
        );
    }
    private Vector3 CatmullRomTangent(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float f)
    {
        float f2 = f * f;

        // 0.5 * ( (-p0 + p2) + 2(...)f + 3(...)f^2 )
        return 0.5f * (
            (-p0 + p2) +
            2.0f * (2.0f * p0 - 5.0f * p1 + 4.0f * p2 - p3) * f +
            3.0f * (-p0 + 3.0f * p1 - 3.0f * p2 + p3) * f2
        );
    }


    private void OnDrawGizmos()
    {
        if (!drawDebugSplines || masterPoints == null || masterPoints.Count == 0)
            return;

        Gizmos.color = Color.red;
        DrawPointList(masterPoints);

        Gizmos.color = Color.blue;
        DrawPointList(leftPoints);

        Gizmos.color = Color.green;
        DrawPointList(rightPoints);
    }

    private void DrawPointList(List<Vector3> pts)
    {
        for (int i = 0; i < pts.Count - 1; i++)
        {
            Gizmos.DrawLine(pts[i], pts[i + 1]);
            Gizmos.DrawSphere(pts[i], 0.2f);
        }
        if (pts.Count > 0)
            Gizmos.DrawSphere(pts[pts.Count - 1], 0.2f);
    }

    public void UpdateLines()
    {
        DrawSpline(centerSplineContainer, centerLine);
        DrawSpline(leftSplineContainer, leftLine);
        DrawSpline(rightSplineContainer, rightLine);
    }

    void DrawSpline(SplineContainer c, LineRenderer lr)
    {
        if (c == null || lr == null) return;
        lr.positionCount = samplesPerCurve + 1;
        for (int i = 0; i <= samplesPerCurve; i++)
        {
            float t = i / (float)samplesPerCurve;
            Vector3 worldPos = c.transform.TransformPoint(c.Spline.EvaluatePosition(t)) + Vector3.up * elevationOffset;
            lr.SetPosition(i, worldPos);
        }
    }

    public void SpawnHandles(SplineContainer c)
    {
        foreach (var h in handles) Destroy(h);
        handles.Clear();

        for (int i = 0; i < c.Spline.Count; i++)
        {
            Vector3 worldPos = c.transform.TransformPoint(c.Spline[i].Position);
            var h = Instantiate(handlePrefab, worldPos, Quaternion.identity, transform);
            h.GetComponent<SplineHandle>().Init(c, i, this);
            handles.Add(h);
        }
    }

    public void SpawnObstacles(float tStart, float tEnd)
    {


        for (int i = 0; i < obstaclesOnFail; i++)
        {
            var chosenContainer = splineContainers[
                UnityEngine.Random.Range(0, splineContainers.Length)
            ];
            var spline = chosenContainer.Spline;

            float t = UnityEngine.Random.Range(tStart, tEnd);
            float3 pos3 = spline.EvaluatePosition(t);
            float3 forward = spline.EvaluateTangent(t);
            float3 up = spline.EvaluateUpVector(t);
            Quaternion rot = Quaternion.LookRotation(forward, up);

            Vector3 spawnPos = (Vector3)pos3;
            if (i % 2 == 0)
            {
                spawnPos = spawnPos + Vector3.up * spawnHeight;
                GameObject obstacle = Instantiate(dynamicObstaclePrefabs[0], spawnPos, rot, transform);
                obstacle.transform.localScale = obstacleScale;
                Debug.Log("Obstacles Erstellt");
            }
            else
            {
                GameObject obstacle = Instantiate(obstaclePrefabs[0], spawnPos, rot, transform);
                obstacle.transform.localScale = obstacleScale;
                Debug.Log("Obstacles Erstellt");
            }

        }
    }



}