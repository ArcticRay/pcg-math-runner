using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using UnityEngine.InputSystem;

public class PlayerSplineFollower : MonoBehaviour
{
    [Header("Spline Settings")]
    public SplineContainer centerSplineContainer;
    public float segmentLength = 20f;

    [Header("Movement Settings")]
    public float baseSpeed = 5f;              
    public float sprintMultiplier = 1.5f;       
    public float laneSwitchSpeed = 5f;         
    [Header("Lane Settings")]
    // -1 = links, 0 = center, 1 = rechts
    private int targetLane = 0;
    private float currentLateralOffset = 0f;  // Lerp-basierter Wert für den aktuellen Seitversatz
    public float laneOffset = 3f;             

    // Progress along the spline
    private float progress = 0f;

    void Update()
    {
        
        float speed = baseSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
            speed *= sprintMultiplier;
        if (Input.GetKey(KeyCode.W))
        {
            progress += speed * Time.deltaTime;
        }

        
        if (Input.GetKeyDown(KeyCode.A))
        {
            targetLane = Mathf.Clamp(targetLane - 1, -1, 1);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            targetLane = Mathf.Clamp(targetLane + 1, -1, 1);
        }

        int numSegments = centerSplineContainer.Spline.Count - 1;
        float totalLength = numSegments * segmentLength;

        float t = (totalLength > 0f) ? progress / totalLength : 0f;
        t = Mathf.Clamp01(t);

        float3 posF3 = centerSplineContainer.EvaluatePosition(t);
        Vector3 pos = (Vector3)posF3;
        float3 tangentF3 = centerSplineContainer.EvaluateTangent(t);
        Vector3 tangent = ((Vector3)tangentF3).normalized;

        Vector3 rightVec = Vector3.Cross(Vector3.up, tangent).normalized;

        float desiredOffset = targetLane * laneOffset;
        currentLateralOffset = Mathf.Lerp(currentLateralOffset, desiredOffset, laneSwitchSpeed * Time.deltaTime);

        Vector3 finalPos = pos + rightVec * currentLateralOffset;
        finalPos.y = pos.y;

        transform.position = finalPos;

        Vector3 horizontalTangent = new Vector3(tangent.x, 0, tangent.z).normalized;
        if (horizontalTangent.sqrMagnitude > 0.001f)
        {
            transform.forward = horizontalTangent;
        }
    }
}
