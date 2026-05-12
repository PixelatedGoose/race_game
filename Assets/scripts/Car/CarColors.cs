using UnityEngine;

public class CarColors : MonoBehaviour
{
    private Light[] carLights;
    private Light pointLight;
    private Light right;
    private Light left;
    public float duration = 3f;

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
}