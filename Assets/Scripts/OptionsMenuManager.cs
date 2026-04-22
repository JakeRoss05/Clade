using System;
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
    public Toggle vsyncToggle;

    [Header("Audio")]
    public Slider masterVolumeSlider;
    public TMP_Text masterVolumeValueText;
    public Slider musicVolumeSlider;
    public TMP_Text musicVolumeValueText;

    [Header("Frame Rate Slider")]
    public int minFrameRate = 30;
    public int maxFrameRate = 240;

    [Header("Tabs")]
    public GameObject gameplayTab;
    public GameObject audioTab;
    public GameObject graphicsTab;
    public GameObject accessibilityTab;

    [Header("Tab Buttons")]
    public Button gameplayTabButton;
    public Button audioTabButton;
    public Button graphicsTabButton;
    public Button accessibilityTabButton;

    [Header("Default Tab")]
    public bool openGraphicsFirst = false;

    [Header("Behaviour")]
    public bool applyImmediately = true;

    private Resolution[] availableResolutions;

    void Awake()
    {
        ResolveTabButtonsIfNeeded();
        ConfigureTabButtons();
        BringTabButtonsToFront();
        PopulateResolutionDropdown();
        ConfigureFrameRateSlider();
        ConfigureVSyncToggle();
        ConfigureAudioSliders();
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
        BringTabButtonsToFront();
        OpenDefaultTab();
    }

    void ResolveTabButtonsIfNeeded()
    {
        if (gameplayTabButton == null)
            gameplayTabButton = FindTabButton("GameplayTab");

        if (audioTabButton == null)
            audioTabButton = FindTabButton("AudioTab");

        if (graphicsTabButton == null)
            graphicsTabButton = FindTabButton("GraphicsTab");

        if (accessibilityTabButton == null)
            accessibilityTabButton = FindTabButton("AccessibilityTab");
    }

    Button FindTabButton(string buttonObjectName)
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);

        foreach (Button button in buttons)
        {
            if (button != null && button.gameObject.name == buttonObjectName)
                return button;
        }

        return null;
    }

    void BringTabButtonsToFront()
    {
        if (gameplayTabButton != null)
            gameplayTabButton.transform.SetAsLastSibling();

        if (audioTabButton != null)
            audioTabButton.transform.SetAsLastSibling();

        if (graphicsTabButton != null)
            graphicsTabButton.transform.SetAsLastSibling();

        if (accessibilityTabButton != null)
            accessibilityTabButton.transform.SetAsLastSibling();
    }

    void ConfigureTabButtons()
    {
        ConfigureSingleTabButton(gameplayTabButton);
        ConfigureSingleTabButton(audioTabButton);
        ConfigureSingleTabButton(graphicsTabButton);
        ConfigureSingleTabButton(accessibilityTabButton);
    }

    void ConfigureSingleTabButton(Button button)
    {
        if (button == null)
            return;

        TMP_Text[] labels = button.GetComponentsInChildren<TMP_Text>(true);
        foreach (TMP_Text label in labels)
        {
            if (label != null)
            {
                label.raycastTarget = false;

                RectTransform labelRect = label.rectTransform;
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = Vector2.zero;
                labelRect.offsetMax = Vector2.zero;
                labelRect.anchoredPosition = Vector2.zero;
                labelRect.localScale = Vector3.one;
            }
        }

        if (button.targetGraphic != null)
            return;

        Image buttonImage = button.gameObject.GetComponent<Image>();
        if (buttonImage == null)
            buttonImage = button.gameObject.AddComponent<Image>();

        buttonImage.color = new Color(1f, 1f, 1f, 0f);
        buttonImage.raycastTarget = true;
        button.targetGraphic = buttonImage;
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

    void ConfigureVSyncToggle()
    {
        ResolveVSyncToggleIfNeeded();

        if (vsyncToggle == null)
            return;

        vsyncToggle.onValueChanged.RemoveListener(SetVSync);
        vsyncToggle.onValueChanged.AddListener(SetVSync);
    }

    void ConfigureAudioSliders()
    {
        ResolveAudioSlidersIfNeeded();

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.wholeNumbers = false;
            masterVolumeSlider.minValue = 0f;
            masterVolumeSlider.maxValue = 1f;
            masterVolumeSlider.onValueChanged.RemoveListener(SetMasterVolume);
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.wholeNumbers = false;
            musicVolumeSlider.minValue = 0f;
            musicVolumeSlider.maxValue = 1f;
            musicVolumeSlider.onValueChanged.RemoveListener(SetMusicVolume);
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        }
    }

    void ResolveAudioSlidersIfNeeded()
    {
        Transform searchRoot = audioTab != null ? audioTab.transform : transform;
        Slider[] sliders = searchRoot.GetComponentsInChildren<Slider>(true);

        var usableSliders = new List<Slider>();
        foreach (Slider slider in sliders)
        {
            if (slider != null && slider != frameRateSlider)
                usableSliders.Add(slider);
        }

        foreach (Slider slider in usableSliders)
        {
            string sliderName = slider.gameObject.name;
            if (masterVolumeSlider == null && sliderName.IndexOf("master", StringComparison.OrdinalIgnoreCase) >= 0)
                masterVolumeSlider = slider;

            if (musicVolumeSlider == null &&
                (sliderName.IndexOf("music", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 sliderName.IndexOf("bgm", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                musicVolumeSlider = slider;
            }
        }

        if (masterVolumeSlider == null && usableSliders.Count > 0)
            masterVolumeSlider = usableSliders[0];

        if (musicVolumeSlider == null)
        {
            foreach (Slider slider in usableSliders)
            {
                if (slider != masterVolumeSlider)
                {
                    musicVolumeSlider = slider;
                    break;
                }
            }
        }

        if (musicVolumeSlider == null)
            musicVolumeSlider = masterVolumeSlider;
    }

    void ResolveVSyncToggleIfNeeded()
    {
        if (vsyncToggle != null)
            return;

        Transform searchRoot = graphicsTab != null ? graphicsTab.transform : transform;
        Toggle[] toggles = searchRoot.GetComponentsInChildren<Toggle>(true);

        foreach (Toggle toggle in toggles)
        {
            if (toggle != null && toggle.gameObject.name.IndexOf("vsync", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                vsyncToggle = toggle;
                return;
            }
        }
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

        if (masterVolumeSlider != null)
        {
            float currentMasterVolume = MasterVolumeSettings.CurrentVolume;
            masterVolumeSlider.SetValueWithoutNotify(currentMasterVolume);
            UpdateMasterVolumeLabel(currentMasterVolume);
        }

        if (musicVolumeSlider != null)
        {
            float currentMusicVolume = MusicVolumeSettings.CurrentVolume;
            musicVolumeSlider.SetValueWithoutNotify(currentMusicVolume);
            UpdateMusicVolumeLabel(currentMusicVolume);
        }

        if (vsyncToggle != null)
            vsyncToggle.SetIsOnWithoutNotify(QualitySettings.vSyncCount > 0);
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

    public void SetMusicVolume(float value)
    {
        MusicVolumeSettings.SetVolume(value);
        UpdateMusicVolumeLabel(value);
    }

    public void SetMasterVolume(float value)
    {
        MasterVolumeSettings.SetVolume(value);
        UpdateMasterVolumeLabel(value);
    }

    void UpdateMasterVolumeLabel(float value)
    {
        if (masterVolumeValueText != null)
            masterVolumeValueText.text = $"{Mathf.RoundToInt(Mathf.Clamp01(value) * 100f)}%";
    }

    void UpdateMusicVolumeLabel(float value)
    {
        if (musicVolumeValueText != null)
            musicVolumeValueText.text = $"{Mathf.RoundToInt(Mathf.Clamp01(value) * 100f)}%";
    }

    public void SetVSync(bool enabled)
    {
        QualitySettings.vSyncCount = enabled ? 1 : 0;

        if (enabled)
            Application.targetFrameRate = -1;
        else if (frameRateSlider != null)
            ApplyTargetFrameRate(Mathf.RoundToInt(frameRateSlider.value));
        else if (Application.targetFrameRate < 0)
            Application.targetFrameRate = 60;
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