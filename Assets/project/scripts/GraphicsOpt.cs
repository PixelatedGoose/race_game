using UnityEngine;
using UnityEngine.UI;
using TMPro; // Import TextMeshPro namespace

public class GraphicsOpt : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown; // Use TMP_Dropdown instead of Dropdown
    public TMP_Dropdown qualityDropdown;    // Use TMP_Dropdown for quality levels
    public Toggle fullscreenToggle;

    private Resolution[] resolutions;

    void Start()
    {
        // Populate resolution dropdown
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        var resolutionOptions = new System.Collections.Generic.List<string>();
        foreach (var res in resolutions)
        {
            resolutionOptions.Add(res.width + " x " + res.height);
        }
        resolutionDropdown.AddOptions(resolutionOptions);

        // Populate quality dropdown
        qualityDropdown.ClearOptions();
        var qualityOptions = new System.Collections.Generic.List<string>(QualitySettings.names);
        qualityDropdown.AddOptions(qualityOptions);

        // Set initial values
        resolutionDropdown.value = System.Array.FindIndex(resolutions, r => r.width == Screen.currentResolution.width && r.height == Screen.currentResolution.height);
        resolutionDropdown.RefreshShownValue();

        qualityDropdown.value = QualitySettings.GetQualityLevel();
        qualityDropdown.RefreshShownValue();

        fullscreenToggle.isOn = Screen.fullScreen;

        // Add listeners
        resolutionDropdown.onValueChanged.AddListener(SetResolutionFromDropdown);
        qualityDropdown.onValueChanged.AddListener(SetQualityFromDropdown);
        fullscreenToggle.onValueChanged.AddListener(ToggleFullscreen);
    }

    public void SetResolutionFromDropdown(int index)
    {
        var resolution = resolutions[index];
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
