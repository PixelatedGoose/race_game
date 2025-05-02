using UnityEngine;
using UnityEngine.UI;

public class optionScript : MonoBehaviour
{
    public Material pixelCount; // Assign this in the Inspector
    private Text pixelCountLabel;

    void OnEnable()
    {
        //PlayerPrefs.DeleteAll(); //VAIN DEBUGAAMISTA VARTEN
        if (PlayerPrefs.HasKey("pixel_value"))
        {
            Debug.Log("pixel_value löydetty; ei muuteta");
        }
        else
        {
            pixelCount.SetFloat("_pixelcount", 256);
            Debug.Log("pixel_value ei löydetty; arvo on nyt 256");
        }
    }

    void Start()
    {
        InitializeSliderValue("pixel");
        pixelCountLabel = GameObject.Find("LabelPA").GetComponent<Text>();
        pixelCountLabel.text = (PlayerPrefs.GetFloat("pixel_value") * 64).ToString();
    }

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

        if (optionObjectName == "pixel")
        {
            pixelCount.SetFloat("_pixelcount", PlayerPrefs.GetFloat("pixel_value") * 64);
            pixelCountLabel.text = (PlayerPrefs.GetFloat("pixel_value") * 64).ToString();
        }

        PlayerPrefs.Save();
        Debug.Log("muutettu: " + PlayerPrefs.GetFloat(optionObjectName + "_value"));
    }

    private void InitializeSliderValue(string optionObjectName)
    {
        var optionSlider = GameObject.Find(optionObjectName).GetComponent<UnityEngine.UI.Slider>();
        if (PlayerPrefs.HasKey(optionObjectName + "_value"))
        {
            optionSlider.value = PlayerPrefs.GetFloat(optionObjectName + "_value");
        }
    }
}

// --OPTIMISAATIO--
// poista GameObject.Find rivit functioneista, siirrä starttiin
// tee uudet option detectionit (jotta ei tarvitte tehä jokasta niinku 'if (optionObjectName == vitunpaska)...'

// huomio että tää koodi on myös PERSEESTÄ ja suosittelen VITUN vahvasti, että teet sen switch case systeemin vaan yhelle funktiolle

// vois pyöriä monen funktion avulla, mutta kaiken pitäs keskittyä yhteen funktioon, joka käyttää switch-case statementtia
