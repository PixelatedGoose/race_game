using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class GraphicsOpt : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown; 
    public TMP_Dropdown qualityDropdown;    
    public Toggle fullscreenToggle;
    private Resolution[] resolutions;
    // Filtered array that contains only unique width x height resolutions
    private Resolution[] uniqueResolutions;

    void Start()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        var resolutionOptions = new System.Collections.Generic.List<string>();
        var uniqueResList = new System.Collections.Generic.List<Resolution>();
        var seen = new System.Collections.Generic.HashSet<string>();

        // Build a list of unique width x height combinations (ignore refresh rate duplicates)
        foreach (var res in resolutions)
        {
            string key = res.width + "x" + res.height;
            if (!seen.Contains(key))
            {
                seen.Add(key);
                uniqueResList.Add(res);
                resolutionOptions.Add(res.width + " x " + res.height);
            }
        }

        uniqueResolutions = uniqueResList.ToArray();
        resolutionDropdown.AddOptions(resolutionOptions);

        qualityDropdown.ClearOptions();
        var qualityOptions = new System.Collections.Generic.List<string>(QualitySettings.names);
        qualityDropdown.AddOptions(qualityOptions);

    // Find currently used resolution in the filtered list (match by width & height only)
    int currentIndex = System.Array.FindIndex(uniqueResolutions, r => r.width == Screen.currentResolution.width && r.height == Screen.currentResolution.height);
    resolutionDropdown.value = currentIndex >= 0 ? currentIndex : 0;
        resolutionDropdown.RefreshShownValue();

        qualityDropdown.value = QualitySettings.GetQualityLevel();
        qualityDropdown.RefreshShownValue();

        fullscreenToggle.isOn = Screen.fullScreen;

        resolutionDropdown.onValueChanged.AddListener(SetResolutionFromDropdown);
        qualityDropdown.onValueChanged.AddListener(SetQualityFromDropdown);
        fullscreenToggle.onValueChanged.AddListener(ToggleFullscreen);
    }

    public void SetResolutionFromDropdown(int index)
    {
        // Use the filtered unique resolutions array so indices map correctly to the dropdown
        if (uniqueResolutions == null || index < 0 || index >= uniqueResolutions.Length)
            return;
        var resolution = uniqueResolutions[index];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void SetQualityFromDropdown(int index)
    {
        QualitySettings.SetQualityLevel(index);
    }

    public void ToggleFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }
}
