using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameOverSceneController : MonoBehaviour
{
    public const string SceneName = "GameOver";
    public const string ScenePath = "Assets/Scenes/GameOver.unity";
    public const string RetryScenePrefKey = "RetrySceneName";

    [SerializeField] private string menuSceneName = "MainMenu";
    [SerializeField] private string darkBackgroundPath =
        "Assets/ThirdParty/ImportedPacks/NightForest/Image without mist.png";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureControllerExists()
    {
        if (SceneManager.GetActiveScene().name != SceneName)
        {
            return;
        }

        if (FindObjectOfType<GameOverSceneController>() != null)
        {
            return;
        }

        GameObject go = new GameObject("GameOverController");
        go.AddComponent<GameOverSceneController>();
    }

    private void Awake()
    {
        BuildUi();
    }

    private void BuildUi()
    {
        EnsureEventSystem();
        Canvas canvas = CreateCanvas();
        BuildBackground(canvas.transform as RectTransform);
        BuildOverlay(canvas.transform as RectTransform);
    }

    private void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        GameObject go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<StandaloneInputModule>();
    }

    private Canvas CreateCanvas()
    {
        GameObject canvasGo = new GameObject("GameOverCanvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private void BuildBackground(RectTransform parent)
    {
        GameObject bg = CreateUiObject("Background", parent);
        Image image = bg.AddComponent<Image>();
        image.color = Color.white;
        image.sprite = LoadSprite(darkBackgroundPath);
        image.type = Image.Type.Sliced;
        image.preserveAspect = false;

        RectTransform rt = bg.GetComponent<RectTransform>();
        Stretch(rt);
    }

    private void BuildOverlay(RectTransform parent)
    {
        GameObject shade = CreateUiObject("Shade", parent);
        Image shadeImage = shade.AddComponent<Image>();
        shadeImage.color = new Color(0f, 0f, 0f, 0.12f);
        Stretch(shade.GetComponent<RectTransform>());

        RectTransform panel = CreateUiObject("Panel", parent).GetComponent<RectTransform>();
        panel.anchorMin = new Vector2(0.5f, 0.5f);
        panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.sizeDelta = new Vector2(900f, 500f);
        panel.anchoredPosition = new Vector2(0f, 20f);

        Image panelImage = panel.gameObject.AddComponent<Image>();
        panelImage.color = new Color(0.05f, 0.04f, 0.09f, 0f);

        Text defeated = CreateText("DEFEATED", panel, 118, new Vector2(0f, 145f), new Color(0.96f, 0.29f, 0.29f));
        defeated.fontStyle = FontStyle.Bold;

        CreateButton("Play Again", panel, new Vector2(0f, -30f), OnPlayAgainPressed);
        CreateButton("Quit", panel, new Vector2(0f, -150f), OnQuitPressed);
    }

    private Text CreateText(string text, RectTransform parent, int fontSize, Vector2 anchoredPos, Color color)
    {
        GameObject go = CreateUiObject(text, parent);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(760f, 140f);
        rt.anchoredPosition = anchoredPos;

        Text label = go.AddComponent<Text>();
        label.text = text;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = fontSize;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = color;
        return label;
    }

    private void CreateButton(string label, RectTransform parent, Vector2 anchoredPos, UnityEngine.Events.UnityAction action)
    {
        GameObject go = CreateUiObject($"{label}Button", parent);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(360f, 86f);
        rt.anchoredPosition = anchoredPos;

        Image image = go.AddComponent<Image>();
        image.color = new Color(0.12f, 0.11f, 0.2f, 0.96f);

        Button button = go.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = image.color;
        colors.highlightedColor = new Color(0.20f, 0.18f, 0.31f, 1f);
        colors.pressedColor = new Color(0.08f, 0.07f, 0.16f, 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;
        button.onClick.AddListener(action);

        Text text = CreateText(label, rt, 46, Vector2.zero, Color.white);
        text.fontSize = 44;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 24;
        text.resizeTextMaxSize = 44;
        text.raycastTarget = false;
    }

    private void OnPlayAgainPressed()
    {
        string retryScene = PlayerPrefs.GetString(RetryScenePrefKey, "Level_01_Test");
        SceneManager.LoadScene(retryScene);
    }

    private void OnQuitPressed()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private GameObject CreateUiObject(string name, RectTransform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.localScale = Vector3.one;
        return go;
    }

    private void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private Sprite LoadSprite(string projectRelativePath)
    {
        if (string.IsNullOrWhiteSpace(projectRelativePath))
        {
            return null;
        }

        string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? Directory.GetCurrentDirectory();
        string fullPath = Path.Combine(projectRoot, projectRelativePath);
        if (!File.Exists(fullPath))
        {
            return null;
        }

        byte[] bytes = File.ReadAllBytes(fullPath);
        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!tex.LoadImage(bytes))
        {
            return null;
        }

        tex.filterMode = FilterMode.Point;
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
    }
}
