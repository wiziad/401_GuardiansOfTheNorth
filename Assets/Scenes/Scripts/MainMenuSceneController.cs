using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

[ExecuteAlways]
public class SimpleMenuArtBuilder : MonoBehaviour
{
    [Header("Auto Build")]
    [SerializeField] private bool rebuildOnEnable = true;
    [SerializeField] private bool rebuildInEditMode = true;
    [SerializeField] private bool clearCanvasBeforeBuild = true;
    [SerializeField] private bool includeLegacyProps = false;

    [Header("Required")]
    [SerializeField] private RectTransform canvasRoot; // Drag MenuCanvas here
    [SerializeField] private Sprite backSprite;        // Drag "back" here
    [SerializeField] private Color backTint = new(1f, 1f, 1f, 1f);
    [SerializeField] private Sprite forestOverlaySprite;
    [SerializeField] private Color forestOverlayTint = new(1f, 1f, 1f, 0f);
    [SerializeField] private string gameSceneName = "Level_01_Test";

    [Header("Workflow")]
    [SerializeField] private bool applyPropSiblingIndex = false;

    [Header("Optional Props")]
    [SerializeField] private List<UiProp> props = new();

    [Header("Logo")]
    [SerializeField] private bool forceCenteredLayout = true;
    [SerializeField] private string logoText = "guardians of the north";
    [SerializeField] private TMP_FontAsset logoFont;
    [SerializeField] private Vector2 logoSize = new(1320f, 220f);
    [SerializeField] private Vector2 logoPosition = new(0f, 210f);
    [SerializeField] private float logoRotationZ = 0f;
    [SerializeField] private float logoCurveAmount = 0f;
    [SerializeField] private float logoFontSize = 128f;
    [SerializeField] private float logoMinFontSize = 64f;
    [SerializeField] private float logoMaxWidthPercent = 0.86f;
    [SerializeField] private Color logoColor = Color.white;
    [SerializeField] private Color logoOutlineColor = Color.black;
    [SerializeField] private float logoOutlineWidth = 0.45f;
    [SerializeField] private Color logoShadowColor = new(0f, 0f, 0f, 0.9f);
    [SerializeField] private Vector2 logoShadowDistance = new(6f, -6f);

    [Header("Card")]
    [SerializeField] private Vector2 cardSize = new(760f, 520f);
    [SerializeField] private Color cardColor = new(0.03f, 0.06f, 0.08f, 0.78f);
    [SerializeField] private Color cardBorderColor = new(0.76f, 0.91f, 0.95f, 0.95f);
    [SerializeField] private Color ambientShadeColor = new(0f, 0f, 0f, 0.42f);
    [SerializeField] private string subtitleText = "A NORTHERN PIXEL SAGA";
    [SerializeField] private Color subtitleColor = new(0.88f, 0.96f, 1f, 0.95f);

    [Header("Buttons")]
    [SerializeField] private Vector2 buttonStackPosition = new(0f, -220f);
    [SerializeField] private Vector2 buttonSize = new(380f, 88f);
    [SerializeField] private Color buttonColor = new(0.09f, 0.24f, 0.31f, 0.96f);
    [SerializeField] private Color buttonBorderColor = new(0.88f, 0.97f, 1f, 1f);
    [SerializeField] private Color buttonTextColor = new(0.98f, 0.99f, 1f, 1f);

    private const string BackObjectName = "BackBg";
    private const string ForestOverlayName = "ForestOverlayBg";
    private const string AmbientShadeName = "AmbientShade";
    private const string LogoObjectName = "LogoTitle";
    private const string ButtonRootName = "MenuButtons";

    [Serializable]
    public class UiProp
    {
        public string id = "Prop";
        public Sprite sprite;
        public Vector2 anchor = new(0.5f, 0.5f);
        public Vector2 anchoredPosition = Vector2.zero;
        public Vector2 size = new(160f, 160f);
        public Color color = Color.white;
        public int siblingIndex = 10;
        public bool preserveAspect = true;
    }

    private void OnEnable()
    {
        TryAutoRebuild();
    }

    private void Start()
    {
        TryAutoRebuild();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying && rebuildOnEnable && rebuildInEditMode)
        {
            Rebuild();
        }
    }
#endif

    [ContextMenu("Rebuild Now")]
    public void Rebuild()
    {
        if (canvasRoot == null) return;

        ConfigureCanvasRoot();
        if (clearCanvasBeforeBuild)
        {
            DeactivateAllCanvasChildren();
        }
        BuildBack();
        BuildAmbientShade();

        if (includeLegacyProps)
        {
            BuildProps();
        }

        BuildLogo(canvasRoot);
        BuildButtons(canvasRoot);
    }

    [ContextMenu("Capture Props From Scene")]
    public void CapturePropsFromScene()
    {
        if (canvasRoot == null) return;

        for (int i = 0; i < props.Count; i++)
        {
            UiProp p = props[i];
            if (p == null) continue;

            string name = string.IsNullOrWhiteSpace(p.id) ? $"Prop_{i}" : p.id;
            Transform t = canvasRoot.Find(name);
            if (t == null) continue;

            RectTransform rt = t.GetComponent<RectTransform>();
            Image img = t.GetComponent<Image>();
            if (rt == null || img == null) continue;

            p.anchor = rt.anchorMin;
            p.anchoredPosition = rt.anchoredPosition;
            p.size = rt.sizeDelta;
            p.siblingIndex = t.GetSiblingIndex();
            p.color = img.color;
            p.preserveAspect = img.preserveAspect;
            p.sprite = img.sprite;
        }
    }

    private void BuildBack()
    {
        if (backSprite == null) return;

        GameObject go = GetOrCreateChild(canvasRoot, BackObjectName);
        RectTransform rt = go.GetComponent<RectTransform>();
        Image img = GetOrAdd<Image>(go);

        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        img.sprite = backSprite;
        img.color = backTint;
        img.preserveAspect = false;
        go.transform.SetAsFirstSibling();

        if (forestOverlaySprite != null)
        {
            GameObject overlay = GetOrCreateChild(canvasRoot, ForestOverlayName);
            RectTransform overlayRt = overlay.GetComponent<RectTransform>();
            Image overlayImg = GetOrAdd<Image>(overlay);

            overlayRt.anchorMin = Vector2.zero;
            overlayRt.anchorMax = Vector2.one;
            overlayRt.offsetMin = Vector2.zero;
            overlayRt.offsetMax = Vector2.zero;

            overlayImg.sprite = forestOverlaySprite;
            overlayImg.color = forestOverlayTint;
            overlayImg.preserveAspect = false;

            overlay.transform.SetSiblingIndex(1);
        }
    }

    private void BuildAmbientShade()
    {
        GameObject go = GetOrCreateChild(canvasRoot, AmbientShadeName);
        RectTransform rt = go.GetComponent<RectTransform>();
        Image img = GetOrAdd<Image>(go);

        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        img.sprite = null;
        img.color = ambientShadeColor;
        go.transform.SetSiblingIndex(2);
    }

    private void ConfigureCanvasRoot()
    {
        Canvas canvas = canvasRoot.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        CanvasScaler scaler = GetOrAdd<CanvasScaler>(canvasRoot.gameObject);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GetOrAdd<GraphicRaycaster>(canvasRoot.gameObject);

        // Force a stable full-screen UI transform each rebuild.
        canvasRoot.localPosition = Vector3.zero;
        canvasRoot.localRotation = Quaternion.identity;
        canvasRoot.localScale = Vector3.one;
        canvasRoot.anchorMin = Vector2.zero;
        canvasRoot.anchorMax = Vector2.one;
        canvasRoot.offsetMin = Vector2.zero;
        canvasRoot.offsetMax = Vector2.zero;
    }

    private void BuildProps()
    {
        for (int i = 0; i < props.Count; i++)
        {
            UiProp p = props[i];
            if (p == null || p.sprite == null) continue;

            string name = string.IsNullOrWhiteSpace(p.id) ? $"Prop_{i}" : p.id;
            GameObject go = GetOrCreateChild(canvasRoot, name);
            RectTransform rt = go.GetComponent<RectTransform>();
            Image img = GetOrAdd<Image>(go);

            rt.anchorMin = p.anchor;
            rt.anchorMax = p.anchor;
            rt.anchoredPosition = p.anchoredPosition;
            rt.sizeDelta = p.size;

            img.sprite = p.sprite;
            img.color = p.color;
            img.preserveAspect = p.preserveAspect;

            if (applyPropSiblingIndex)
            {
                go.transform.SetSiblingIndex(Mathf.Max(1, p.siblingIndex));
            }
        }
    }

    private void BuildLogo(RectTransform parent)
    {
        GameObject go = GetOrCreateChild(parent, LogoObjectName);
        RectTransform rt = go.GetComponent<RectTransform>();
        TextMeshProUGUI text = GetOrAdd<TextMeshProUGUI>(go);

        float canvasHeight = Mathf.Max(100f, canvasRoot.rect.height);
        float safeY = Mathf.Clamp(logoPosition.y, -canvasHeight * 0.42f, canvasHeight * 0.42f);

        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = logoSize;
        rt.anchoredPosition = forceCenteredLayout
            ? new Vector2(0f, safeY)
            : logoPosition;
        rt.localRotation = Quaternion.Euler(0f, 0f, logoRotationZ);
        rt.localScale = Vector3.one;
        rt.SetSiblingIndex(100);

        text.text = logoText;
        text.alignment = TextAlignmentOptions.Center;
        text.margin = Vector4.zero;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Overflow;
        text.fontSize = logoFontSize;
        text.color = logoColor;
        text.fontStyle = FontStyles.Bold;

        if (logoFont != null)
        {
            text.font = logoFont;
        }

        text.outlineColor = logoOutlineColor;
        text.outlineWidth = logoOutlineWidth;
        AutoFitLogo(text);

        Shadow titleShadow = GetOrAdd<Shadow>(go);
        titleShadow.effectColor = logoShadowColor;
        titleShadow.effectDistance = logoShadowDistance;
        titleShadow.useGraphicAlpha = true;

        CurvedTMPText curve = GetOrAdd<CurvedTMPText>(go);
        curve.SetCurveAmount(logoCurveAmount);
        curve.RefreshNow();
    }

    private void BuildButtons(RectTransform parent)
    {
        GameObject root = GetOrCreateChild(parent, ButtonRootName);
        RectTransform rootRt = root.GetComponent<RectTransform>();
        rootRt.anchorMin = new Vector2(0.5f, 0.5f);
        rootRt.anchorMax = new Vector2(0.5f, 0.5f);
        rootRt.pivot = new Vector2(0.5f, 0.5f);
        rootRt.sizeDelta = new Vector2(buttonSize.x, buttonSize.y);
        rootRt.anchoredPosition = forceCenteredLayout ? new Vector2(0f, buttonStackPosition.y) : buttonStackPosition;
        rootRt.SetSiblingIndex(110);

        VerticalLayoutGroup layout = GetOrAdd<VerticalLayoutGroup>(root);
        layout.spacing = 0f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = GetOrAdd<ContentSizeFitter>(root);
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        DisableLegacyButtons(root.transform);
        CreateButton(root.transform, "PlayButton", "PLAY", OnStartPressed);
    }

    private void CreateButton(Transform parent, string objectName, string label, UnityEngine.Events.UnityAction action)
    {
        GameObject go = GetOrCreateChild((RectTransform)parent, objectName);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = buttonSize;

        LayoutElement layout = GetOrAdd<LayoutElement>(go);
        layout.preferredWidth = buttonSize.x;
        layout.preferredHeight = buttonSize.y;
        layout.minHeight = buttonSize.y;

        Image image = GetOrAdd<Image>(go);
        image.color = buttonColor;

        Outline border = GetOrAdd<Outline>(go);
        border.effectColor = buttonBorderColor;
        border.effectDistance = new Vector2(2f, -2f);

        Button button = GetOrAdd<Button>(go);
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;
        button.colors = CreateColorBlock(buttonColor);
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);

        MenuButtonPulse pulse = GetOrAdd<MenuButtonPulse>(go);
        pulse.Configure();

        GameObject textObject = GetOrCreateChild((RectTransform)go.transform, "Label");
        RectTransform textRt = textObject.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        TextMeshProUGUI text = GetOrAdd<TextMeshProUGUI>(textObject);
        text.text = label;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Overflow;
        text.fontSize = 44f;
        text.color = buttonTextColor;
        text.fontStyle = FontStyles.Bold;

        if (logoFont != null)
        {
            text.font = logoFont;
        }

        text.outlineColor = Color.black;
        text.outlineWidth = 0.25f;
    }

    private void AutoFitLogo(TextMeshProUGUI text)
    {
        if (text == null)
        {
            return;
        }

        float canvasWidth = canvasRoot.rect.width;
        if (canvasWidth <= 0f)
        {
            return;
        }

        text.fontSize = logoFontSize;
        text.ForceMeshUpdate();

        float preferred = text.preferredWidth;
        float maxAllowedWidth = canvasWidth * Mathf.Clamp01(logoMaxWidthPercent);

        if (preferred > maxAllowedWidth && preferred > 0f)
        {
            float fitted = logoFontSize * (maxAllowedWidth / preferred);
            text.fontSize = Mathf.Max(logoMinFontSize, fitted);
        }
    }

    private void OnStartPressed()
    {
        if (Application.isPlaying && !string.IsNullOrWhiteSpace(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
        }
    }

    private void OnQuitPressed()
    {
        if (Application.isPlaying)
        {
            Application.Quit();
        }
    }

    private static ColorBlock CreateColorBlock(Color baseColor)
    {
        ColorBlock colors = ColorBlock.defaultColorBlock;
        colors.normalColor = baseColor;
        colors.highlightedColor = Color.Lerp(baseColor, Color.white, 0.2f);
        colors.pressedColor = Color.Lerp(baseColor, Color.black, 0.15f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(baseColor.r, baseColor.g, baseColor.b, 0.5f);
        colors.fadeDuration = 0.1f;
        return colors;
    }

    private static GameObject GetOrCreateChild(RectTransform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
        {
            existing.gameObject.SetActive(true);
            return existing.gameObject;
        }

        GameObject go = new(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private void TryAutoRebuild()
    {
        if (!rebuildOnEnable || canvasRoot == null)
        {
            return;
        }

        if (!Application.isPlaying && !rebuildInEditMode)
        {
            return;
        }

        Rebuild();
    }

    private void DeactivateAllCanvasChildren()
    {
        for (int i = 0; i < canvasRoot.childCount; i++)
        {
            canvasRoot.GetChild(i).gameObject.SetActive(false);
        }
    }

    private static void DisableLegacyButtons(Transform root)
    {
        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            if (child.name != "PlayButton")
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    private static T GetOrAdd<T>(GameObject go) where T : Component
    {
        T c = go.GetComponent<T>();
        return c != null ? c : go.AddComponent<T>();
    }
}

public class CurvedTMPText : MonoBehaviour
{
    [SerializeField] private float curveAmount = 24f;
    private TextMeshProUGUI text;

    public void SetCurveAmount(float value)
    {
        curveAmount = value;
    }

    public void RefreshNow()
    {
        if (text == null)
        {
            text = GetComponent<TextMeshProUGUI>();
        }

        if (text == null)
        {
            return;
        }

        text.ForceMeshUpdate();

        TMP_TextInfo textInfo = text.textInfo;
        float width = text.rectTransform.rect.width;
        if (width <= 0f)
        {
            return;
        }

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible)
            {
                continue;
            }

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;
            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

            float centerX = (vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) * 0.5f;
            float normalized = Mathf.InverseLerp(-width * 0.5f, width * 0.5f, centerX) * 2f - 1f;
            float yOffset = -(normalized * normalized) * curveAmount + curveAmount;

            Vector3 offset = new Vector3(0f, yOffset, 0f);
            vertices[vertexIndex + 0] += offset;
            vertices[vertexIndex + 1] += offset;
            vertices[vertexIndex + 2] += offset;
            vertices[vertexIndex + 3] += offset;
        }

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            text.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }
}

public class MenuButtonPulse : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float idlePulseAmount = 0.035f;
    [SerializeField] private float idlePulseSpeed = 2.5f;
    [SerializeField] private float hoverScale = 1.12f;
    [SerializeField] private float lerpSpeed = 10f;
    [SerializeField] private float hoverBrightness = 0.2f;

    private RectTransform rectTransform;
    private Image image;
    private Color baseColor;
    private Color hoverColor;
    private Vector3 baseScale;
    private bool hovered;

    public void Configure()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        if (image == null)
        {
            image = GetComponent<Image>();
        }

        if (image != null)
        {
            baseColor = image.color;
            hoverColor = Color.Lerp(baseColor, Color.white, Mathf.Clamp01(hoverBrightness));
        }

        baseScale = Vector3.one;
    }

    private void Awake()
    {
        Configure();
    }

    private void Update()
    {
        if (rectTransform == null)
        {
            return;
        }

        float pulse = 1f + Mathf.Sin(Time.unscaledTime * idlePulseSpeed) * idlePulseAmount;
        float target = hovered ? hoverScale : pulse;
        Vector3 targetScale = baseScale * target;
        rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, targetScale, Time.unscaledDeltaTime * lerpSpeed);

        if (image != null)
        {
            Color targetColor = hovered ? hoverColor : baseColor;
            image.color = Color.Lerp(image.color, targetColor, Time.unscaledDeltaTime * lerpSpeed);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovered = false;
    }
}
