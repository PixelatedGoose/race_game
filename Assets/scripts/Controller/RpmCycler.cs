using UnityEngine;
using Logitech;

public class RpmCycler : MonoBehaviour
{
    [Header("Cycle Settings")]
    [Range(0f, 1f)] public float min = 0f;
    [Range(0f, 1f)] public float max = 1f;
    public float cycleSpeed = 0.2f;

    [Header("Warning Light")]
    public bool enableWarning = true;
    [Range(0f, 1f)] public float warningThreshold = 0.9f;

    void Update()
    {
        LogitechGSDK.LogiUpdate();
        
        float span = Mathf.Max(0f, max - min);
        float value = min + Mathf.PingPong(Time.time * cycleSpeed, span);
        
        if (enableWarning && value >= warningThreshold)
            LogitechLedController.SetMax();
        else
            LogitechLedController.SetNormalized(value);
    }

    void OnDisable()
    {
        LogitechLedController.Clear();
    }
}
