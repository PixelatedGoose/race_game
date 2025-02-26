using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class optionScript : MonoBehaviour
{
    public Material pixelCount; // Assign this in the Inspector

    void Start()
    {
        InitializeSliderValue("pixel");
        // Add other sliders here if needed
    }

    public void setToggleOptionValue(string optionObjectName)
    {
        var optionToggle = GameObject.Find(optionObjectName).GetComponent<Toggle>(); //etsi togglen nimi

        if (optionToggle.isOn)
        {
            PlayerPrefs.SetInt(optionObjectName + "_value", 1);
        }
        else
        {
            PlayerPrefs.SetInt(optionObjectName + "_value", 0);
        }

        PlayerPrefs.Save(); //tallennus
        Debug.Log("muutettu: " + PlayerPrefs.GetInt(optionObjectName + "_value"));
        // Debug.Log("NAME" + optionObjectName);
        // optionObjectName on sama ku gameobjectin nimi hierarkiassa
        // tol voi tarkistaa et ootko laittanu sen oikein
    }

    public void setSliderOptionValue(string optionObjectName)
    {
        var optionSlider = GameObject.Find(optionObjectName).GetComponent<Slider>(); //etsi sliderin nimi

        PlayerPrefs.SetFloat(optionObjectName + "_value", optionSlider.value); //aseta sliderin value

        PlayerPrefs.Save();
        Debug.Log("muutettu: " + PlayerPrefs.GetFloat(optionObjectName + "_value"));

        if (optionObjectName == "pixel")
        {
            pixelCount.SetFloat("_pixelcount", PlayerPrefs.GetFloat("pixel_value") * 64);
        }
    }

    private void InitializeSliderValue(string optionObjectName)
    {
        var optionSlider = GameObject.Find(optionObjectName).GetComponent<Slider>();
        if (PlayerPrefs.HasKey(optionObjectName + "_value"))
        {
            optionSlider.value = PlayerPrefs.GetFloat(optionObjectName + "_value");
        }
    }
}