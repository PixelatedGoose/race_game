using UnityEngine;

public class ColorChanger : MonoBehaviour
{
    public Light pointLight;

    public Light right;
    public Light left;

    public float duration = 1.0f;


    void Awake()
    {
        Controls = new CarInputActions();
        Controls.Enable();
    }

    CarInputActions Controls;



    private void OnEnable()
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

        Controls.Enable();
    }

    private void OnDisable()
    {
        Controls.Disable();
    }


    private void Update()
    {

        if (Controls.CarControls.lights.triggered && PlayerPrefs.GetInt("optionTest_value") == 1)
        {
            left.enabled = !left.enabled;
            right.enabled = !right.enabled;
        }

        if (Controls.CarControls.underglow.triggered && PlayerPrefs.GetInt("optionTest_value") == 1)
        {
            pointLight.enabled = !pointLight.enabled;
        }

        if (pointLight.enabled)
        {
            float t = Mathf.PingPong(Time.time / duration, 1.0f);
            pointLight.color = Color.Lerp(Color.red, Color.blue, t);
        }
    }

    public void CheckLightState()
    {
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
