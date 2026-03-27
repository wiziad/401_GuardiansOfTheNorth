using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MapManager2 : MonoBehaviour
{
    [Header("--- MAP ZOOM (drag MapBackground here) ---")]
    public MapZoom mapZoom;

    [Header("--- PINS ---")]
    public Button albertaPin;
    public Button ontarioPin;
    public Button quebecPin;

    [Header("--- LEVEL PANEL (Alberta - the active level) ---")]
    // In your Hierarchy: LevelPanel > LevelTitle, LevelDescription, PlayButton, CloseButton
    public GameObject      levelPanel;
    public TextMeshProUGUI levelTitleText;        // drag LevelTitle here
    public TextMeshProUGUI levelDescriptionText;  // drag LevelDescription here
    public Button          playButton;            // drag PlayButton here
    public Button          closePanelButton;      // drag CloseButton here

    [Header("--- COMPLETED PANEL (Ontario already done) ---")]
    // In your Hierarchy: CompletedPanel > CompletedButton, CompletedLevelDescription, CompletedLevelTitle
    public GameObject      completedPanel;
    public Button          closeCompletedButton;  // drag CompletedButton here

    [Header("--- LOCKED PANEL 2 (Quebec not available) ---")]
    // In your Hierarchy: LockedPanel2 > LevelTitle3, LockedMessage2, CloseLockedButton2
    public GameObject      lockedPanel2;
    public Button          closeLockedButton2;    // drag CloseLockedButton2 here

    [Header("--- CLICK BLOCKER (optional) ---")]
    public GameObject clickBlocker;

    void Start()
    {
        if (albertaPin     == null) Debug.LogError("MapManager2: albertaPin not assigned!");
        if (ontarioPin     == null) Debug.LogError("MapManager2: ontarioPin not assigned!");
        if (levelPanel     == null) Debug.LogError("MapManager2: levelPanel not assigned!");
        if (completedPanel == null) Debug.LogError("MapManager2: completedPanel not assigned!");

        levelPanel.SetActive(false);
        completedPanel.SetActive(false);
        if (lockedPanel2 != null) lockedPanel2.SetActive(false);

        StripTransition(albertaPin);
        StripTransition(ontarioPin);
        StripTransition(quebecPin);

        albertaPin.onClick.AddListener(OnAlbertaClicked);
        ontarioPin.onClick.AddListener(OnOntarioClicked);

        quebecPin.onClick.AddListener(OnQuebecClicked);

        playButton.onClick.AddListener(OnPlayClicked);
        closePanelButton.onClick.AddListener(CloseLevelPanel);
        closeCompletedButton.onClick.AddListener(CloseCompletedPanel);
        if (closeLockedButton2 != null)
            closeLockedButton2.onClick.AddListener(CloseLockedPanel);
    }

    void StripTransition(Button btn)
    {
        if (btn == null) return;
        btn.transition = Selectable.Transition.None;
    }

    void OnAlbertaClicked()
    {
        // levelTitleText.text       = "Alberta";
        // levelDescriptionText.text =
        //     "Wild boar populations have invaded Alberta's grasslands.\n\n" +
        //     "Track and contain them before breeding season spreads the threat!";

        BlockClicks();

        if (mapZoom != null) mapZoom.ZoomToAlberta(() => levelPanel.SetActive(true));
        else                 levelPanel.SetActive(true);
    }

    void OnOntarioClicked()
    {
        completedPanel.SetActive(true);
    }

    void OnQuebecClicked()
    {
        if (lockedPanel2 != null) lockedPanel2.SetActive(true);
    }

    void OnPlayClicked()
    {
        SceneManager.LoadScene("ScrollIntro2");
    }

    void CloseLevelPanel()
    {
        levelPanel.SetActive(false);
        if (mapZoom != null) mapZoom.ZoomOut(() => AllowClicks());
        else                 AllowClicks();
    }

    void CloseCompletedPanel()
    {
        completedPanel.SetActive(false);
    }

    void CloseLockedPanel()
    {
        if (lockedPanel2 != null) lockedPanel2.SetActive(false);
    }

    void BlockClicks()
    {
        if (clickBlocker != null) clickBlocker.SetActive(true);
    }

    void AllowClicks()
    {
        if (clickBlocker != null) clickBlocker.SetActive(false);
    }
}