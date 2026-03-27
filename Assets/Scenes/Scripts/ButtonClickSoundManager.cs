using UnityEngine;
using UnityEngine.UI;

public class ButtonClickSoundManager : MonoBehaviour
{
    public static AudioClip buttonClickSound;
    private static AudioSource globalAudioSource;
    private static bool isInitialized = false;

    public static void InitializeButtonClickSound(AudioClip clip)
    {
        buttonClickSound = clip;

        if (!isInitialized)
        {
            isInitialized = true;
            SetupAllButtons();
        }
    }

    public static void SetupAllButtons()
    {
        Button[] allButtons = FindObjectsOfType<Button>();
        
        foreach (Button button in allButtons)
        {
            // Check if this button already has UIButtonClickSound
            if (button.GetComponent<UIButtonClickSound>() == null)
            {
                button.gameObject.AddComponent<UIButtonClickSound>();
            }
        }
    }

    public static void PlayButtonClickSound()
    {
        if (globalAudioSource != null && buttonClickSound != null)
        {
            globalAudioSource.PlayOneShot(buttonClickSound);
        }
    }

    public static void SetAudioSource(AudioSource source)
    {
        globalAudioSource = source;
    }
}
