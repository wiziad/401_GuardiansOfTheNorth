using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameOverSceneController : MonoBehaviour
{
    public const string SceneName = "GameOver";
    public const string ScenePath = "Assets/Scenes/GameOver.unity";
    public const string RetryScenePrefKey = "RetrySceneName";

    [SerializeField] private string menuSceneName = "MainMenu";
    [SerializeField] private string darkBackgroundResourcePath = "RuntimeSprites/NightForestBackground";
    [SerializeField] private string darkBackgroundPath =
        "Assets/ThirdParty/ImportedPacks/NightForest/Image without mist.png";

    [SerializeField] public AudioClip buttonClickSound;

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
        // Initialize button click sound
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        
        ButtonClickSoundManager.SetAudioSource(audioSource);
        if (buttonClickSound != null)
            ButtonClickSoundManager.InitializeButtonClickSound(buttonClickSound);
        
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
        image.sprite = LoadSprite(darkBackgroundResourcePath, darkBackgroundPath) ?? MakeFallbackBackgroundSprite();
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

        // Add click sound to this button
        go.AddComponent<UIButtonClickSound>();

        Text text = CreateText(label, rt, 46, Vector2.zero, Color.white);
        text.fontSize = 44;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 24;
        text.resizeTextMaxSize = 44;
        text.raycastTarget = false;
    }

    private void OnPlayAgainPressed()
    {
        if (CloudSaveManager.Instance != null)
        {
            CloudSaveManager.Instance.ClearPendingLoadedSave();
        }

        string retryScene = PlayerPrefs.GetString(RetryScenePrefKey, "Level_01_Test");
        SceneManager.LoadScene(retryScene);
    }

    private void OnQuitPressed()
    {
        string retryScene = PlayerPrefs.GetString(RetryScenePrefKey, SceneRoutes.Level1Scene);

        // Return to the corresponding map instead of quitting.
        if (retryScene == SceneRoutes.Level2Scene)
        {
            SceneRoutes.LoadScene(SceneRoutes.Map1Scene);
            return;
        }

        if (retryScene == SceneRoutes.Level1Scene)
        {
            SceneRoutes.LoadScene(SceneRoutes.Map2Scene);
            return;
        }

        SceneRoutes.LoadScene(SceneRoutes.Map1Scene);
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

    private Sprite LoadSprite(string resourcesPath, string editorAssetPath)
    {
        Texture2D texture = LoadTexture(resourcesPath, editorAssetPath);
        if (texture == null)
        {
            return null;
        }

        return Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            100f
        );
    }

    private Texture2D LoadTexture(string resourcesPath, string editorAssetPath)
    {
        if (!string.IsNullOrWhiteSpace(resourcesPath))
        {
            Texture2D runtimeTexture = Resources.Load<Texture2D>(resourcesPath);
            if (runtimeTexture != null)
            {
                return runtimeTexture;
            }
        }

#if UNITY_EDITOR
        if (!string.IsNullOrWhiteSpace(editorAssetPath))
        {
            Texture2D editorTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(editorAssetPath);
            if (editorTexture != null)
            {
                return editorTexture;
            }
        }
#endif

        return null;
    }

    private Sprite MakeFallbackBackgroundSprite()
    {
        const int width = 256;
        const int height = 256;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color top = new Color(0.06f, 0.11f, 0.19f, 1f);
        Color bottom = new Color(0.01f, 0.03f, 0.08f, 1f);

        for (int y = 0; y < height; y++)
        {
            float t = y / (height - 1f);
            Color row = Color.Lerp(bottom, top, t);
            for (int x = 0; x < width; x++)
            {
                tex.SetPixel(x, y, row);
            }
        }

        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.Apply();

        return Sprite.Create(tex, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 100f);
    }
}
