using System.Runtime.Serialization.Formatters;
using UnityEditor.UI;
using UnityEngine;

public class ColorChangerAI : MonoBehaviour
{
    public Light pointLight;
    public Light right;
    public Light left;
    public float duration = 1.0f;

    CarInputActions Controls;

    void Awake()
    {
        Controls = new CarInputActions();
        Controls.Enable();
    }

    private void OnEnable()
    {
        foreach (Transform child in transform)
        {
            Light childLight = child.GetComponent<Light>();

            if (child.CompareTag("pl"))
            {
                if (childLight != null)
                {
                    pointLight = childLight;
                }
            }
            else if (child.CompareTag("rl"))
            {
                if (childLight != null)
                {
                    right = childLight;
                }
            }
            else if (child.CompareTag("ll"))
            {
                if (childLight != null)
                {
                    left = childLight;
                }
            }
        }
    }

    private void OnDisable()
    {
        Controls.Disable();
    }



    private void Update()
    {
        if (pointLight.enabled)
        {
            float t = Mathf.PingPong(Time.time / duration, 1.0f);
            pointLight.color = Color.Lerp(Color.red, Color.blue, t);
        }
    }
}