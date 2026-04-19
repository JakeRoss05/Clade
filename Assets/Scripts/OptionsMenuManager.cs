using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenuManager : MonoBehaviour
{
    [Header("Graphics")]
    public TMP_Dropdown resolutionDropdown;
    public Slider frameRateSlider;
    public TMP_Text frameRateValueText;
    public TMP_Dropdown qualityDropdown;
    public Toggle fullscreenToggle;

    [Header("Frame Rate Slider")]
    public int minFrameRate = 30;
    public int maxFrameRate = 240;

    [Header("Tabs")]
    public GameObject gameplayTab;
    public GameObject audioTab;
    public GameObject graphicsTab;
    public GameObject accessibilityTab;

    [Header("Default Tab")]
    public bool openGraphicsFirst = false;

    [Header("Behaviour")]
    public bool applyImmediately = true;

    private Resolution[] availableResolutions;

    void Awake()
    {
        PopulateResolutionDropdown();
        ConfigureFrameRateSlider();
        PopulateQualityDropdown();

        SyncCurrentSettingsToUI();
        OpenDefaultTab();
    }

    void OnEnable()
    {
        SyncCurrentSettingsToUI();
    }

    public void ShowDefaultTab()
    {
        OpenDefaultTab();
    }

    void PopulateResolutionDropdown()
    {
        if (resolutionDropdown == null)
            return;

        var filteredResolutions = new List<Resolution>();
        var seenResolutions = new HashSet<string>();

        foreach (Resolution resolution in Screen.resolutions)
        {
            int refreshRate = Mathf.RoundToInt((float)resolution.refreshRateRatio.value);
            if (refreshRate <= 0)
                continue;

            string key = $"{resolution.width}x{resolution.height}@{refreshRate}";
            if (seenResolutions.Add(key))
                filteredResolutions.Add(resolution);
        }

        availableResolutions = filteredResolutions.ToArray();

        var options = new List<TMP_Dropdown.OptionData>();
        int currentIndex = 0;
        int recommendedIndex = -1;
        int detectedWidth = Screen.currentResolution.width;
        int detectedHeight = Screen.currentResolution.height;
        int detectedRefreshRate = Mathf.RoundToInt((float)Screen.currentResolution.refreshRateRatio.value);

        for (int i = 0; i < availableResolutions.Length; i++)
        {
            Resolution resolution = availableResolutions[i];
            int refreshRate = Mathf.RoundToInt((float)resolution.refreshRateRatio.value);
            string optionText = $"{resolution.width} x {resolution.height}";

            if (resolution.width == detectedWidth && resolution.height == detectedHeight)
            {
                if (recommendedIndex == -1)
                    recommendedIndex = i;

                if (refreshRate == detectedRefreshRate)
                    recommendedIndex = i;
            }

            options.Add(new TMP_Dropdown.OptionData(optionText));

            if (resolution.width == detectedWidth &&
                resolution.height == detectedHeight &&
                refreshRate == detectedRefreshRate)
            {
                currentIndex = i;
            }
        }

        if (recommendedIndex >= 0 && recommendedIndex < options.Count)
            options[recommendedIndex].text += " (reccomended)";

        resolutionDropdown.onValueChanged.RemoveListener(SetResolution);
        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.SetValueWithoutNotify(currentIndex);
        resolutionDropdown.RefreshShownValue();
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
    }

    void ConfigureFrameRateSlider()
    {
        if (frameRateSlider == null)
            return;

        if (maxFrameRate < minFrameRate)
            maxFrameRate = minFrameRate;

        frameRateSlider.wholeNumbers = true;
        frameRateSlider.minValue = minFrameRate;
        frameRateSlider.maxValue = maxFrameRate;

        frameRateSlider.onValueChanged.RemoveListener(SetFrameRateSlider);
        frameRateSlider.onValueChanged.AddListener(SetFrameRateSlider);
    }

    void PopulateQualityDropdown()
    {
        if (qualityDropdown == null)
            return;

        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(QualitySettings.names.ToList());
    }

    void SyncCurrentSettingsToUI()
    {
        if (fullscreenToggle != null)
            fullscreenToggle.isOn = Screen.fullScreen;

        if (qualityDropdown != null)
            qualityDropdown.value = Mathf.Clamp(QualitySettings.GetQualityLevel(), 0, QualitySettings.names.Length - 1);

        if (frameRateSlider != null)
        {
            int currentTarget = Application.targetFrameRate > 0 ? Application.targetFrameRate : 60;
            int clampedTarget = Mathf.Clamp(currentTarget, minFrameRate, maxFrameRate);
            frameRateSlider.SetValueWithoutNotify(clampedTarget);
            UpdateFrameRateLabel(clampedTarget);
        }
    }

    public void SetResolution(int index)
    {
        if (availableResolutions == null || index < 0 || index >= availableResolutions.Length)
            return;

        Resolution resolution = availableResolutions[index];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode, resolution.refreshRateRatio);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreenMode = isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;

        if (resolutionDropdown != null && availableResolutions != null && resolutionDropdown.value < availableResolutions.Length)
        {
            Resolution resolution = availableResolutions[resolutionDropdown.value];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode, resolution.refreshRateRatio);
        }
    }

    public void SetFrameRate(int index)
    {
        // Backward-compatible path if a dropdown event is still wired.
        int targetFrameRate = index switch
        {
            0 => 30,
            1 => 60,
            2 => 120,
            _ => 144
        };

        ApplyTargetFrameRate(targetFrameRate);
    }

    public void SetFrameRateSlider(float value)
    {
        int targetFrameRate = Mathf.RoundToInt(value);
        ApplyTargetFrameRate(targetFrameRate);
    }

    void ApplyTargetFrameRate(int targetFrameRate)
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = Mathf.Clamp(targetFrameRate, minFrameRate, maxFrameRate);
        UpdateFrameRateLabel(Application.targetFrameRate);
    }

    void UpdateFrameRateLabel(int frameRate)
    {
        if (frameRateValueText != null)
            frameRateValueText.text = $"{frameRate} FPS";
    }

    public void SetVSync(bool enabled)
    {
        QualitySettings.vSyncCount = enabled ? 1 : 0;

        if (enabled)
            Application.targetFrameRate = -1;
    }

    public void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index, true);
    }

    public void OpenGameplayTab()
    {
        SetActiveTab(gameplayTab);
    }

    public void OpenAudioTab()
    {
        SetActiveTab(audioTab);
    }

    public void OpenGraphicsTab()
    {
        SetActiveTab(graphicsTab);
    }

    public void OpenAccessibilityTab()
    {
        SetActiveTab(accessibilityTab);
    }

    void OpenDefaultTab()
    {
        if (openGraphicsFirst)
        {
            OpenGraphicsTab();
            return;
        }

        if (gameplayTab != null)
        {
            OpenGameplayTab();
            return;
        }

        if (audioTab != null)
        {
            OpenAudioTab();
            return;
        }

        if (accessibilityTab != null)
        {
            OpenAccessibilityTab();
        }
    }

    void SetActiveTab(GameObject activeTab)
    {
        SetTabVisible(gameplayTab, gameplayTab == activeTab);
        SetTabVisible(audioTab, audioTab == activeTab);
        SetTabVisible(graphicsTab, graphicsTab == activeTab);
        SetTabVisible(accessibilityTab, accessibilityTab == activeTab);
    }

    void SetTabVisible(GameObject tab, bool isVisible)
    {
        if (tab != null)
            tab.SetActive(isVisible);
    }
}