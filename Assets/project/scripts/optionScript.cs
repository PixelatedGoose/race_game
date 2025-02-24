using UnityEngine;
using UnityEngine.Rendering.Universal;

public class optionScript : MonoBehaviour
{
    public Material pixelCount; // Assign this in the Inspector
    public void setToggleOptionValue(string optionObjectName)
    {
        var optionToggle = GameObject.Find(optionObjectName).GetComponent<UnityEngine.UI.Toggle>(); //etsi togglen nimi

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
        var optionSlider = GameObject.Find(optionObjectName).GetComponent<UnityEngine.UI.Slider>(); //etsi sliderin nimi

        PlayerPrefs.SetFloat(optionObjectName + "_value", optionSlider.value); //aseta sliderin value

        PlayerPrefs.Save();
        Debug.Log("muutettu: " + PlayerPrefs.GetFloat(optionObjectName + "_value"));

        if (optionObjectName == "pixel")
        {
            pixelCount.SetFloat("_pixelcount", PlayerPrefs.GetFloat("pixel_value") * 64);
        }
    }
}