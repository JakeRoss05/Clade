using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GameplayMusicController : MonoBehaviour
{
    [Header("Music")]
    public AudioClip musicClip;
    [Range(0f, 1f)] public float targetVolume = 0.6f;
    public bool loop = true;
    public float startDelay = 0f;

    [Header("Fade")]
    public float fadeInDuration = 1.5f;

    private AudioSource audioSource;
    private float fadeMultiplier = 1f;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = loop;
    }

    void OnEnable()
    {
        MusicVolumeSettings.VolumeChanged += HandleMusicVolumeChanged;
        ApplyVolume();
    }

    void OnDisable()
    {
        MusicVolumeSettings.VolumeChanged -= HandleMusicVolumeChanged;
    }

    void Start()
    {
        if (musicClip == null)
        {
            Debug.LogWarning("GameplayMusicController: No music clip assigned.");
            return;
        }

        audioSource.clip = musicClip;
        StartCoroutine(PlayWithFadeIn());
    }

    void HandleMusicVolumeChanged(float volume)
    {
        ApplyVolume();
    }

    private IEnumerator PlayWithFadeIn()
    {
        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay);

        if (fadeInDuration <= 0f)
        {
            fadeMultiplier = 1f;
            ApplyVolume();
            audioSource.Play();
            yield break;
        }

        fadeMultiplier = 0f;
        ApplyVolume();
        audioSource.Play();

        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            fadeMultiplier = Mathf.Clamp01(elapsed / fadeInDuration);
            ApplyVolume();
            yield return null;
        }

        fadeMultiplier = 1f;
        ApplyVolume();
    }

    void ApplyVolume()
    {
        if (audioSource == null)
            return;

        audioSource.volume = targetVolume * fadeMultiplier * MusicVolumeSettings.CurrentVolume;
    }
}