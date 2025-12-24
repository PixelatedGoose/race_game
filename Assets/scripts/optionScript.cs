using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class optionScript : MonoBehaviour
{
    public Material pixelCount;
    private Text pixelCountLabel;
    private Dictionary<string, Slider> sliders = new Dictionary<string, Slider>();
    private Dictionary<string, Toggle> toggles = new Dictionary<string, Toggle>();

    private const float DefaultPixelValue = 256f;
    private const float PixelMultiplier = 64f;
    private const float DefaultVolume = 0.6f;

    void OnEnable()
    {
        if (!PlayerPrefs.HasKey("pixel_value"))
        {
            pixelCount.SetFloat("_pixelcount", DefaultPixelValue);
            Debug.Log("pixel_value not found; set to default: " + DefaultPixelValue);
        }

        if (!PlayerPrefs.HasKey("audio_value"))
        {
            PlayerPrefs.SetFloat("audio_value", DefaultVolume);
            Debug.Log("audio_value not found; set to default: " + DefaultVolume);
        }
    }

    void Start()
    {
        foreach (var colorChanger in FindObjectsByType<ColorChanger>(FindObjectsSortMode.None))
        {
            colorChanger.LightsState(3, true);
        }

        CacheUIElements();
        UpdateLabels();

        //'audio_value' ei siis oo olemassa, vaan se on 'volume'... mielenkiintosta
        Debug.Log(PlayerPrefs.GetFloat("audio_value"));
    }

    private void CacheUIElements()
    {
        toggles["optionTest"] = GameObject.Find("optionTest").GetComponent<Toggle>();
        sliders["pixel"] = GameObject.Find("pixel").GetComponent<Slider>();
        pixelCountLabel = GameObject.Find("LabelPA").GetComponent<Text>();
    }

    public void InitializeSliderValues()
    {
        foreach (var slider in sliders)
        {
            if (PlayerPrefs.HasKey(slider.Key + "_value"))
            {
                slider.Value.value = PlayerPrefs.GetFloat(slider.Key + "_value");
            }
        }
    }
    
    public void InitializeToggleValues()
    {
        foreach (var toggle in toggles)
        {
            if (PlayerPrefs.HasKey(toggle.Key + "_value"))
            {
                toggle.Value.isOn = PlayerPrefs.GetInt(toggle.Key + "_value") == 1;
            }
        }
    }

    public void UpdateTogglePreference(string toggleName)
    {
        if (toggles.TryGetValue(toggleName, out var toggle))
        {
            PlayerPrefs.SetInt(toggleName + "_value", toggle.isOn ? 1 : 0);
            if (toggleName == "optionTest")
            {
                foreach (var colorChanger in FindObjectsByType<ColorChanger>(FindObjectsSortMode.None))
                {
                    colorChanger.LightsState(3, true);
                }
            }
            PlayerPrefs.Save();
        }
    }

    public void UpdateSliderPreference(string sliderName)
    {
        if (sliders.TryGetValue(sliderName, out var slider))
        {
            PlayerPrefs.SetFloat(sliderName + "_value", slider.value);
            if (sliderName == "pixel")
            {
                pixelCount.SetFloat("_pixelcount", slider.value * PixelMultiplier);
            }
            UpdateLabels();
            PlayerPrefs.Save();
        }
        Debug.Log($"saved: {sliderName}, with value of {slider.value}");
    }

    private void UpdateLabels()
    {
        pixelCountLabel.text = (PlayerPrefs.GetFloat("pixel_value") * PixelMultiplier).ToString();
    }
}