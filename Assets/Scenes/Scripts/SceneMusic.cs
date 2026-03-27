using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMusic : MonoBehaviour
{
    public AudioClip sceneMusic;

    void Start()
    {
        if (MusicManager.Instance != null)
        {
            string activeScene = SceneManager.GetActiveScene().name;

            if (activeScene == SceneRoutes.MainMenuScene)
            {
                MusicManager.Instance.SetMainMenuClip(sceneMusic);
                MusicManager.Instance.PlayMusic(sceneMusic);
                return;
            }

            if (activeScene == SceneRoutes.Map1Scene || activeScene == SceneRoutes.Map2Scene)
            {
                AudioClip mapClip = MusicManager.Instance.GetMainMenuClipOr(sceneMusic);
                MusicManager.Instance.PlayMusic(mapClip);
                return;
            }

            MusicManager.Instance.PlayMusic(sceneMusic);
        }
    }
}
