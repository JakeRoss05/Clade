using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public static class MusicVolumeSettings
{
    const string PlayerPrefsKey = "Clade.MusicVolume";
    const float DefaultVolume = 0.6f;

    static readonly Dictionary<int, float> baseVolumes = new Dictionary<int, float>();
    static readonly Dictionary<long, float> baseVideoTrackVolumes = new Dictionary<long, float>();

    static bool initialized;
    static float musicVolume = DefaultVolume;

    public static event Action<float> VolumeChanged;

    public static float CurrentVolume
    {
        get
        {
            EnsureInitialized();
            return musicVolume;
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
        musicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(PlayerPrefsKey, DefaultVolume));
        SceneManager.sceneLoaded += HandleSceneLoaded;
        ApplyVolumeToAllMusicSources();
    }

    static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyVolumeToAllMusicSources();
        VolumeChanged?.Invoke(musicVolume);
    }

    public static void SetVolume(float value)
    {
        EnsureInitialized();

        float clampedValue = Mathf.Clamp01(value);
        if (Mathf.Approximately(musicVolume, clampedValue))
        {
            ApplyVolumeToAllMusicSources();
            VolumeChanged?.Invoke(musicVolume);
            return;
        }

        musicVolume = clampedValue;
        PlayerPrefs.SetFloat(PlayerPrefsKey, musicVolume);
        PlayerPrefs.Save();
        ApplyVolumeToAllMusicSources();
        VolumeChanged?.Invoke(musicVolume);
    }

    static void ApplyVolumeToAllMusicSources()
    {
        ApplyVolumeToAudioSources();
        ApplyVolumeToVideoPlayers();
    }

    static void ApplyVolumeToAudioSources()
    {
        AudioSource[] audioSources = UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);

        foreach (AudioSource audioSource in audioSources)
        {
            if (audioSource == null)
                continue;

            if (audioSource.GetComponent<GameplayMusicController>() != null)
                continue;

            if (!IsLikelyMusicSource(audioSource.gameObject.name))
                continue;

            int sourceId = audioSource.GetInstanceID();
            if (!baseVolumes.ContainsKey(sourceId))
                baseVolumes[sourceId] = audioSource.volume;

            audioSource.volume = baseVolumes[sourceId] * musicVolume;
        }
    }

    static void ApplyVolumeToVideoPlayers()
    {
        VideoPlayer[] videoPlayers = UnityEngine.Object.FindObjectsByType<VideoPlayer>(FindObjectsSortMode.None);

        foreach (VideoPlayer videoPlayer in videoPlayers)
        {
            if (videoPlayer == null)
                continue;

            if (!IsLikelyMusicSource(videoPlayer.gameObject.name))
                continue;

            if (videoPlayer.audioOutputMode != VideoAudioOutputMode.Direct)
                continue;

            int trackCount = Mathf.Max(1, (int)videoPlayer.controlledAudioTrackCount);
            for (ushort track = 0; track < trackCount; track++)
            {
                long trackKey = BuildVideoTrackKey(videoPlayer.GetInstanceID(), track);
                if (!baseVideoTrackVolumes.ContainsKey(trackKey))
                    baseVideoTrackVolumes[trackKey] = videoPlayer.GetDirectAudioVolume(track);

                videoPlayer.SetDirectAudioVolume(track, baseVideoTrackVolumes[trackKey] * musicVolume);
            }
        }
    }

    static long BuildVideoTrackKey(int instanceId, ushort track)
    {
        return ((long)instanceId << 16) | track;
    }

    static bool IsLikelyMusicSource(string objectName)
    {
        return objectName.IndexOf("music", StringComparison.OrdinalIgnoreCase) >= 0 ||
               objectName.IndexOf("bgm", StringComparison.OrdinalIgnoreCase) >= 0 ||
               objectName.IndexOf("background", StringComparison.OrdinalIgnoreCase) >= 0 ||
               objectName.IndexOf("video", StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
