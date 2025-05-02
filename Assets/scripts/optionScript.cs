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
    private const float DefaultVolume = 0.5f; 

    void OnEnable()
    {
        if (!PlayerPrefs.HasKey("pixel_value"))
        {
            pixelCount.SetFloat("_pixelcount", DefaultPixelValue);
            Debug.Log("pixel_value not found; set to default: " + DefaultPixelValue);
        }

        if (!PlayerPrefs.HasKey("volume"))
        {
            PlayerPrefs.SetFloat("volume", DefaultVolume);
            Debug.Log("volume not found; set to default: " + DefaultVolume);
        }
    }

    void Start()
    {
        CacheUIElements();
        InitializeSliderValues();
        UpdatePixelCountLabel();
    }

    private void CacheUIElements()
    {
        sliders["pixel"] = GameObject.Find("pixel").GetComponent<Slider>();
        sliders["volume"] = GameObject.Find("volumeSlider").GetComponent<Slider>(); // Cache volume slider
        
        pixelCountLabel = GameObject.Find("LabelPA").GetComponent<Text>();

        
        sliders["volume"].onValueChanged.AddListener(UpdateVolume);
    }

    private void InitializeSliderValues()
    {
        foreach (var slider in sliders)
        {
            if (PlayerPrefs.HasKey(slider.Key + "_value"))
            {
                slider.Value.value = PlayerPrefs.GetFloat(slider.Key + "_value");
            }
        }

        // Initialize volume slider
        if (sliders.ContainsKey("volume"))
        {
            sliders["volume"].value = PlayerPrefs.GetFloat("volume");
        }
    }

    public void UpdateTogglePreference(string toggleName)
    {
        if (toggles.TryGetValue(toggleName, out var toggle))
        {
            PlayerPrefs.SetInt(toggleName + "_value", toggle.isOn ? 1 : 0);
            PlayerPrefs.Save();
            Debug.Log($"Updated {toggleName}_value to {toggle.isOn}");
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
                UpdatePixelCountLabel();
            }
            PlayerPrefs.Save();
        }
    }

    private void UpdatePixelCountLabel()
    {
        pixelCountLabel.text = (PlayerPrefs.GetFloat("pixel_value") * PixelMultiplier).ToString();
    }

    private void UpdateVolume(float value)
    {
        PlayerPrefs.SetFloat("volume", value);
        PlayerPrefs.Save();
        AudioListener.volume = value; 
        Debug.Log($"Volume updated to {value}");
    }
}