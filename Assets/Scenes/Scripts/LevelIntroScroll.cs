using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

// Attach to Canvas. Controls the level intro scroll popup.
public class LevelIntroScroll : MonoBehaviour
{
    [SerializeField] private Animator scrollAnimator;       // scroll's animator
    [SerializeField] private TextMeshProUGUI levelTitleText;   // title text object
    [SerializeField] private TextMeshProUGUI levelContextText; // description text object
    [SerializeField] private GameObject scrollPanel;        // the scroll UI object
    [SerializeField] private LevelIntroData levelData;      // drag the level's data asset here

    void Start()
    {
        scrollPanel.SetActive(false);
        ShowScroll(levelData.levelTitle, levelData.levelContext);
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
        scrollAnimator.SetTrigger("Close");
        SceneManager.LoadScene("Level_01_Test");
    }
}