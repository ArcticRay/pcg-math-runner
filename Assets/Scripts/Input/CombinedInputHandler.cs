using System;
using UnityEngine;

public class CombinedInputHandler : MonoBehaviour
{
    // --- Gameplay Events ---
    public event Action<bool, bool> OnMove;   // (forwardHeld, sprintHeld)
    public event Action<int> OnLaneChange;    // -1 left, +1 right
    public event Action OnJump;               // Jump

    [Header("Fallback / Debug")]
    [SerializeField] private bool forceKeyboard = false;
    [SerializeField] private bool logAxesOnce = true;

    [Header("Input Manager Axis Names (Axes 4–7)")]
    // Map these names in Unity Input Manager to board axes
    [SerializeField] private string axisTopLeft = "Axis 4";
    [SerializeField] private string axisTopRight = "Axis 5";
    [SerializeField] private string axisBotLeft = "Axis 6";
    [SerializeField] private string axisBotRight = "Axis 7";

    [Header("Board → kg / Normalization")]
    [Tooltip("Offset of empty board (kg), subtracted as tare.")]
    [SerializeField] private float tareKg = 0f;
    [Tooltip("Scaling: 1.0 input unit ≙ kg. Adjust so sum ≈ body weight.")]
    [SerializeField] private float kgPerUnit = 50f;
    [Tooltip("Smoothing factor (0..1): 0=slow, 1=no smoothing")]
    [Range(0f, 1f)][SerializeField] private float smoothing = 0.25f;

    [Header("Thresholds / Hysteresis / Cooldowns")]
    [Tooltip("Forward held if |forward| > threshold (0..1)")]
    [SerializeField] private float forwardHeldThreshold = 0.20f;
    [Tooltip("Sprint held if forward > threshold (0..1)")]
    [SerializeField] private float sprintThreshold = 0.60f;

    [Tooltip("Lane change if |lateral| ≥ threshold")]
    [SerializeField] private float laneThreshold = 0.35f;
    [Tooltip("Return to neutral if |lateral| < (threshold - hysteresis)")]
    [SerializeField] private float laneHysteresis = 0.15f;
    [Tooltip("Time between lane-change events (s)")]
    [SerializeField] private float laneCooldown = 0.35f;

    [Tooltip("Jump if weight delta (kg) within one frame ≤ -X")]
    [SerializeField] private float jumpDropKg = 12f;
    [Tooltip("Jump only if previous weight > X (prevents noise)")]
    [SerializeField] private float jumpMinPrevKg = 15f;

    [Tooltip("Absolute weight deadzone: below this sum is clamped to 0")]
    [SerializeField] private float weightDeadzoneKg = 1.0f;

    // --- Runtime State ---
    private bool usingBoard = false;
    private float filtTL, filtTR, filtBL, filtBR;   // filtered raw values
    private float lateral, forward;                 // -1..+1
    private float totalKg, lastTotalKg;
    private float laneCdTimer = 0f;
    private int leanLatch = 0;                      // -1,0,+1 (for hysteresis)
    private bool boardActivityLogged = false;

    void Start()
    {
        if (logAxesOnce)
        {
            var names = Input.GetJoystickNames();
            for (int i = 0; i < names.Length; i++)
                Debug.Log($"[Input] Joystick {i + 1}: '{names[i]}'");
        }
    }

    void Update()
    {
        if (!forceKeyboard && DetectBoardActivity())
        {
            HandleBalanceBoardInput();
        }
        else
        {
            HandleKeyboardInput();
        }
    }

    // ---------- Detection ----------
    // Detect if the board axes (4–7) provide nonzero values.
    private bool DetectBoardActivity()
    {
        float tl = Mathf.Max(0f, Input.GetAxisRaw(axisTopLeft));
        float tr = Mathf.Max(0f, Input.GetAxisRaw(axisTopRight));
        float bl = Mathf.Max(0f, Input.GetAxisRaw(axisBotLeft));
        float br = Mathf.Max(0f, Input.GetAxisRaw(axisBotRight));

        float sum = tl + tr + bl + br;
        bool active = sum > 0.001f;

        if (active && !boardActivityLogged)
        {
            Debug.Log("[BalanceBoard] Axes 4–7 provide values → board active.");
            boardActivityLogged = true;
        }
        usingBoard = active;
        return usingBoard;
    }

    // ---------- Keyboard Fallback ----------
    private void HandleKeyboardInput()
    {
        bool forwardHeld = Input.GetKey(KeyCode.W);
        bool sprintHeld = Input.GetKey(KeyCode.LeftShift);
        OnMove?.Invoke(forwardHeld, sprintHeld);

        if (Input.GetKeyDown(KeyCode.A)) OnLaneChange?.Invoke(-1);
        if (Input.GetKeyDown(KeyCode.D)) OnLaneChange?.Invoke(+1);
        if (Input.GetKeyDown(KeyCode.Space)) OnJump?.Invoke();
    }

    // ---------- Balance Board ----------
    private void HandleBalanceBoardInput()
    {
        float tlRaw = Mathf.Max(0f, Input.GetAxisRaw(axisTopLeft));
        float trRaw = Mathf.Max(0f, Input.GetAxisRaw(axisTopRight));
        float blRaw = Mathf.Max(0f, Input.GetAxisRaw(axisBotLeft));
        float brRaw = Mathf.Max(0f, Input.GetAxisRaw(axisBotRight));

        float a = Mathf.Clamp01(smoothing);
        filtTL = Mathf.Lerp(filtTL, tlRaw, a);
        filtTR = Mathf.Lerp(filtTR, trRaw, a);
        filtBL = Mathf.Lerp(filtBL, blRaw, a);
        filtBR = Mathf.Lerp(filtBR, brRaw, a);

        float unitsSum = filtTL + filtTR + filtBL + filtBR;
        float kg = Mathf.Max(0f, unitsSum * kgPerUnit - tareKg);
        if (kg < weightDeadzoneKg) kg = 0f;

        float denom = Mathf.Max(1e-5f, unitsSum);
        float x = ((filtTR + filtBR) - (filtTL + filtBL)) / denom;
        float y = ((filtTL + filtTR) - (filtBL + filtBR)) / denom;

        lateral = Mathf.Lerp(lateral, Mathf.Clamp(x, -1f, 1f), a);
        forward = Mathf.Lerp(forward, Mathf.Clamp(y, -1f, 1f), a);

        bool forwardHeld = forward > forwardHeldThreshold;
        bool sprintHeld = forward > sprintThreshold;
        OnMove?.Invoke(forwardHeld, sprintHeld);

        if (laneCdTimer > 0f) laneCdTimer -= Time.deltaTime;

        if (laneCdTimer <= 0f)
        {
            if (lateral >= laneThreshold && leanLatch != +1)
            {
                OnLaneChange?.Invoke(+1);
                leanLatch = +1;
                laneCdTimer = laneCooldown;
            }
            else if (lateral <= -laneThreshold && leanLatch != -1)
            {
                OnLaneChange?.Invoke(-1);
                leanLatch = -1;
                laneCdTimer = laneCooldown;
            }
            else
            {
                if (Mathf.Abs(lateral) < Mathf.Max(0f, laneThreshold - laneHysteresis))
                    leanLatch = 0;
            }
        }

        // Jump detection

        float deltaKg = kg - lastTotalKg;
        if (deltaKg <= -Mathf.Abs(jumpDropKg) && lastTotalKg > jumpMinPrevKg)
        {
            OnJump?.Invoke();
        }

        lastTotalKg = kg;
        totalKg = kg;
    }
}
