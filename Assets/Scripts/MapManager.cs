using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

// Map2: Ontario already complete. Alberta is now unlocked.
// Quebec is still locked (or hidden) until implemented.
public class MapManager : MonoBehaviour
{
    [Header("--- MAP ZOOM (drag MapBackground here) ---")]
    public MapZoom mapZoom;

    [Header("--- PINS ---")]
    public Button ontarioPin;
    public Button albertaPin;
    public Button quebecPin;   // optional - leave empty if not used yet

    [Header("--- LEVEL PANEL (shows for Alberta) ---")]
    public GameObject      levelPanel;
    public TextMeshProUGUI levelTitleText;
    public TextMeshProUGUI levelDescriptionText;
    public Button          playButton;
    public Button          closePanelButton;

    [Header("--- COMPLETED PANEL (shows when Ontario is clicked) ---")]
    public GameObject completedPanel;
    public Button     closeCompletedButton;

    [Header("--- LOCKED PANEL (shows for Quebec) ---")]
    public GameObject lockedPanel;
    public Button     closeLockedButton;

    void Start()
    {
        if (ontarioPin     == null) Debug.LogError("MapManager2: ontarioPin not assigned!");
        if (albertaPin     == null) Debug.LogError("MapManager2: albertaPin not assigned!");
        if (levelPanel     == null) Debug.LogError("MapManager2: levelPanel not assigned!");
        if (completedPanel == null) Debug.LogError("MapManager2: completedPanel not assigned!");

        levelPanel    .SetActive(false);
        completedPanel.SetActive(false);
        if (lockedPanel != null) lockedPanel.SetActive(false);

        StripButtonTransition(ontarioPin);
        StripButtonTransition(albertaPin);
        StripButtonTransition(quebecPin);

        ontarioPin.onClick.AddListener(OnOntarioClicked);
        albertaPin.onClick.AddListener(OnAlbertaClicked);
        if (quebecPin != null)
            quebecPin.onClick.AddListener(OnQuebecClicked);

        playButton          .onClick.AddListener(OnPlayClicked);
        closePanelButton    .onClick.AddListener(CloseLevelPanel);
        closeCompletedButton.onClick.AddListener(CloseCompletedPanel);
        if (closeLockedButton != null)
            closeLockedButton.onClick.AddListener(CloseLockedPanel);
    }

    void StripButtonTransition(Button btn)
    {
        if (btn == null) return;
        btn.transition = Selectable.Transition.None;
    }

    void OnOntarioClicked()
    {
        completedPanel.SetActive(true);
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

    void OnQuebecClicked()
    {
        if (lockedPanel != null) lockedPanel.SetActive(true);
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
        if (lockedPanel != null) lockedPanel.SetActive(false);
    }

    [Header("--- CLICK BLOCKER (optional but recommended) ---")]
    public GameObject clickBlocker;

    void BlockClicks()
    {
        if (clickBlocker != null) clickBlocker.SetActive(true);
    }

    void AllowClicks()
    {
        if (clickBlocker != null) clickBlocker.SetActive(false);
    }
}