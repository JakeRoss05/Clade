using System;
using UnityEngine;

public static class MasterVolumeSettings
{
    const string PlayerPrefsKey = "Clade.MasterVolume";
    const float DefaultVolume = 1f;

    static bool initialized;
    static float masterVolume = DefaultVolume;

    public static event Action<float> VolumeChanged;

    public static float CurrentVolume
    {
        get
        {
            EnsureInitialized();
            return masterVolume;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        EnsureInitialized();
    }

    static void EnsureInitialized()
    {
        if (initialized)
            return;

        initialized = true;
        masterVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(PlayerPrefsKey, DefaultVolume));
        AudioListener.volume = masterVolume;
    }

    public static void SetVolume(float value)
    {
        EnsureInitialized();

        masterVolume = Mathf.Clamp01(value);
        AudioListener.volume = masterVolume;
        PlayerPrefs.SetFloat(PlayerPrefsKey, masterVolume);
        PlayerPrefs.Save();
        VolumeChanged?.Invoke(masterVolume);
    }
}
