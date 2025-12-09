using UnityEngine;

public class ColorChanger : MonoBehaviour
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
        Controls.CarControls.lights.performed += ctx => LightsState(1, false);
        Controls.CarControls.underglow.performed += ctx => LightsState(2, false);

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
    
    /// <summary>
    /// tarkistaa, saako valoja vaihtaa. tavallisesti kutsutaan inputin kautta
    /// </summary>
    /// <returns></returns>
    /// <param name="shouldSet">jos funktio kutsutaan, pitäskö sen muuttaa valot asetuksen mukaiseksi?</param>
    public void LightsState(int lightSelected, bool shouldSet = false)
    {
        bool lightsOptionEnabled = PlayerPrefs.GetInt("optionTest_value") == 1; //lights?

        if (shouldSet) //set?
        {
            left.enabled = lightsOptionEnabled;
            right.enabled = lightsOptionEnabled;
            pointLight.enabled = lightsOptionEnabled;
            return;
        }
        if (!lightsOptionEnabled) return;

        switch (lightSelected)
        {
            case 1:
                left.enabled = !left.enabled;
                right.enabled = !right.enabled;
                break;
            case 2:
                pointLight.enabled = !pointLight.enabled;
                break;
            case 3:
                left.enabled = !left.enabled;
                right.enabled = !right.enabled;
                pointLight.enabled = !pointLight.enabled;
                break;
        }
    }
}