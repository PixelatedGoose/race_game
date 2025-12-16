using UnityEngine;
using Logitech;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class LogitechSDKManager
{
    private static bool isInitialized = false;
    private static bool initAttempted = false;

#if UNITY_EDITOR
    [InitializeOnLoadMethod]
    static void EditorInit()
    {
        // This runs when Unity Editor loads/reloads scripts
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            // DO NOT shutdown - keep SDK alive for next play session
            Debug.Log("[LogitechSDK] Play mode ending - keeping SDK alive");
        }
    }
#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void RuntimeInit()
    {
        if (!initAttempted)
        {
            InitializeSDK();
            initAttempted = true;
        }
    }

    static void InitializeSDK()
    {
        Debug.Log("[LogitechSDK] Initializing...");
        
        try
        {
            bool result = LogitechGSDK.LogiSteeringInitialize(false);
            Debug.Log($"[LogitechSDK] LogiSteeringInitialize returned: {result}");
            
            bool connected = LogitechGSDK.LogiIsConnected(0);
            Debug.Log($"[LogitechSDK] LogiIsConnected returned: {connected}");
            
            if (result || connected)
            {
                isInitialized = true;
                Debug.Log("[LogitechSDK] SDK initialized successfully!");
            }
            else
            {
                Debug.LogWarning("[LogitechSDK] Failed - restart G Hub and replug wheel, then restart Unity");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[LogitechSDK] Exception: {e.Message}");
        }
    }

    public static bool Initialize()
    {
        if (!isInitialized && !initAttempted)
        {
            InitializeSDK();
            initAttempted = true;
        }
        return isInitialized && LogitechGSDK.LogiIsConnected(0);
    }
}