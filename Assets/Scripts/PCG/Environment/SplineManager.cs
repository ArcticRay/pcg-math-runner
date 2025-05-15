using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;

public class SplineManager : MonoBehaviour
{
    // a Spline Container per Lane
    public SplineContainer centerSplineContainer;
    public SplineContainer leftSplineContainer;
    public SplineContainer rightSplineContainer;

    private List<Vector3> splinePoints;

    [Header("Prozedurale Parameter")]
    public float segmentLength = 20f;         // Length of new segments
    public float maxCumulativeAngle = 90f;
    public float maxDeltaAngle = 20f;           // Maximales Winkelinkrement pro Segment in Grad
    public float horizontalNoiseScale = 0.1f;
    public float verticalNoiseScale = 0.1f;
    public float heightAmplitude = 2f;
    public float laneOffset = 3f;

    private Vector3 baseDirection = Vector3.right;
    private float cumulativeAngle = 0f;
    private int counter = 4;

    public event Action<List<Vector3>> OnSplineExtended;

    public SplineChunkGenerator splineChunkGenerator;

    void Start()
    {
        splinePoints = new List<Vector3>();
        if (centerSplineContainer == null || leftSplineContainer == null || rightSplineContainer == null)
        {
            Debug.LogError("Bitte alle drei SplineContainer (Center, Left, Right) zuweisen!");
            return;
        }


        centerSplineContainer.Spline.Clear();
        leftSplineContainer.Spline.Clear();
        rightSplineContainer.Spline.Clear();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ExtendSpline();
        }
    }

    public void UseMasterControlPoints(List<Vector3> list)
    {
        splinePoints = list;
        for (int i = 0; i < splinePoints.Count; i++)
        {
            Vector3 newPointCenter = splinePoints[i];
        }
    }


    void ExtendSpline()
    {
        // Center lane as base
        int lastIndex = centerSplineContainer.Spline.Count - 1;
        float3 lastPos = centerSplineContainer.Spline[lastIndex].Position;

        float randomOffsetX = UnityEngine.Random.Range(0f, 10000f);
        float randomOffsetY = UnityEngine.Random.Range(0f, 10000f);

        float noiseValue = Mathf.PerlinNoise(counter * horizontalNoiseScale + randomOffsetX, randomOffsetY);
        float deltaAngle = Mathf.Lerp(-maxDeltaAngle, maxDeltaAngle, noiseValue);

        // Constraints
        if (Mathf.Abs(cumulativeAngle + deltaAngle) > maxCumulativeAngle)
        {
            deltaAngle = Mathf.Sign(deltaAngle) * (maxCumulativeAngle - Mathf.Abs(cumulativeAngle));
        }
        cumulativeAngle += deltaAngle;

        Quaternion rot = Quaternion.AngleAxis(cumulativeAngle, Vector3.up);
        Vector3 newDirection = rot * baseDirection;

        float heightNoise = Mathf.PerlinNoise(counter * verticalNoiseScale + randomOffsetX, 1f);
        float verticalOffset = Mathf.Lerp(-heightAmplitude, heightAmplitude, heightNoise);

        Vector3 newPointCenter = (Vector3)lastPos + newDirection * segmentLength + new Vector3(0, verticalOffset, 0);

        Vector3 rightVec = Vector3.Cross(Vector3.up, newDirection).normalized;

        Vector3 newPointLeft = newPointCenter - rightVec * laneOffset;
        Vector3 newPointRight = newPointCenter + rightVec * laneOffset;

        BezierKnot newKnotCenter = new BezierKnot(
            new float3(newPointCenter),
            new float3(-newDirection * (segmentLength * 0.5f)),
            new float3(newDirection * (segmentLength * 0.5f)),
            quaternion.identity
        );
        BezierKnot newKnotLeft = new BezierKnot(
            new float3(newPointLeft),
            new float3(-newDirection * (segmentLength * 0.5f)),
            new float3(newDirection * (segmentLength * 0.5f)),
            quaternion.identity
        );
        BezierKnot newKnotRight = new BezierKnot(
            new float3(newPointRight),
            new float3(-newDirection * (segmentLength * 0.5f)),
            new float3(newDirection * (segmentLength * 0.5f)),
            quaternion.identity
        );

        centerSplineContainer.Spline.Add(newKnotCenter);
        leftSplineContainer.Spline.Add(newKnotLeft);
        rightSplineContainer.Spline.Add(newKnotRight);

        counter++;

        splinePoints.Add(newPointCenter);
        OnSplineExtended?.Invoke(splinePoints);
    }

    public List<Vector3> GetSplinePoints()
    {
        return splinePoints;
    }
}
