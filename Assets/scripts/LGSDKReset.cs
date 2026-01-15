using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LGSDKReset : MonoBehaviour
{
    void OnEnable()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(ResetSDKNextFrame());
    }

    IEnumerator ResetSDKNextFrame()
    {
        yield return new WaitForSeconds(0.25f);
        LogitechSDKManager.ForceReinitialize();
    }
}