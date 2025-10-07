using UnityEngine;

public class SimpleAudioFeedback : MonoBehaviour
{
    [Header("Audio Source")]
    [SerializeField] private AudioSource audioSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip successSound;
    [SerializeField] private AudioClip fadeInSound;
    [SerializeField] private AudioClip fadeOutSound;

    private void Awake()
    {
        if (!audioSource)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (!audioSource)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void PlaySuccessSound()
    {
        PlayClip(successSound);
    }

    public void PlayFadeInSound()
    {
        PlayClip(fadeInSound);
    }

    public void PlayFadeOutSound()
    {
        PlayClip(fadeOutSound);
    }

    private void PlayClip(AudioClip clip)
    {
        if (clip && audioSource)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}