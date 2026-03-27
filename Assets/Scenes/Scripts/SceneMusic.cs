using UnityEngine;

public class SceneMusic : MonoBehaviour
{
    public AudioClip sceneMusic;

    void Start()
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayMusic(sceneMusic);
        }
    }
}