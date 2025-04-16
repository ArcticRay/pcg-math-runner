using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using UnityEngine.InputSystem;

public class SplineManager : MonoBehaviour
{
    // a Spline Container per Lane
    public SplineContainer centerSplineContainer;
    public SplineContainer leftSplineContainer;
    public SplineContainer rightSplineContainer;

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

    void Start()
    {
        if (centerSplineContainer == null || leftSplineContainer == null || rightSplineContainer == null)
        {
            Debug.LogError("Bitte alle drei SplineContainer (Center, Left, Right) zuweisen!");
            return;
        }

        BezierKnot[] initialKnots = new BezierKnot[4];
        for (int i = 0; i < initialKnots.Length; i++)
        {
            float x = i * segmentLength;
            // Base Path (Spawn Area)
            initialKnots[i] = new BezierKnot(
                new float3(x, 0, 0),
                new float3((i == 0) ? 0 : -segmentLength * 0.5f, 0, 0),
                new float3((i < 3) ? segmentLength * 0.5f : 0, 0, 0),
                quaternion.identity
            );
        }

        centerSplineContainer.Spline.Clear();
        leftSplineContainer.Spline.Clear();
        rightSplineContainer.Spline.Clear();

        for (int i = 0; i < initialKnots.Length; i++)
        {
            centerSplineContainer.Spline.Add(initialKnots[i]);

            
            Vector3 tangent = Vector3.right;
            Vector3 rightVec = Vector3.Cross(Vector3.up, tangent).normalized;
            Vector3 offset = rightVec * laneOffset;

            BezierKnot leftKnot = new BezierKnot(
                initialKnots[i].Position - (float3)offset,
                initialKnots[i].TangentIn,
                initialKnots[i].TangentOut,
                quaternion.identity
            );
            BezierKnot rightKnot = new BezierKnot(
                initialKnots[i].Position + (float3)offset,
                initialKnots[i].TangentIn,
                initialKnots[i].TangentOut,
                quaternion.identity
            );
            leftSplineContainer.Spline.Add(leftKnot);
            rightSplineContainer.Spline.Add(rightKnot);
        }

        counter = initialKnots.Length;
        baseDirection = Vector3.right;
        cumulativeAngle = 0f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ExtendSpline();
        }
    }
    
    void ExtendSpline()
    {
        // Center lane as base
        int lastIndex = centerSplineContainer.Spline.Count - 1;
        float3 lastPos = centerSplineContainer.Spline[lastIndex].Position;
        
        float noiseValue = Mathf.PerlinNoise(counter * horizontalNoiseScale, 0f);
        float deltaAngle = Mathf.Lerp(-maxDeltaAngle, maxDeltaAngle, noiseValue);
        
        // Constraints
        if (Mathf.Abs(cumulativeAngle + deltaAngle) > maxCumulativeAngle)
        {
            deltaAngle = Mathf.Sign(deltaAngle) * (maxCumulativeAngle - Mathf.Abs(cumulativeAngle));
        }
        cumulativeAngle += deltaAngle;
        
        Quaternion rot = Quaternion.AngleAxis(cumulativeAngle, Vector3.up);
        Vector3 newDirection = rot * baseDirection;
        
        float heightNoise = Mathf.PerlinNoise(counter * verticalNoiseScale, 1f);
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
    }
}
