using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

// Attach to Canvas. Controls the level intro scroll popup.
public class LevelIntroScroll : MonoBehaviour
{
    [SerializeField] public AudioClip buttonClickSound;

    [SerializeField] private Animator scrollAnimator;       // scroll's animator
    [SerializeField] private TextMeshProUGUI levelTitleText;   // title text object
    [SerializeField] private TextMeshProUGUI levelContextText; // description text object
    [SerializeField] private GameObject scrollPanel;        // the scroll UI object
    [SerializeField] private LevelIntroData levelData;      // drag the level's data asset here
    [SerializeField] private string nextSceneName = "Level_01_Test";
    [SerializeField] private string fallbackScenePath = "";

    void Start()
    {
        EnsureEventSystem();
        
        // Initialize button click sound
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        
        ButtonClickSoundManager.SetAudioSource(audioSource);
        if (buttonClickSound != null)
            ButtonClickSoundManager.InitializeButtonClickSound(buttonClickSound);
        
        BindStartButton();
        scrollPanel.SetActive(false);
        ShowScroll(levelData.levelTitle, levelData.levelContext);
    }

    private static void EnsureEventSystem()
    {
        EventSystem existingEventSystem = Object.FindFirstObjectByType<EventSystem>();
        if (existingEventSystem != null && existingEventSystem.GetComponent<BaseInputModule>() != null)
        {
            return;
        }

        GameObject eventSystemObject = existingEventSystem != null
            ? existingEventSystem.gameObject
            : new GameObject("EventSystem");

        if (existingEventSystem == null)
        {
            eventSystemObject.AddComponent<EventSystem>();
        }

#if ENABLE_INPUT_SYSTEM
        if (eventSystemObject.GetComponent<InputSystemUIInputModule>() == null)
        {
            eventSystemObject.AddComponent<InputSystemUIInputModule>();
        }
#else
        if (eventSystemObject.GetComponent<StandaloneInputModule>() == null)
        {
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }
#endif
    }

    private void BindStartButton()
    {
        if (scrollPanel == null)
        {
            return;
        }

        Button startButton = scrollPanel.GetComponentInChildren<Button>(true);
        if (startButton == null)
        {
            return;
        }

        startButton.onClick.RemoveListener(HideScroll);
        startButton.onClick.AddListener(HideScroll);
        
        // Add click sound to this button
        if (startButton.GetComponent<UIButtonClickSound>() == null)
            startButton.gameObject.AddComponent<UIButtonClickSound>();
    }

    public void ShowScroll(string title, string context)
    {
        levelTitleText.text = title;
        levelContextText.text = context;
        scrollPanel.SetActive(true);
    }

    // Hook this up to the Begin Battle button
    public void HideScroll()
    {
        Debug.Log($"LevelIntroScroll: Start pressed. nextSceneName='{nextSceneName}', fallbackScenePath='{fallbackScenePath}'");
        bool goesToLevel2ByName = nextSceneName == Level2SceneBootstrap.Level2SceneName;
        bool goesToLevel2ByPath = !string.IsNullOrWhiteSpace(fallbackScenePath)
            && fallbackScenePath.Contains(Level2SceneBootstrap.Level2SceneName);
        if (goesToLevel2ByName || goesToLevel2ByPath)
        {
            Level2SceneBootstrap.SkipIntroOnce = true;
        }

        if (scrollAnimator != null)
        {
            scrollAnimator.SetTrigger("Close");
        }

        if (CloudSaveManager.Instance != null)
        {
            CloudSaveManager.Instance.SaveCurrentProgress();
        }

        if (!string.IsNullOrWhiteSpace(nextSceneName) &&
            Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
            return;
        }

        if (!string.IsNullOrWhiteSpace(fallbackScenePath))
        {
            SceneManager.LoadScene(fallbackScenePath);
        }
    }
}
