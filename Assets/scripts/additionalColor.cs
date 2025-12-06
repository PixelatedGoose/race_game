using UnityEngine;

public class AdditionalColorChanger : MonoBehaviour
{
    public Light pointLight;

    public Light right;
    public Light left;

    public float duration = 1.0f;
    private float t = 0.0f;
    private float colorTime = 0.0f;

    private void Start()
    {
        foreach (Transform child in transform)
        {
            if (child.CompareTag("pl"))
            {
                // Do something with child.gameObject, e.g. get the Light component
                Light childLight = child.GetComponent<Light>();
                if (childLight != null)
                {
                    pointLight = childLight;
                }
            }
            else if (child.CompareTag("rl"))
            {
                // Do something with child.gameObject, e.g. get the Light component
                Light childLight = child.GetComponent<Light>();
                if (childLight != null)
                {
                    right = childLight;
                }
            }
            else if (child.CompareTag("ll"))
            {
                // Do something with child.gameObject, e.g. get the Light component
                Light childLight = child.GetComponent<Light>();
                if (childLight != null)
                {
                    left = childLight;
                }
            }
        }
    }

    private void FixedUpdate()
    {
        t = Mathf.PingPong(Time.time / duration, 1.0f);

        if (pointLight.enabled)
        {
            colorTime += 0.01f;

            if (colorTime < 0.6f)
            {
                pointLight.color = Color.red;
            }
            else if (colorTime < 1.2f)
            {
                pointLight.color = Color.green;
            }
            else if (colorTime < 1.8f)
            {
                pointLight.color = Color.blue;
            }
            else
            {
                colorTime = 0.0f;
            }
        }

        left.color = Color.Lerp(Color.red, Color.blue, t);
        right.color = Color.Lerp(Color.red, Color.blue, t);

    }

    public void CheckLightState()
    {
        Debug.Log("FUNC CALL: " + PlayerPrefs.GetInt("optionTest_value"));
        if (PlayerPrefs.GetInt("optionTest_value") == 0)
        {
            pointLight.enabled = false;
            left.enabled = false;
            right.enabled = false;
        }
        else
        {
            pointLight.enabled = true;
            left.enabled = true;
            right.enabled = true;
        }
    }
}