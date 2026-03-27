using UnityEngine;
using System.Collections;

public class SequentialAudio : MonoBehaviour
{
    public AudioClip firstClip;
    public AudioClip secondClip;

    private void Start()
    {
        StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        if (MusicManager.Instance == null) yield break;

        //  Stop current scene music
        MusicManager.Instance.StopMusic();

        // Play first clip (non-loop)
        MusicManager.Instance.PlayMusic(firstClip);
        yield return new WaitForSeconds(firstClip.length);

        //  Play second clip (looped music)
        MusicManager.Instance.PlayMusic(secondClip);
    }
}