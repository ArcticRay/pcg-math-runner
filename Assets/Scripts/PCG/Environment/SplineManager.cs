using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using UnityEngine.InputSystem;

public class SplineManager : MonoBehaviour
{
    // Container for three Splines (lanes)
    public SplineContainer splineContainer;

    [Header("Prozedurale Parameter")]
    public float segmentLength = 10f;         // Length of new segments
    public float maxCumulativeAngle = 90f;      // in degree
    public float maxDeltaAngle = 20f;           // in degree
    public float horizontalNoiseScale = 0.1f;   // Scaling for noise
    public float verticalNoiseScale = 0.1f;     // Scaling for noise
    public float heightAmplitude = 2f;          // Hight for path variation
    public float laneOffset = 3f;               // Lane offset for additional 2 lanes

    
    private Vector3 baseDirection = Vector3.right;
    
    private float cumulativeAngle = 0f;

    // Counter for knots
    private int counter = 4;

    void Start()
    {

        if (splineContainer == null)
            splineContainer = GetComponent<SplineContainer>();

        if (splineContainer == null)
        {
            Debug.LogError("Kein SplineContainer gefunden!");
            return;
        }

        // Create three Splines in Container
        Spline[] newSplines = new Spline[3];
        for (int i = 0; i < newSplines.Length; i++)
        {
            newSplines[i] = new Spline();
        }
        
        splineContainer.Splines = newSplines;

        // Create four base knots
        BezierKnot[] initialKnots = new BezierKnot[4];
        for (int i = 0; i < initialKnots.Length; i++)
        {
            float x = i * segmentLength;
            // Basic path
            initialKnots[i] = new BezierKnot(
                new float3(x, 0, 0),
                new float3((i == 0) ? 0 : -segmentLength * 0.5f, 0, 0),
                new float3((i < 3) ? segmentLength * 0.5f : 0, 0, 0),
                quaternion.identity
            );
        }

        // Clear splines
        splineContainer.Splines[0].Clear();
        splineContainer.Splines[1].Clear();
        splineContainer.Splines[2].Clear();

        
        for (int i = 0; i < initialKnots.Length; i++)
        {
            splineContainer.Splines[0].Add(initialKnots[i]);

            // Bei einem geraden Pfad ist die Tangente in X-Richtung, sodass:
            // Der "rechte" Vektor = Cross(Vector3.up, Vector3.right) ergibt (0, 0, -1).
            Vector3 tangent = Vector3.right;
            Vector3 rightVec = Vector3.Cross(Vector3.up, tangent).normalized;
            Vector3 offset = rightVec * laneOffset;

            // Linke Spur: Zentrum minus offset
            BezierKnot leftKnot = new BezierKnot(
                initialKnots[i].Position - (float3)offset,
                initialKnots[i].TangentIn,
                initialKnots[i].TangentOut,
                quaternion.identity
            );
            // Rechte Spur: Zentrum plus offset
            BezierKnot rightKnot = new BezierKnot(
                initialKnots[i].Position + (float3)offset,
                initialKnots[i].TangentIn,
                initialKnots[i].TangentOut,
                quaternion.identity
            );
            splineContainer.Splines[1].Add(leftKnot);
            splineContainer.Splines[2].Add(rightKnot);
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
        // Central Lane as base
        int lastIndex = splineContainer.Splines[0].Count - 1;
        float3 lastPos = splineContainer.Splines[0][lastIndex].Position;
        
        float noiseValue = Mathf.PerlinNoise(counter * horizontalNoiseScale, 0f);
        float deltaAngle = Mathf.Lerp(-maxDeltaAngle, maxDeltaAngle, noiseValue);
        
        if (Mathf.Abs(cumulativeAngle + deltaAngle) > maxCumulativeAngle)
        {
            deltaAngle = Mathf.Sign(deltaAngle) * (maxCumulativeAngle - Mathf.Abs(cumulativeAngle));
        }
        cumulativeAngle += deltaAngle;
        
        Quaternion rot = Quaternion.AngleAxis(cumulativeAngle, Vector3.up);
        Vector3 newDirection = rot * baseDirection;
        
        // vertical Variation via Perlin Noise:
        float heightNoise = Mathf.PerlinNoise(counter * verticalNoiseScale, 1f);
        float verticalOffset = Mathf.Lerp(-heightAmplitude, heightAmplitude, heightNoise);
        
        Vector3 newPointCenter = (Vector3)lastPos + newDirection * segmentLength + new Vector3(0, verticalOffset, 0);
        
        Vector3 rightVec = Vector3.Cross(Vector3.up, newDirection).normalized;
        
        Vector3 newPointLeft = newPointCenter - rightVec * laneOffset;
        Vector3 newPointRight = newPointCenter + rightVec * laneOffset;
        
        // Create new bezier knots
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
        
        splineContainer.Splines[0].Add(newKnotCenter);
        splineContainer.Splines[1].Add(newKnotLeft);
        splineContainer.Splines[2].Add(newKnotRight);
        
        counter++;
    }
}
