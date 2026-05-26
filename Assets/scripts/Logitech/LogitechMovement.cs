using UnityEngine;
using Logitech;

public class LogitechMovement : MonoBehaviour
{
    public static LogitechMovement Instance { get; private set; }

    [Header("Settings")]
    public bool useLogitechWheel = false;
    public float forceFeedbackMultiplier = 1f;

    internal bool logitechInitialized = false;
    internal bool lastLogiConnected = false;

    public bool allowAutoEnable = true;

    private PlayerCarController playerCar;
    private int activeDeviceIndex = 0;

    void Awake()
    {
        Instance = this;
        playerCar = FindFirstObjectByType<PlayerCarController>();
    }

    void Update()
    {
        bool connected = false;

        try { connected = LogitechGSDK.LogiIsConnected(0); }
        catch { connected = false; }

        // init on connect
        if (connected && !lastLogiConnected && useLogitechWheel)
        {
            InitializeLogitechWheel();
        }

        // auto enable wheel
        if (!useLogitechWheel && connected && allowAutoEnable)
        {
            var state = LogitechGSDK.LogiGetStateUnity(0);

            bool activity =
                Mathf.Abs(state.lX) > 5000 ||
                Mathf.Abs(state.lY) > 5000;

            if (activity)
            {
                useLogitechWheel = true;
                InitializeLogitechWheel();
            }
        }

        lastLogiConnected = connected;

        if (useLogitechWheel && logitechInitialized && connected)
        {
            ApplyWheelInput();
            ApplyForceFeedback();
        }
    }

    internal void InitializeLogitechWheel()
    {
        logitechInitialized = LogitechSDKManager.Initialize();
    }

    void ApplyWheelInput()
    {
        LogitechGSDK.LogiUpdate();

        var state = LogitechGSDK.LogiGetStateUnity(0);

        float steer = state.lX / 32768f;
        float throttle = Mathf.Clamp01(-state.lY / 32768f);

        if (playerCar == null) return;

        // ONLY override steering when wheel is active
        playerCar.MovementInputs = new Vector2(steer, throttle);
        playerCar.LastWheelInputTime = Time.time;
    }

    internal void ApplyForceFeedback()
    {
        if (!logitechInitialized) return;
        if (!LogitechGSDK.LogiIsConnected(0)) return;

        int spring = Mathf.RoundToInt(40 * forceFeedbackMultiplier);
        LogitechGSDK.LogiPlaySpringForce(0, 0, 100, spring);

        int damper = Mathf.RoundToInt(10 * forceFeedbackMultiplier);
        LogitechGSDK.LogiPlayDamperForce(0, damper);
    }

    internal void StopAllForceFeedback()
    {
        if (!logitechInitialized) return;
        LogitechGSDK.LogiStopDirtRoadEffect(0);
    }
}