using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class InstructionsOverlay : MonoBehaviour
{
    [Header("Settings")]
    public float displayTime = 8f;
    public float fadeDuration = 1f;
    public TMP_FontAsset pixelFont;

    private CanvasGroup instructionsGroup;
    private CanvasGroup fadeGroup;
    private TMP_Text skipText;

    private PlayerController playerController;

    void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
            playerController.enabled = false;

        BuildUI();
        StartCoroutine(RunInstructions());
    }

    void BuildUI()
    {
        // ── Root Canvas ───────────────────────────────────────────────────────
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 99;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        gameObject.AddComponent<GraphicRaycaster>();

        // ── Fade Panel ────────────────────────────────────────────────────────
        GameObject fadePanelObj = CreateImage("FadePanel", gameObject, Color.black);
        StretchFull(fadePanelObj.GetComponent<RectTransform>());
        fadeGroup = fadePanelObj.AddComponent<CanvasGroup>();
        fadeGroup.alpha = 1f;

        // ── Instructions Panel (fullscreen) ───────────────────────────────────
        GameObject panelObj = CreateImage("InstructionsPanel", gameObject,
            new Color(0.05f, 0.05f, 0.07f, 0.98f));
        StretchFull(panelObj.GetComponent<RectTransform>());
        instructionsGroup = panelObj.AddComponent<CanvasGroup>();
        instructionsGroup.alpha = 0f;

        // ── Content Container ─────────────────────────────────────────────────
        GameObject content = new GameObject("Content");
        content.transform.SetParent(panelObj.transform, false);
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.pivot = new Vector2(0.5f, 0.5f);
        contentRect.sizeDelta = new Vector2(1000f, 900f);
        contentRect.anchoredPosition = Vector2.zero;

        // ── Title ─────────────────────────────────────────────────────────────
        CreateLabel("HOW TO PLAY", content, 64, Color.white,
            new Vector2(0f, 390f), new Vector2(900f, 80f));

        // ── Subtitle ──────────────────────────────────────────────────────────
        CreateLabel("Master these controls to survive", content, 28,
            new Color(0.55f, 0.55f, 0.55f, 1f),
            new Vector2(0f, 320f), new Vector2(800f, 40f));

        // ── Top Divider ───────────────────────────────────────────────────────
        CreateDivider(content, new Vector2(0f, 280f));

        // ── Control Rows (each key gets its own row) ──────────────────────────
        //        key text       description             y position
        CreateControlRow("A",          "Move Left",         200f, content);
        CreateControlRow("D",          "Move Right",        110f, content);
        CreateControlRow("W",          "Move Up",            20f, content);
        CreateControlRow("S",          "Move Down",         -70f, content);
        CreateControlRow("SPACE",      "Jump",             -160f, content);
        CreateControlRow("LEFT CLICK", "Attack",           -250f, content);
        CreateControlRow("LEFT SHIFT", "Dash",             -340f, content);

        // ── Bottom Divider ────────────────────────────────────────────────────
        CreateDivider(content, new Vector2(0f, -410f));

        // ── Skip Text ─────────────────────────────────────────────────────────
        skipText = CreateLabel("Press any key to start...", content, 28,
            new Color(0.55f, 0.55f, 0.55f, 1f),
            new Vector2(0f, -460f), new Vector2(800f, 45f));
        skipText.gameObject.SetActive(false);
    }

    void CreateControlRow(string keyText, string description, float yPos, GameObject parent)
    {
        // Shadow
        GameObject shadow = CreateImage("Shadow", parent, new Color(0f, 0f, 0f, 0.5f));
        RectTransform shadowRect = shadow.GetComponent<RectTransform>();
        shadowRect.anchorMin = new Vector2(0.5f, 0.5f);
        shadowRect.anchorMax = new Vector2(0.5f, 0.5f);
        shadowRect.sizeDelta = new Vector2(246f, 66f);
        shadowRect.anchoredPosition = new Vector2(-280f, yPos - 4f);

        // Key border
        GameObject keyBorder = CreateImage("KeyBorder", parent,
            new Color(0.45f, 0.45f, 0.5f, 0.9f));
        RectTransform borderRect = keyBorder.GetComponent<RectTransform>();
        borderRect.anchorMin = new Vector2(0.5f, 0.5f);
        borderRect.anchorMax = new Vector2(0.5f, 0.5f);
        borderRect.sizeDelta = new Vector2(244f, 64f);
        borderRect.anchoredPosition = new Vector2(-280f, yPos);

        // Key background
        GameObject keyBox = CreateImage("KeyBox", parent,
            new Color(0.15f, 0.15f, 0.2f, 1f));
        RectTransform keyRect = keyBox.GetComponent<RectTransform>();
        keyRect.anchorMin = new Vector2(0.5f, 0.5f);
        keyRect.anchorMax = new Vector2(0.5f, 0.5f);
        keyRect.sizeDelta = new Vector2(238f, 58f);
        keyRect.anchoredPosition = new Vector2(-280f, yPos);

        // Key label
        TMP_Text keyLabel = CreateLabel(keyText, parent, 26, Color.white,
            new Vector2(-280f, yPos), new Vector2(234f, 58f));
        keyLabel.alignment = TextAlignmentOptions.Center;
        keyLabel.fontStyle = FontStyles.Bold;

        // Arrow
        TMP_Text arrow = CreateLabel("→", parent, 30,
            new Color(0.4f, 0.85f, 0.4f, 1f),
            new Vector2(-100f, yPos), new Vector2(60f, 58f));
        arrow.alignment = TextAlignmentOptions.Center;

        // Description
        TMP_Text desc = CreateLabel(description, parent, 30,
            new Color(0.9f, 0.9f, 0.9f, 1f),
            new Vector2(150f, yPos), new Vector2(500f, 58f));
        desc.alignment = TextAlignmentOptions.Left;
    }

    void CreateDivider(GameObject parent, Vector2 position)
    {
        GameObject divider = CreateImage("Divider", parent,
            new Color(1f, 1f, 1f, 0.12f));
        RectTransform divRect = divider.GetComponent<RectTransform>();
        divRect.anchorMin = new Vector2(0.5f, 0.5f);
        divRect.anchorMax = new Vector2(0.5f, 0.5f);
        divRect.sizeDelta = new Vector2(850f, 2f);
        divRect.anchoredPosition = position;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    GameObject CreateImage(string name, GameObject parent, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent.transform, false);
        Image img = obj.AddComponent<Image>();
        img.color = color;
        return obj;
    }

    TMP_Text CreateLabel(string text, GameObject parent, float fontSize,
        Color color, Vector2 anchoredPos, Vector2 size)
    {
        GameObject obj = new GameObject("Text_" + text);
        obj.transform.SetParent(parent.transform, false);
        TMP_Text tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        if (pixelFont != null) tmp.font = pixelFont;

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = anchoredPos;

        return tmp;
    }

    void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    // ── Sequence ──────────────────────────────────────────────────────────────

    IEnumerator RunInstructions()
    {
        yield return StartCoroutine(Fade(fadeGroup, 1f, 0f, fadeDuration));
        yield return StartCoroutine(Fade(instructionsGroup, 0f, 1f, fadeDuration));

        if (skipText != null)
            skipText.gameObject.SetActive(true);

        float timer = 0f;
        while (timer < displayTime)
        {
            timer += Time.deltaTime;
            if (Input.anyKeyDown) break;
            yield return null;
        }

        if (skipText != null)
            skipText.gameObject.SetActive(false);

        yield return StartCoroutine(Fade(instructionsGroup, 1f, 0f, fadeDuration));
        yield return StartCoroutine(Fade(fadeGroup, 0f, 1f, fadeDuration));

        if (playerController != null)
            playerController.enabled = true;

        yield return StartCoroutine(Fade(fadeGroup, 1f, 0f, fadeDuration));

        Destroy(gameObject);
    }

    IEnumerator Fade(CanvasGroup cg, float from, float to, float duration)
    {
        float timer = 0f;
        cg.alpha = from;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, timer / duration);
            yield return null;
        }
        cg.alpha = to;
    }
}