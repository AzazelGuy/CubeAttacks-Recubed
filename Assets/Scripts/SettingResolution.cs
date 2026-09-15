using System.Collections.Generic;
using UnityEngine;
using TMPro; // Needed for the dropdown
using UnityEngine.UI; // neededfor the toggle

public class SettingResolution : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    private Resolution[] resolutions;

    void Start()
    {
        // Detects Supported Res
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResolutionIndex = 0;


        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.width &&
    resolutions[i].height == Screen.height)
            {
                currentResolutionIndex = i;
            }
        }


        // Fills the box with the resolutions
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        // Sycs the current res with the toggle
        fullscreenToggle.isOn = Screen.fullScreen;

        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
    }

    // Change Resolution
    public void SetResolution(int index)
    {
        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }

    // To change resolution
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }
}