using UnityEngine;

//tämä on EPÄPYHÄ HELVETTI ja aion korjata valojen toimivuuen lol. oletan et se on jotai PlayerPrefsin kanssa,
//koska ne valot sai pois päältä ainoastaa jos sen asetuksen laitto päälle ja pois. like seriously wtf
//HUOM. tein pieniä muutoksia joten jos ne ei toimi nii tuun korjaa tulevina päivinä, oletan vaan kaiken :)))
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
        Controls.Enable();
        
        foreach (Transform child in transform)
        {
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
            
            Light childLight = child.GetComponent<Light>();
        }
    }

    private void OnDisable()
    {
        Controls.Disable();
    }



    private void Update()
    {
        if (CheckLightState() == true)
        {
            if (Controls.CarControls.lights.triggered)
            {
                left.enabled = !left.enabled;
                right.enabled = !right.enabled;
            }

            if (Controls.CarControls.underglow.triggered)
            {
                pointLight.enabled = !pointLight.enabled;
            }
        }

        if (pointLight.enabled)
        {
            float t = Mathf.PingPong(Time.time / duration, 1.0f);
            pointLight.color = Color.Lerp(Color.red, Color.blue, t);
        }
    }

    public void CheckLightState()
    {
        bool lightEnabled = PlayerPrefs.GetInt("optionTest_value") == 0;
        if (!lightEnabled)
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
        return lightEnabled;
    }
}
