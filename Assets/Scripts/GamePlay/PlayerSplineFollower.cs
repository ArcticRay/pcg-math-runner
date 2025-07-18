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

    private bool isSwitchingLane = false;

    [Header("Lane Settings")]
    public float laneOffset = 20f;
    // Lane-Indices: -1 = links, 0 = center, 1 = rechts
    private int targetLane = 0;
    private float currentLateralOffset = 0f;

    [Header("Spurwechsel-Cooldown")]
    public float laneSwitchCooldown = 0.5f; // Time in Seconds
    private float switchTimer = 0f;

    // progress on lane
    private float progress = 0f;

    public float jumpForce = 30f;
    public float gravity = 50f;
    private float verticalVelocity = 0f;
    private float yOffset = 0f;

    private bool isInAir = false;


    public Animator anim;
    public Rigidbody rb;

    void Update()
    {
        // if (switchTimer > 0f)
        // switchTimer -= Time.deltaTime;

        float speed = baseSpeed;
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.W))
        {
            // anim.SetBool("IsRunning", true);
            speed *= sprintMultiplier;
        }
        else
        {
            // anim.SetBool("IsRunning", false);
        }

        if (Input.GetKey(KeyCode.W))
        {
            // anim.SetBool("IsJogging", true);
            anim.SetBool("Walking", true);
            progress += speed * Time.deltaTime;
        }
        else
        {
            anim.SetBool("Walking", false);
        }

        if (switchTimer <= 0f)
        {
            if (Input.GetKeyDown(KeyCode.A) && targetLane > -1)
            {
                targetLane--;
                if (!anim.GetBool("isJumping"))
                {
                    anim.SetTrigger("Left");
                }
                isSwitchingLane = true;
                switchTimer = laneSwitchCooldown;
            }
            else if (Input.GetKeyDown(KeyCode.D) && targetLane < 1)
            {
                targetLane++;
                if (!anim.GetBool("isJumping"))
                {
                    anim.SetTrigger("Right");
                }
                isSwitchingLane = true;
                switchTimer = laneSwitchCooldown;
            }
        }
        else
        {
            switchTimer -= Time.deltaTime;

            if (switchTimer <= 0f)
            {
                isSwitchingLane = false;
            }
        }

        HandleJump();

        // calculate spline position
        int numSegments = centerSplineContainer.Spline.Count - 1;
        float totalLength = numSegments * segmentLength;
        float t = totalLength > 0f ? progress / totalLength : 0f;
        t = Mathf.Clamp01(t);

        float3 posF3 = centerSplineContainer.EvaluatePosition(t);
        Vector3 pos = (Vector3)posF3;
        float3 tangentF3 = centerSplineContainer.EvaluateTangent(t);
        Vector3 tangent = ((Vector3)tangentF3).normalized;
        Vector3 rightVec = Vector3.Cross(Vector3.up, tangent).normalized;

        float desiredOffset = targetLane * laneOffset;
        currentLateralOffset = Mathf.Lerp(currentLateralOffset, desiredOffset, laneSwitchSpeed * Time.deltaTime);

        // Vector3 finalPos = pos + rightVec * currentLateralOffset;
        // finalPos.y = pos.y;
        // transform.position = finalPos;

        Vector3 finalPos = pos + rightVec * currentLateralOffset + Vector3.up * yOffset;
        finalPos.y = pos.y + yOffset;  // sicherstellen, dass Y korrekt ist
        transform.position = finalPos;

        // set forward direction
        Vector3 horizontalTangent = new Vector3(tangent.x, 0, tangent.z).normalized;
        if (horizontalTangent.sqrMagnitude > 0.001f)
            transform.forward = horizontalTangent;
    }

    private void HandleJump()
    {
        // Sprung auslösen
        if (Input.GetKeyDown(KeyCode.Space) && !isInAir && !isSwitchingLane)
        {
            verticalVelocity = jumpForce;
            isInAir = true;
            anim.SetBool("isJumping", true);
        }

        // Gravitation anwenden
        verticalVelocity -= gravity * Time.deltaTime;
        yOffset += verticalVelocity * Time.deltaTime;

        // Bodenkollision
        if (yOffset < 0f)
        {
            yOffset = 0f;
            verticalVelocity = 0f;

            if (isInAir)
            {
                isInAir = false;
                anim.SetBool("isJumping", false);
            }
        }
    }


}
