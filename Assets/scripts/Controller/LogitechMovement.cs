using UnityEngine;
using Logitech;

public class LogitechMovement : MonoBehaviour
{

    internal static LogitechMovement Instance {get; private set;}
    
    [Header("Logitech G923 Settings")]
    public bool useLogitechWheel = true;
    public float forceFeedbackMultiplier = 1.0f;
    internal bool logitechInitialized = false;
    internal bool lastLogiConnected = false;
    
    PlayerCarController PlayerCar;



    internal void InitializeLogitechWheel()
    {
        if (!useLogitechWheel)
        {
            Debug.Log("[CarController] useLogitechWheel is false — skipping Logitech initialization.");
            return;
        }

        logitechInitialized = LogitechSDKManager.Initialize();
        if (logitechInitialized)
            Debug.Log("[CarController] Logitech wheel initialized successfully.");
        else
            Debug.LogWarning("[CarController] Logitech wheel failed to initialize.");
    }

    internal void GetLogitechInputs()
    {
        if (!LogitechGSDK.LogiIsConnected(0)) return;

        var state = LogitechGSDK.LogiGetStateUnity(0);
        PlayerCar.SteerInput = state.lX / 32768.0f;
        
        // Logitech pedals: -32768 (fully pressed) to 32767 (not pressed)
        // Invert and normalize to 0-1 range
        float throttle = Mathf.Clamp01(-state.lY / 32768.0f);
        
        // Clutch pedal for reverse - rglSlider is an array, index 0 is clutch
        float clutch = Mathf.Clamp01(-state.rglSlider[0] / 32768.0f);
        
        if (clutch > 0.1f)
            PlayerCar.MoveInput = -clutch;
        else if (throttle > 0.1f)
            PlayerCar.MoveInput = throttle;
        else
            PlayerCar.MoveInput = 0f;
    }

    internal void StopAllForceFeedback()
    {
        if (!logitechInitialized || !LogitechGSDK.LogiIsConnected(0)) return;
        
        LogitechGSDK.LogiStopDirtRoadEffect(0);
    }

    internal void ApplyForceFeedback()
    {
        if (!LogitechSDKManager.IsReady) return;

        if (GameManager.instance.isPaused)
        {
            LogitechGSDK.LogiStopDirtRoadEffect(0);
            return;
        }
        if (!logitechInitialized || !LogitechGSDK.LogiIsConnected(0)) return;
        
        float speed = PlayerCar.CarRb.linearVelocity.magnitude * 3.6f;

        // Continuously apply spring force (centering)
        int springStrength = Mathf.RoundToInt(40 * forceFeedbackMultiplier);
        LogitechGSDK.LogiPlaySpringForce(0, 0, 100, springStrength);

        // Continuously apply damper force (resistance) for steering
        int damperStrength = Mathf.RoundToInt(10 * forceFeedbackMultiplier);
        LogitechGSDK.LogiPlayDamperForce(0, damperStrength);
        
        // Dirt road only when on grass and moving
        if (PlayerCar.IsOnGrassCached() && speed >= 10)
            LogitechGSDK.LogiPlayDirtRoadEffect(0, Mathf.RoundToInt(12.5f * forceFeedbackMultiplier));
        else
            LogitechGSDK.LogiStopDirtRoadEffect(0);
    }
}
