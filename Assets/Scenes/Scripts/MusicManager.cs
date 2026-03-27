using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    private AudioSource audioSource;
    private AudioClip mainMenuClip;

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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
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

    public void SetMainMenuClip(AudioClip clip)
    {
        if (clip != null)
        {
            mainMenuClip = clip;
        }
    }

    public AudioClip GetMainMenuClipOr(AudioClip fallbackClip)
    {
        return mainMenuClip != null ? mainMenuClip : fallbackClip;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == SceneRoutes.Map1Scene || scene.name == SceneRoutes.Map2Scene)
        {
            if (mainMenuClip != null)
            {
                PlayMusic(mainMenuClip);
            }
        }
    }
}
