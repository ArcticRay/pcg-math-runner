using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using System;

public class PlayerSplineFollower : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CombinedInputHandler input;
    public SplineContainer centerSplineContainer;
    public Animator anim;
    public Rigidbody rb;

    [Header("Spline Settings")]
    public float segmentLength = 20f;

    [Header("Movement Settings")]
    public float baseSpeed = 5f;
    public float sprintMultiplier = 1.5f;
    public float laneSwitchSpeed = 5f;

    [Header("Lane Settings")]
    public float laneOffset = 20f;
    public float laneSwitchCooldown = 0.5f;        // s
    private float switchTimer = 0f;
    private int targetLane = 0;                    // -1, 0, 1
    private float currentLateralOffset = 0f;
    private bool isSwitchingLane = false;

    [Header("Jump")]
    public float jumpForce = 30f;
    public float gravity = 50f;
    private float verticalVelocity = 0f;
    private float yOffset = 0f;
    private bool isInAir = false;

    [Header("Audio")]
    public AudioSource landingSource;
    public AudioClip landingClip;
    public AudioSource footstepSource;
    public AudioClip[] footstepClips;
    public float stepDistance = 15f;

    private bool isBlocked = false;
    private float progress = 0f;
    private float distanceAccumulator = 0f;
    private Vector3 lastPosition;

    private bool forwardHeld;
    private bool sprintHeld;

    void Reset()
    {
        if (!input) input = FindFirstObjectByType<CombinedInputHandler>();
    }

    void OnEnable()
    {
        if (!input) input = FindFirstObjectByType<CombinedInputHandler>();
        if (input)
        {
            input.OnMove += HandleMoveInput;
            input.OnLaneChange += HandleLaneChange;
            input.OnJump += HandleJumpInput;
        }
        else
        {
            Debug.LogWarning("PlayerSplineFollower: Kein CombinedInputHandler gefunden.");
        }
    }

    void OnDisable()
    {
        if (input)
        {
            input.OnMove -= HandleMoveInput;
            input.OnLaneChange -= HandleLaneChange;
            input.OnJump -= HandleJumpInput;
        }
    }

    void Start()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        TickCooldowns(Time.deltaTime);
        ApplyMovement(Time.deltaTime);
        ApplyJumpPhysics(Time.deltaTime);
        UpdateSplineTransform();
        UpdateFootsteps();
    }

    // ---------- Input-Event-Handler ----------
    private void HandleMoveInput(bool forwardHeld, bool sprintHeld)
    {
        this.forwardHeld = forwardHeld;
        this.sprintHeld = sprintHeld;
    }

    private void HandleLaneChange(int delta)
    {
        if (switchTimer > 0f) return;       // Cooldown
        if (delta == 0) return;

        int newLane = Mathf.Clamp(targetLane + Mathf.Clamp(delta, -1, 1), -1, 1);
        if (newLane == targetLane) return;

        targetLane = newLane;

        if (!anim.GetBool("isJumping"))
        {
            if (delta < 0) anim.SetTrigger("Left");
            else anim.SetTrigger("Right");
        }

        isSwitchingLane = true;
        switchTimer = laneSwitchCooldown;
    }

    private void HandleJumpInput()
    {
        if (isInAir || isSwitchingLane) return;

        verticalVelocity = jumpForce;
        isInAir = true;
        anim.SetBool("isJumping", true);
    }

    private void TickCooldowns(float dt)
    {
        if (switchTimer > 0f)
        {
            switchTimer -= dt;
            if (switchTimer <= 0f)
                isSwitchingLane = false;
        }
    }

    private void ApplyMovement(float dt)
    {
        float speed = baseSpeed * (sprintHeld ? sprintMultiplier : 1f);

        if (forwardHeld && !isBlocked)
        {
            anim.SetBool("Walking", true);
            progress += speed * dt;
        }
        else
        {
            anim.SetBool("Walking", false);
        }
    }

    private void ApplyJumpPhysics(float dt)
    {
        bool prevInAir = isInAir;

        verticalVelocity -= gravity * dt;
        yOffset += verticalVelocity * dt;

        if (yOffset < 0f)
        {
            yOffset = 0f;
            verticalVelocity = 0f;

            if (isInAir)
            {
                isInAir = false;
                anim.SetBool("isJumping", false);
                if (prevInAir) PlayLanding();
            }
        }
    }

    private void UpdateSplineTransform()
    {
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

        Vector3 finalPos = pos + rightVec * currentLateralOffset + Vector3.up * yOffset;
        finalPos.y = pos.y + yOffset;
        transform.position = finalPos;

        Vector3 horizontalTangent = new Vector3(tangent.x, 0, tangent.z).normalized;
        if (horizontalTangent.sqrMagnitude > 0.001f)
            transform.forward = horizontalTangent;
    }

    private void UpdateFootsteps()
    {
        if (anim.GetBool("Walking") && !isInAir && !isBlocked)
        {
            float delta = Vector3.Distance(transform.position, lastPosition);
            distanceAccumulator += delta;
            if (distanceAccumulator >= stepDistance)
            {
                PlayFootstep();
                distanceAccumulator = 0f;
            }
        }
        lastPosition = transform.position;
    }

    private void PlayFootstep()
    {
        if (footstepClips == null || footstepClips.Length == 0 || footstepSource == null) return;
        int idx = UnityEngine.Random.Range(0, footstepClips.Length);
        footstepSource.PlayOneShot(footstepClips[idx]);
    }

    private void PlayLanding()
    {
        if (landingClip == null || landingSource == null) return;
        landingSource.PlayOneShot(landingClip);
    }

    // ---------- Collision ----------
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            isBlocked = true;
            anim.SetBool("Walking", false);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Obstacle"))
            isBlocked = false;
    }
}
