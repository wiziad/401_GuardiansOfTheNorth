using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayMusic(AudioClip newClip)
    {
        if (newClip == null) return;

        if (audioSource.clip == newClip && audioSource.isPlaying) return;

        audioSource.clip = newClip;
        audioSource.Play();
    }

    public void StopMusic()
    {
        audioSource.Stop();
    }
}