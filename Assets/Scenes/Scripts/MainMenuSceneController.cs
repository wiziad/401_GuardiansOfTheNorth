using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
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

    [Header("Auth Popup")]
    [SerializeField] private Vector2 authPanelSize = new(640f, 560f);
    [SerializeField] private Color authOverlayColor = new(0f, 0f, 0f, 0.7f);
    [SerializeField] private Color authPanelColor = new(0.04f, 0.08f, 0.11f, 0.98f);
    [SerializeField] private Color authInputColor = new(0.09f, 0.16f, 0.2f, 0.98f);
    [SerializeField] private Color authPlaceholderColor = new(0.78f, 0.87f, 0.92f, 0.55f);
    [SerializeField] private Color authTextColor = new(0.95f, 0.98f, 1f, 1f);
    [SerializeField] private Color authLinkColor = new(0.62f, 0.89f, 1f, 1f);
    [SerializeField] private string backendBaseUrl = "https://four01-guardiansofthenorth.onrender.com";
    [SerializeField] private string authTokenPlayerPrefsKey = "AuthToken";

    private const string BackObjectName = "BackBg";
    private const string ForestOverlayName = "ForestOverlayBg";
    private const string AmbientShadeName = "AmbientShade";
    private const string LogoObjectName = "LogoTitle";
    private const string ButtonRootName = "MenuButtons";
    private const string AuthModalRootName = "AuthModalRoot";
    private const string LoginRoute = "/api/auth/login";
    private const string RegisterRoute = "/api/auth/register";

    private GameObject authModalRoot;
    private GameObject loginPanel;
    private GameObject signupPanel;
    private GameObject authFooterRow;
    private TextMeshProUGUI authFooterLabel;
    private Button authFooterButton;
    private TMP_InputField loginEmailField;
    private TMP_InputField loginPasswordField;
    private TMP_InputField signupUsernameField;
    private TMP_InputField signupEmailField;
    private TMP_InputField signupPasswordField;
    private TextMeshProUGUI loginStatusText;
    private TextMeshProUGUI signupStatusText;
    private Button loginSubmitButton;
    private Button signupSubmitButton;
    private bool authBusy;

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
        if (!rebuildOnEnable)
        {
            TryAutoRebuild();
        }
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
        BuildAuthPopups(canvasRoot);
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

    private void BuildAuthPopups(RectTransform parent)
    {
        authModalRoot = GetOrCreateChild(parent, AuthModalRootName);
        ClearChildren(authModalRoot.transform);
        RectTransform modalRootRt = authModalRoot.GetComponent<RectTransform>();
        modalRootRt.anchorMin = Vector2.zero;
        modalRootRt.anchorMax = Vector2.one;
        modalRootRt.offsetMin = Vector2.zero;
        modalRootRt.offsetMax = Vector2.zero;
        authModalRoot.transform.SetSiblingIndex(300);

        Image overlay = GetOrAdd<Image>(authModalRoot);
        overlay.color = authOverlayColor;
        overlay.raycastTarget = true;

        loginPanel = BuildLoginPanel((RectTransform)authModalRoot.transform);
        signupPanel = BuildSignupPanel((RectTransform)authModalRoot.transform);
        authFooterRow = BuildFooterSwitch((RectTransform)authModalRoot.transform, "AuthFooterSwitch");

        ShowAuthPanel(false);
    }

    private GameObject BuildLoginPanel(RectTransform parent)
    {
        GameObject panel = BuildAuthPanelShell(parent, "LoginPanel", "LOGIN");
        Transform content = panel.transform.Find("Content");
        if (content == null)
        {
            return panel;
        }

        loginEmailField = CreateInputField(content, "LoginEmail", "Email", false);
        loginPasswordField = CreateInputField(content, "LoginPassword", "Password", true);

        loginSubmitButton = CreateActionButton(content, "LoginConfirmButton", "ENTER REALM", HandleLoginSubmit);
        loginStatusText = CreateStatusText(content, "LoginStatus");

        SetChildOrder(content, "LoginEmail", 0);
        SetChildOrder(content, "LoginPassword", 1);
        SetChildOrder(content, "LoginConfirmButton", 2);
        SetChildOrder(content, "LoginStatus", 3);
        DisableUnexpectedChildren(content, "LoginEmail", "LoginPassword", "LoginConfirmButton", "LoginStatus");

        return panel;
    }

    private GameObject BuildSignupPanel(RectTransform parent)
    {
        GameObject panel = BuildAuthPanelShell(parent, "SignupPanel", "SIGN UP");
        Transform content = panel.transform.Find("Content");
        if (content == null)
        {
            return panel;
        }

        signupUsernameField = CreateInputField(content, "SignupUsername", "Username", false);
        signupEmailField = CreateInputField(content, "SignupEmail", "Email", false);
        signupPasswordField = CreateInputField(content, "SignupPassword", "Password", true);

        signupSubmitButton = CreateActionButton(content, "SignupConfirmButton", "BECOME A GUARDIAN", HandleSignupSubmit);
        signupStatusText = CreateStatusText(content, "SignupStatus");

        SetChildOrder(content, "SignupUsername", 0);
        SetChildOrder(content, "SignupEmail", 1);
        SetChildOrder(content, "SignupPassword", 2);
        SetChildOrder(content, "SignupConfirmButton", 3);
        SetChildOrder(content, "SignupStatus", 4);
        DisableUnexpectedChildren(content, "SignupUsername", "SignupEmail", "SignupPassword", "SignupConfirmButton", "SignupStatus");

        return panel;
    }

    private GameObject BuildAuthPanelShell(RectTransform parent, string panelName, string title)
    {
        GameObject panel = GetOrCreateChild(parent, panelName);
        ClearChildren(panel.transform);
        RectTransform panelRt = panel.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.pivot = new Vector2(0.5f, 0.5f);
        panelRt.anchoredPosition = Vector2.zero;
        panelRt.sizeDelta = authPanelSize;

        Image panelImage = GetOrAdd<Image>(panel);
        panelImage.color = authPanelColor;

        Outline panelOutline = GetOrAdd<Outline>(panel);
        panelOutline.effectColor = buttonBorderColor;
        panelOutline.effectDistance = new Vector2(2f, -2f);

        VerticalLayoutGroup layout = GetOrAdd<VerticalLayoutGroup>(panel);
        layout.padding = new RectOffset(32, 32, 24, 28);
        layout.spacing = 16f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = GetOrAdd<ContentSizeFitter>(panel);
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

        Transform titleRow = EnsureRow(panel.transform, "TitleRow", 74f, 6f);
        CreateTitleText(titleRow, "Title", title);
        CreateSmallGhostButton(titleRow, "CloseButton", "X", HideAuthPanel);

        EnsureContentRoot(panel.transform, "Content");

        return panel;
    }

    private GameObject BuildFooterSwitch(RectTransform parent, string name)
    {
        GameObject footer = GetOrCreateChild(parent, name);
        RectTransform rt = footer.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(authPanelSize.x - 24f, 60f);
        rt.anchoredPosition = new Vector2(0f, -(authPanelSize.y * 0.5f) - 38f);

        GameObject buttonGo = GetOrCreateChild((RectTransform)footer.transform, "FooterButton");
        RectTransform buttonRt = buttonGo.GetComponent<RectTransform>();
        buttonRt.anchorMin = Vector2.zero;
        buttonRt.anchorMax = Vector2.one;
        buttonRt.offsetMin = Vector2.zero;
        buttonRt.offsetMax = Vector2.zero;

        Image buttonImage = GetOrAdd<Image>(buttonGo);
        buttonImage.color = new Color(1f, 1f, 1f, 0f);
        buttonImage.raycastTarget = true;

        authFooterButton = GetOrAdd<Button>(buttonGo);
        authFooterButton.targetGraphic = buttonImage;
        authFooterButton.transition = Selectable.Transition.ColorTint;
        authFooterButton.colors = CreateColorBlock(new Color(1f, 1f, 1f, 0f));

        GameObject labelGo = GetOrCreateChild((RectTransform)buttonGo.transform, "Label");
        RectTransform labelRt = labelGo.GetComponent<RectTransform>();
        labelRt.anchorMin = Vector2.zero;
        labelRt.anchorMax = Vector2.one;
        labelRt.offsetMin = Vector2.zero;
        labelRt.offsetMax = Vector2.zero;

        authFooterLabel = GetOrAdd<TextMeshProUGUI>(labelGo);
        authFooterLabel.alignment = TextAlignmentOptions.Center;
        authFooterLabel.textWrappingMode = TextWrappingModes.NoWrap;
        authFooterLabel.overflowMode = TextOverflowModes.Overflow;
        authFooterLabel.fontSize = 26f;
        authFooterLabel.fontStyle = FontStyles.Bold;
        if (logoFont != null)
        {
            authFooterLabel.font = logoFont;
        }

        return footer;
    }

    private void UpdateFooterSwitch(string prefixText, string linkText, UnityEngine.Events.UnityAction action)
    {
        if (authFooterLabel != null)
        {
            string linkHex = ColorUtility.ToHtmlStringRGB(authLinkColor);
            string normalHex = ColorUtility.ToHtmlStringRGB(authTextColor);
            authFooterLabel.text = $"<color=#{normalHex}>{prefixText}</color> <u><color=#{linkHex}>{linkText}</color></u>";
        }

        if (authFooterButton != null)
        {
            authFooterButton.onClick.RemoveAllListeners();
            if (action != null)
            {
                authFooterButton.onClick.AddListener(action);
            }
        }
    }

    private Transform EnsureRow(Transform parent, string name, float minHeight, float spacing)
    {
        GameObject row = GetOrCreateChild((RectTransform)parent, name);

        HorizontalLayoutGroup layout = GetOrAdd<HorizontalLayoutGroup>(row);
        layout.spacing = spacing;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        LayoutElement element = GetOrAdd<LayoutElement>(row);
        element.minHeight = minHeight;
        element.preferredHeight = minHeight;

        return row.transform;
    }

    private Transform EnsureContentRoot(Transform parent, string name)
    {
        GameObject content = GetOrCreateChild((RectTransform)parent, name);
        VerticalLayoutGroup layout = GetOrAdd<VerticalLayoutGroup>(content);
        layout.spacing = 16f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        LayoutElement element = GetOrAdd<LayoutElement>(content);
        element.flexibleHeight = 1f;

        return content.transform;
    }

    private void EnsureDivider(Transform parent, string name)
    {
        GameObject divider = GetOrCreateChild((RectTransform)parent, name);
        Image image = GetOrAdd<Image>(divider);
        image.color = new Color(buttonBorderColor.r, buttonBorderColor.g, buttonBorderColor.b, 0.4f);

        LayoutElement element = GetOrAdd<LayoutElement>(divider);
        element.minHeight = 2f;
        element.preferredHeight = 2f;
    }

    private void CreateTitleText(Transform parent, string name, string value)
    {
        GameObject titleGo = GetOrCreateChild((RectTransform)parent, name);
        LayoutElement element = GetOrAdd<LayoutElement>(titleGo);
        element.flexibleWidth = 1f;
        element.minWidth = 0f;

        TextMeshProUGUI text = GetOrAdd<TextMeshProUGUI>(titleGo);
        text.text = value;
        text.alignment = TextAlignmentOptions.Left;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.fontSize = 48f;
        text.color = authTextColor;
        text.fontStyle = FontStyles.Bold;
        if (logoFont != null)
        {
            text.font = logoFont;
        }
    }

    private void CreateSmallGhostButton(Transform parent, string name, string label, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonGo = GetOrCreateChild((RectTransform)parent, name);
        RectTransform rt = buttonGo.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(54f, 54f);

        LayoutElement layout = GetOrAdd<LayoutElement>(buttonGo);
        layout.preferredWidth = 54f;
        layout.preferredHeight = 54f;

        Image image = GetOrAdd<Image>(buttonGo);
        image.color = new Color(1f, 1f, 1f, 0.08f);

        Outline outline = GetOrAdd<Outline>(buttonGo);
        outline.effectColor = new Color(1f, 1f, 1f, 0.22f);
        outline.effectDistance = new Vector2(1f, -1f);

        Button button = GetOrAdd<Button>(buttonGo);
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;
        button.colors = CreateColorBlock(image.color);
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);

        GameObject labelGo = GetOrCreateChild((RectTransform)buttonGo.transform, "Label");
        RectTransform labelRt = labelGo.GetComponent<RectTransform>();
        labelRt.anchorMin = Vector2.zero;
        labelRt.anchorMax = Vector2.one;
        labelRt.offsetMin = Vector2.zero;
        labelRt.offsetMax = Vector2.zero;

        TextMeshProUGUI text = GetOrAdd<TextMeshProUGUI>(labelGo);
        text.text = label;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Overflow;
        text.fontSize = 30f;
        text.color = authTextColor;
        text.fontStyle = FontStyles.Bold;
        if (logoFont != null)
        {
            text.font = logoFont;
        }
    }

    private TMP_InputField CreateInputField(Transform parent, string name, string placeholderText, bool isPassword)
    {
        GameObject inputRoot = GetOrCreateChild((RectTransform)parent, name);
        RectTransform rootRt = inputRoot.GetComponent<RectTransform>();
        rootRt.sizeDelta = new Vector2(0f, 72f);

        LayoutElement rootLayout = GetOrAdd<LayoutElement>(inputRoot);
        rootLayout.preferredHeight = 72f;
        rootLayout.minHeight = 72f;

        Image background = GetOrAdd<Image>(inputRoot);
        background.color = authInputColor;

        Outline outline = GetOrAdd<Outline>(inputRoot);
        outline.effectColor = new Color(buttonBorderColor.r, buttonBorderColor.g, buttonBorderColor.b, 0.35f);
        outline.effectDistance = new Vector2(1f, -1f);

        TMP_InputField input = GetOrAdd<TMP_InputField>(inputRoot);
        input.lineType = TMP_InputField.LineType.SingleLine;
        input.contentType = isPassword ? TMP_InputField.ContentType.Password : TMP_InputField.ContentType.Standard;
        input.characterLimit = 120;

        GameObject textArea = GetOrCreateChild((RectTransform)inputRoot.transform, "Text Area");
        RectTransform textAreaRt = textArea.GetComponent<RectTransform>();
        textAreaRt.anchorMin = Vector2.zero;
        textAreaRt.anchorMax = Vector2.one;
        textAreaRt.offsetMin = new Vector2(18f, 12f);
        textAreaRt.offsetMax = new Vector2(-18f, -12f);

        RectMask2D mask = GetOrAdd<RectMask2D>(textArea);
        mask.padding = Vector4.zero;

        GameObject textGo = GetOrCreateChild((RectTransform)textArea.transform, "Text");
        RectTransform textRt = textGo.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        TextMeshProUGUI text = GetOrAdd<TextMeshProUGUI>(textGo);
        text.text = string.Empty;
        text.alignment = TextAlignmentOptions.Left;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Truncate;
        text.fontSize = 30f;
        text.color = authTextColor;
        if (logoFont != null)
        {
            text.font = logoFont;
        }

        GameObject placeholderGo = GetOrCreateChild((RectTransform)textArea.transform, "Placeholder");
        RectTransform placeholderRt = placeholderGo.GetComponent<RectTransform>();
        placeholderRt.anchorMin = Vector2.zero;
        placeholderRt.anchorMax = Vector2.one;
        placeholderRt.offsetMin = Vector2.zero;
        placeholderRt.offsetMax = Vector2.zero;

        TextMeshProUGUI placeholder = GetOrAdd<TextMeshProUGUI>(placeholderGo);
        placeholder.text = placeholderText;
        placeholder.alignment = TextAlignmentOptions.Left;
        placeholder.textWrappingMode = TextWrappingModes.NoWrap;
        placeholder.overflowMode = TextOverflowModes.Truncate;
        placeholder.fontSize = 30f;
        placeholder.color = authPlaceholderColor;
        if (logoFont != null)
        {
            placeholder.font = logoFont;
        }

        input.targetGraphic = background;
        input.textViewport = textAreaRt;
        input.textComponent = text;
        input.placeholder = placeholder;
        input.pointSize = text.fontSize;
        input.readOnly = false;
        input.enabled = true;

        return input;
    }

    private Button CreateActionButton(Transform parent, string name, string label, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonGo = GetOrCreateChild((RectTransform)parent, name);
        RectTransform rt = buttonGo.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0f, 78f);

        LayoutElement layout = GetOrAdd<LayoutElement>(buttonGo);
        layout.preferredHeight = 78f;
        layout.minHeight = 78f;

        Image image = GetOrAdd<Image>(buttonGo);
        image.color = buttonColor;

        Outline outline = GetOrAdd<Outline>(buttonGo);
        outline.effectColor = buttonBorderColor;
        outline.effectDistance = new Vector2(2f, -2f);

        Button button = GetOrAdd<Button>(buttonGo);
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;
        button.colors = CreateColorBlock(buttonColor);
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);

        GameObject labelGo = GetOrCreateChild((RectTransform)buttonGo.transform, "Label");
        RectTransform labelRt = labelGo.GetComponent<RectTransform>();
        labelRt.anchorMin = Vector2.zero;
        labelRt.anchorMax = Vector2.one;
        labelRt.offsetMin = Vector2.zero;
        labelRt.offsetMax = Vector2.zero;

        TextMeshProUGUI text = GetOrAdd<TextMeshProUGUI>(labelGo);
        text.text = label;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Overflow;
        text.fontSize = 36f;
        text.color = buttonTextColor;
        text.fontStyle = FontStyles.Bold;
        if (logoFont != null)
        {
            text.font = logoFont;
        }

        return button;
    }

    private TextMeshProUGUI CreateStatusText(Transform parent, string name)
    {
        GameObject statusGo = GetOrCreateChild((RectTransform)parent, name);
        LayoutElement layout = GetOrAdd<LayoutElement>(statusGo);
        layout.preferredHeight = 0f;
        layout.minHeight = 0f;

        Image statusImage = statusGo.GetComponent<Image>();
        if (statusImage != null)
        {
            DestroyComponentSafe(statusImage);
        }

        Outline statusOutline = statusGo.GetComponent<Outline>();
        if (statusOutline != null)
        {
            DestroyComponentSafe(statusOutline);
        }

        Button statusButton = statusGo.GetComponent<Button>();
        if (statusButton != null)
        {
            DestroyComponentSafe(statusButton);
        }

        TMP_InputField statusInput = statusGo.GetComponent<TMP_InputField>();
        if (statusInput != null)
        {
            DestroyComponentSafe(statusInput);
        }

        TextMeshProUGUI text = GetOrAdd<TextMeshProUGUI>(statusGo);
        text.text = string.Empty;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.fontSize = 20f;
        text.color = new Color(1f, 0.8f, 0.65f, 1f);
        if (logoFont != null)
        {
            text.font = logoFont;
        }

        return text;
    }

    private void CreateSwitchRow(
        Transform parent,
        string rowName,
        string prefixText,
        string linkText,
        UnityEngine.Events.UnityAction action
    )
    {
        Transform row = EnsureRow(parent, rowName, 64f, 0f);

        HorizontalLayoutGroup rowLayout = GetOrAdd<HorizontalLayoutGroup>(row.gameObject);
        rowLayout.childAlignment = TextAnchor.MiddleCenter;
        rowLayout.childControlWidth = true;
        rowLayout.childForceExpandWidth = true;

        string linkHex = ColorUtility.ToHtmlStringRGB(authLinkColor);
        string normalHex = ColorUtility.ToHtmlStringRGB(authTextColor);
        string centeredText = $"<color=#{normalHex}>{prefixText}</color> <u><color=#{linkHex}>{linkText}</color></u>";

        GameObject rowButtonGo = GetOrCreateChild((RectTransform)row, "SwitchButton");
        LayoutElement rowButtonLayout = GetOrAdd<LayoutElement>(rowButtonGo);
        rowButtonLayout.preferredHeight = 56f;

        Image rowButtonImage = GetOrAdd<Image>(rowButtonGo);
        rowButtonImage.color = new Color(1f, 1f, 1f, 0f);
        rowButtonImage.raycastTarget = true;

        Button rowButton = GetOrAdd<Button>(rowButtonGo);
        rowButton.targetGraphic = rowButtonImage;
        rowButton.transition = Selectable.Transition.ColorTint;
        rowButton.colors = CreateColorBlock(new Color(1f, 1f, 1f, 0f));
        rowButton.onClick.RemoveAllListeners();
        rowButton.onClick.AddListener(action);

        GameObject labelGo = GetOrCreateChild((RectTransform)rowButtonGo.transform, "Label");
        RectTransform labelRt = labelGo.GetComponent<RectTransform>();
        labelRt.anchorMin = Vector2.zero;
        labelRt.anchorMax = Vector2.one;
        labelRt.offsetMin = Vector2.zero;
        labelRt.offsetMax = Vector2.zero;

        TextMeshProUGUI label = GetOrAdd<TextMeshProUGUI>(labelGo);
        label.text = centeredText;
        label.alignment = TextAlignmentOptions.Center;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.overflowMode = TextOverflowModes.Overflow;
        label.fontSize = 30f;
        label.fontStyle = FontStyles.Bold;
        if (logoFont != null)
        {
            label.font = logoFont;
        }
    }

    private void ShowAuthPanel(bool show)
    {
        if (authModalRoot != null)
        {
            authModalRoot.SetActive(show);
        }
    }

    private void HideAuthPanel()
    {
        ShowAuthPanel(false);
    }

    private void ShowLoginPanel()
    {
        ShowAuthPanel(true);
        if (loginPanel != null) loginPanel.SetActive(true);
        if (signupPanel != null) signupPanel.SetActive(false);
        if (authFooterRow != null) authFooterRow.SetActive(true);
        UpdateFooterSwitch("Not a guardian?", "Sign up to become one", ShowSignupPanel);
        SetStatus(loginStatusText, string.Empty);
        if (loginEmailField != null) loginEmailField.ActivateInputField();
    }

    private void ShowSignupPanel()
    {
        ShowAuthPanel(true);
        if (loginPanel != null) loginPanel.SetActive(false);
        if (signupPanel != null) signupPanel.SetActive(true);
        if (authFooterRow != null) authFooterRow.SetActive(true);
        UpdateFooterSwitch("Already a guardian?", "Log in", ShowLoginPanel);
        SetStatus(signupStatusText, string.Empty);
        if (signupUsernameField != null) signupUsernameField.ActivateInputField();
    }

    private void HandleLoginSubmit()
    {
        if (authBusy)
        {
            return;
        }

        string email = loginEmailField != null ? loginEmailField.text.Trim() : string.Empty;
        string password = loginPasswordField != null ? loginPasswordField.text : string.Empty;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            if (loginStatusText != null)
            {
                SetStatus(loginStatusText, "Enter email and password.");
            }
            return;
        }

        StartCoroutine(LoginRoutine(email, password));
    }

    private void HandleSignupSubmit()
    {
        if (authBusy)
        {
            return;
        }

        string username = signupUsernameField != null ? signupUsernameField.text.Trim() : string.Empty;
        string email = signupEmailField != null ? signupEmailField.text.Trim() : string.Empty;
        string password = signupPasswordField != null ? signupPasswordField.text : string.Empty;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            if (signupStatusText != null)
            {
                SetStatus(signupStatusText, "Fill username, email, and password.");
            }
            return;
        }

        if (username.Length < 3)
        {
            SetStatus(signupStatusText, "Username must be at least 3 characters.");
            return;
        }

        if (!email.Contains("@"))
        {
            SetStatus(signupStatusText, "Enter a valid email address.");
            return;
        }

        if (password.Length < 6)
        {
            SetStatus(signupStatusText, "Password must be at least 6 characters.");
            return;
        }

        StartCoroutine(SignupRoutine(username, email, password));
    }

    private IEnumerator LoginRoutine(string email, string password)
    {
        SetAuthBusy(true);
        SetStatus(loginStatusText, "Logging in...");

        LoginRequest payload = new()
        {
            email = email,
            password = password
        };

        yield return PostAuthRequest(LoginRoute, JsonUtility.ToJson(payload), result =>
        {
            if (!result.Success)
            {
                if (loginStatusText != null)
                {
                    SetStatus(loginStatusText, result.Message);
                }
                return;
            }

            SaveAuthToken(result.Token);
            HideAuthPanel();

            if (Application.isPlaying && !string.IsNullOrWhiteSpace(gameSceneName))
            {
                SceneManager.LoadScene(gameSceneName);
            }
        });

        SetAuthBusy(false);
    }

    private IEnumerator SignupRoutine(string username, string email, string password)
    {
        SetAuthBusy(true);
        SetStatus(signupStatusText, "Creating guardian account...");

        RegisterRequest payload = new()
        {
            username = username,
            email = email,
            password = password
        };

        yield return PostAuthRequest(RegisterRoute, JsonUtility.ToJson(payload), result =>
        {
            if (!result.Success)
            {
                if (signupStatusText != null)
                {
                    SetStatus(signupStatusText, result.Message);
                }
                return;
            }

            SaveAuthToken(result.Token);
            ShowLoginPanel();

            if (loginStatusText != null)
            {
                SetStatus(loginStatusText, "Account created. Please log in.");
            }

            if (loginEmailField != null)
            {
                loginEmailField.text = email;
            }
            if (loginPasswordField != null)
            {
                loginPasswordField.text = string.Empty;
            }

            if (signupUsernameField != null) signupUsernameField.text = string.Empty;
            if (signupEmailField != null) signupEmailField.text = string.Empty;
            if (signupPasswordField != null) signupPasswordField.text = string.Empty;
        });

        SetAuthBusy(false);
    }

    private IEnumerator PostAuthRequest(string route, string jsonBody, Action<AuthRequestResult> onComplete)
    {
        string url = BuildEndpointUrl(route);
        using UnityWebRequest request = new(url, UnityWebRequest.kHttpVerbPOST);
        byte[] body = Encoding.UTF8.GetBytes(jsonBody);

        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        string responseText = request.downloadHandler != null ? request.downloadHandler.text : string.Empty;
        bool requestSucceeded = request.result == UnityWebRequest.Result.Success;
        bool isHttpOk = request.responseCode >= 200 && request.responseCode < 300;
        bool parsed = TryParseAuthEnvelope(responseText, out AuthEnvelope envelope);

        if (!requestSucceeded || !isHttpOk)
        {
            string message = parsed && !string.IsNullOrWhiteSpace(envelope.message)
                ? envelope.message
                : $"Request failed ({request.responseCode}).";
            if (string.IsNullOrWhiteSpace(message) || message == "Request failed (0).")
            {
                message = string.IsNullOrWhiteSpace(request.error) ? "Could not reach server." : request.error;
            }
            onComplete?.Invoke(new AuthRequestResult(false, message, null));
            yield break;
        }

        if (!parsed)
        {
            onComplete?.Invoke(new AuthRequestResult(false, "Could not read server response.", null));
            yield break;
        }

        if (!envelope.success)
        {
            onComplete?.Invoke(new AuthRequestResult(false, string.IsNullOrWhiteSpace(envelope.message) ? "Request failed." : envelope.message, null));
            yield break;
        }

        if (envelope.data == null || string.IsNullOrWhiteSpace(envelope.data.token))
        {
            onComplete?.Invoke(new AuthRequestResult(false, "Token missing in server response.", null));
            yield break;
        }

        onComplete?.Invoke(new AuthRequestResult(true, string.Empty, envelope.data.token));
    }

    private bool TryParseAuthEnvelope(string json, out AuthEnvelope envelope)
    {
        envelope = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            envelope = JsonUtility.FromJson<AuthEnvelope>(json);
            return envelope != null;
        }
        catch
        {
            return false;
        }
    }

    private string BuildEndpointUrl(string route)
    {
        string baseUrl = (backendBaseUrl ?? string.Empty).Trim();
        if (baseUrl.EndsWith("/"))
        {
            baseUrl = baseUrl.Substring(0, baseUrl.Length - 1);
        }

        if (string.IsNullOrWhiteSpace(route))
        {
            return baseUrl;
        }

        return route.StartsWith("/") ? baseUrl + route : $"{baseUrl}/{route}";
    }

    private void SaveAuthToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return;
        }

        PlayerPrefs.SetString(authTokenPlayerPrefsKey, token);
        PlayerPrefs.Save();
    }

    private void SetAuthBusy(bool busy)
    {
        authBusy = busy;

        if (loginSubmitButton != null)
        {
            loginSubmitButton.interactable = !busy;
        }

        if (signupSubmitButton != null)
        {
            signupSubmitButton.interactable = !busy;
        }
    }

    private void SetStatus(TextMeshProUGUI statusText, string message)
    {
        if (statusText == null)
        {
            return;
        }

        bool hasMessage = !string.IsNullOrWhiteSpace(message);
        statusText.text = hasMessage ? message : string.Empty;

        LayoutElement layout = statusText.GetComponent<LayoutElement>();
        if (layout != null)
        {
            layout.minHeight = hasMessage ? 28f : 0f;
            layout.preferredHeight = hasMessage ? 28f : 0f;
        }
    }

    private static void SetChildOrder(Transform parent, string childName, int order)
    {
        if (parent == null)
        {
            return;
        }

        Transform child = parent.Find(childName);
        if (child != null)
        {
            child.SetSiblingIndex(order);
        }
    }

    private static void DisableUnexpectedChildren(Transform parent, params string[] allowedNames)
    {
        if (parent == null)
        {
            return;
        }

        HashSet<string> allowed = new(allowedNames);
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            bool shouldBeActive = allowed.Contains(child.name);
            child.gameObject.SetActive(shouldBeActive);
        }
    }

    private void ClearChildren(Transform parent)
    {
        if (parent == null)
        {
            return;
        }

        Transform trash = GetOrCreateUiTrashRoot();
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            child.SetParent(trash, false);
            child.gameObject.SetActive(false);
            DestroyComponentSafe(child.gameObject);
        }
    }

    private Transform GetOrCreateUiTrashRoot()
    {
        Transform trash = transform.Find("__UiTrash");
        if (trash != null)
        {
            trash.gameObject.SetActive(false);
            return trash;
        }

        GameObject trashGo = new("__UiTrash");
        trashGo.transform.SetParent(transform, false);
        trashGo.SetActive(false);
        return trashGo.transform;
    }

    private static void DestroyComponentSafe(Component component)
    {
        if (component == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(component);
        }
        else
        {
            DestroyImmediate(component);
        }
    }

    private static void DestroyComponentSafe(GameObject gameObject)
    {
        if (gameObject == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }

    [Serializable]
    private class LoginRequest
    {
        public string email;
        public string password;
    }

    [Serializable]
    private class RegisterRequest
    {
        public string username;
        public string email;
        public string password;
    }

    [Serializable]
    private class AuthEnvelope
    {
        public bool success;
        public string message;
        public AuthData data;
    }

    [Serializable]
    private class AuthData
    {
        public string token;
    }

    private struct AuthRequestResult
    {
        public AuthRequestResult(bool success, string message, string token)
        {
            Success = success;
            Message = message;
            Token = token;
        }

        public bool Success { get; }
        public string Message { get; }
        public string Token { get; }
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
        ShowLoginPanel();
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
