using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ============================================================
//  LevelSidePanel.cs
//  Attach to your Panel UI GameObject.
//
//  HOW TO SET UP THE PANEL IN UNITY:
//  ─────────────────────────────────
//  1. In Hierarchy: right-click Canvas → UI → Panel
//     Name it "LevelSidePanel"
//
//  2. Set its RectTransform:
//     - Anchor: right-centre  (alt+shift click the right-middle preset)
//     - Width: 320, Height: 500
//     - Pivot X: 1, Y: 0.5
//     - Pos X: 0 (when visible), +340 (when hidden off screen)
//
//  3. Inside the panel add these child UI elements:
//
//     "ThumbnailImage"   → UI Image (optional level preview pic)
//     "LevelTitleText"   → TextMeshPro, large bold
//     "ThreatTypeText"   → TextMeshPro, smaller subtitle
//     "DescriptionText"  → TextMeshPro, body text
//     "YesButton"        → Button with green background
//     "NoButton"         → Button with red background
//
//  4. Drag each into the Inspector slots below.
//  5. Set the panel INACTIVE in the Hierarchy at start.
// ============================================================

public class LevelSidePanel : MonoBehaviour
{
    [Header("UI References — drag from Hierarchy")]
    public RectTransform panelRect;
    public Image         thumbnail;
    public TextMeshProUGUI levelTitleText;
    public TextMeshProUGUI threatTypeText;
    public TextMeshProUGUI descriptionText;
    public Button        yesButton;
    public Button        noButton;

    [Header("Slide animation")]
    public float slideDuration = 0.4f;
    public AnimationCurve slideCurve = AnimationCurve.EaseInOut(0,0,1,1);

    private float hiddenX;    // off-screen X position
    private float shownX;     // on-screen X position (0 = flush to right edge)
    private Action onYes;
    private Action onNo;

    void Awake()
    {
        // Panel slides in from the right
        // hiddenX = panel width (fully off screen to the right)
        // shownX  = 0 (anchored to right edge, fully visible)
        shownX  = 0f;
        hiddenX = panelRect.rect.width + 20f;

        // Start hidden
        panelRect.anchoredPosition = new Vector2(hiddenX, panelRect.anchoredPosition.y);
        gameObject.SetActive(false);

        yesButton.onClick.AddListener(OnYesClicked);
        noButton.onClick.AddListener(OnNoClicked);
    }

    // ── Show panel with data ──────────────────────────────────────────────
    public void Show(PinData data, Action yesCallback, Action noCallback)
    {
        onYes = yesCallback;
        onNo  = noCallback;

        // Fill in content
        levelTitleText.text  = data.levelTitle;
        threatTypeText.text  = data.threatType;
        descriptionText.text = data.shortDescription;

        if (thumbnail != null && data.levelThumbnail != null)
            thumbnail.sprite = data.levelThumbnail;

        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(SlideRoutine(hiddenX, shownX));
    }

    // ── Hide panel ────────────────────────────────────────────────────────
    public void Hide()
    {
        StopAllCoroutines();
        StartCoroutine(SlideRoutine(shownX, hiddenX, onComplete: () =>
        {
            gameObject.SetActive(false);
        }));
    }

    IEnumerator SlideRoutine(float fromX, float toX, Action onComplete = null)
    {
        float elapsed = 0f;
        float startY  = panelRect.anchoredPosition.y;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t  = slideCurve.Evaluate(Mathf.Clamp01(elapsed / slideDuration));
            panelRect.anchoredPosition = new Vector2(Mathf.Lerp(fromX, toX, t), startY);
            yield return null;
        }

        panelRect.anchoredPosition = new Vector2(toX, startY);
        onComplete?.Invoke();
    }

    void OnYesClicked() => onYes?.Invoke();
    void OnNoClicked()  => onNo?.Invoke();

    public bool IsVisible() => gameObject.activeSelf;
}
