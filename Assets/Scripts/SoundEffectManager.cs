using UnityEngine;

public class SoundEffectManager : MonoBehaviour
{
    public static SoundEffectManager instance;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource ambientAudioSource;
    [SerializeField] private AudioClip foodEatenClip;
    [SerializeField] private AudioClip ambientClip;
    [Range(0f, 1f)] [SerializeField] private float ambientVolume = 0.35f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (ambientAudioSource == null)
        {
            AudioSource[] audioSources = GetComponents<AudioSource>();
            if (audioSources.Length > 1)
            {
                ambientAudioSource = audioSources[1];
            }
        }

        if (ambientAudioSource != null)
        {
            ambientAudioSource.playOnAwake = false;
            ambientAudioSource.loop = true;
            ambientAudioSource.spatialBlend = 0f;
        }
    }

    private void OnEnable()
    {
        MusicVolumeSettings.VolumeChanged += HandleMusicVolumeChanged;
        ApplyAmbientVolume();
    }

    private void OnDisable()
    {
        MusicVolumeSettings.VolumeChanged -= HandleMusicVolumeChanged;
    }

    private void Start()
    {
        PlayAmbientLoop();
    }

    public void WindowPopup(AudioClip clip)
    {
        Play(clip);
    }

    public void ExitBackSound(AudioClip clip)
    {
        Play(clip);
    }

    public void FoodEatenSound()
    {
        Play(foodEatenClip);
    }

    public void PromptSound(AudioClip clip)
    {
        Play(clip);
    }

    public void SetAmbientClip(AudioClip clip)
    {
        ambientClip = clip;
        PlayAmbientLoop();
    }

    void Play(AudioClip clip)
    {
        if (audioSource == null || clip == null)
            return;

        audioSource.PlayOneShot(clip);
    }

    void PlayAmbientLoop()
    {
        if (ambientAudioSource == null || ambientClip == null)
            return;

        ambientAudioSource.clip = ambientClip;
        ApplyAmbientVolume();

        if (!ambientAudioSource.isPlaying)
        {
            ambientAudioSource.Play();
        }
    }

    void HandleMusicVolumeChanged(float volume)
    {
        ApplyAmbientVolume();
    }

    void ApplyAmbientVolume()
    {
        if (ambientAudioSource == null)
            return;

        ambientAudioSource.volume = ambientVolume * MusicVolumeSettings.CurrentVolume;
    }

}