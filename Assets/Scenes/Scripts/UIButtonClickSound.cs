using UnityEngine;
using UnityEngine.UI;

public class UIButtonClickSound : MonoBehaviour
{
    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        
        if (button != null)
        {
            button.onClick.AddListener(PlayClickSound);
        }
    }

    private void PlayClickSound()
    {
        ButtonClickSoundManager.PlayButtonClickSound();
    }
}
