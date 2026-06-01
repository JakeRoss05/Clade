using UnityEngine;

public class SoundEffectManager : MonoBehaviour
{
    public static SoundEffectManager instance;

    [SerializeField] private AudioSource audioSource;

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
    }

public void WindowPopup(AudioClip clip)
{
    audioSource.PlayOneShot(clip);
}

public void ExitBackSound(AudioClip clip)
{
    audioSource.PlayOneShot(clip);
}

}