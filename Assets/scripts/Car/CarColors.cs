using UnityEngine;

public class CarColors : MonoBehaviour
{
    private Light[] carLights;
    private Light pointLight;
    private Light right;
    private Light left;
    [SerializeField] private float duration = 3f;
    public AudioSource lights;

    private void OnEnable()
    {
        carLights = GetComponentsInChildren<Light>();
        foreach (Light child in carLights)
        {
            if (child.CompareTag("pl")) pointLight = child;
            else if (child.CompareTag("rl")) right = child;
            else if (child.CompareTag("ll")) left = child;
        }

        LeanTween.value(pointLight.gameObject, new Color(1f, 0f, 0f), new Color(0f, 0f, 1f), duration).setOnUpdate((Color val) => { pointLight.color = val; }).setLoopPingPong();
    }

    public void ToggleLights()
    {
        if (GameManager.IsPaused) return;

        left.enabled = !left.enabled;
        right.enabled = !right.enabled;
        lights.Play();
    }
}