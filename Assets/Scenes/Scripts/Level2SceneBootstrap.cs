using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteAlways]
public class Level2SceneBootstrap : MonoBehaviour
{
    public static Level2SceneBootstrap Instance { get; private set; }
    public const string Level2SceneName = "Level_02_WaterCleanup";
    public static bool SkipIntroOnce;
    public static bool IntroRedirectConsumed;

    [Header("Preview")]
    [SerializeField] private bool buildInEditMode = true;
    [SerializeField] private bool showIntroBeforeGameplay = true;
    [SerializeField] private string introSceneName = "ScrollIntro2";
    [SerializeField] private bool keepManualSceneObjectsVisible = true;

    [Header("Asset Paths")]
    [SerializeField] private string boatPath = "Assets/ThirdParty/ImportedPacks/Level2/Fishing_Craftpix/3 Objects/Boat.png";
    [SerializeField] private string fallbackBoatPath = "Assets/ThirdParty/ImportedPacks/Level2/Fishing_Craftpix/3 Objects/Boat2.png";
    [SerializeField] private string fallbackPlayerPath = "Assets/Sprites/Player/Idle/Idle_Down.png";

    [Header("Gameplay")]
    [SerializeField] private int invasiveToSpawn = 10;
    [SerializeField] private int trashToSpawn = 8;
    [SerializeField] private float fishingRange = 2.2f;
    [SerializeField] private float levelTimerSeconds = 60f;
    [SerializeField] private int boatCapacity = 5;
    [SerializeField] private Vector2 waterMin = new Vector2(-7.4f, -3.1f);
    [SerializeField] private Vector2 waterMax = new Vector2(7.4f, 3.6f);

    [SerializeField] private string[] invasivePaths =
    {
        "Assets/ThirdParty/ImportedPacks/Level2/Fishing_Craftpix/3 Objects/Grass1.png",
        "Assets/ThirdParty/ImportedPacks/Level2/Fishing_Craftpix/3 Objects/Grass2.png",
        "Assets/ThirdParty/ImportedPacks/Level2/Fishing_Craftpix/3 Objects/Grass3.png",
        "Assets/ThirdParty/ImportedPacks/Level2/Fishing_Craftpix/3 Objects/Grass4.png"
    };

    [SerializeField] private string[] trashPaths =
    {
        "Assets/ThirdParty/ImportedPacks/Level2/Pixel_Mart/banana.png",
        "Assets/ThirdParty/ImportedPacks/Level2/Pixel_Mart/cookies.png",
        "Assets/ThirdParty/ImportedPacks/Level2/Pixel_Mart/cabbage.png",
        "Assets/ThirdParty/ImportedPacks/Level2/Pixel_Mart/strawberry_p.png",
        "Assets/ThirdParty/ImportedPacks/Level2/Pixel_Mart/tuna_can.png",
        "Assets/ThirdParty/ImportedPacks/Level2/Pixel_Mart/milk_bottle.png"
    };

    private readonly Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

    private int totalCollectibles;
    private int invasiveCollected;
    private int trashCollected;

    private GameObject boatObject;
    private Level2BoatController boatController;
    private GameObject playerVisual;
    private readonly List<Vector2> spawnedCollectiblePositions = new List<Vector2>();

    private const string GeneratedRootName = "L2_Generated";
    private bool isBuilding;
    private float nextAutoPreviewTry;
    private bool runtimeBuilt;
    private bool introGateChecked;
    private float timeRemaining;
    private bool levelFailed;
    private bool levelComplete;
    private int currentBoatLoad;
    private bool dockInRange;
    private bool timerStarted;
    private bool showInstructionPopup;
    private float capacityWarningUntil;

    private void Awake()
    {
        Instance = this;
        if (!Application.isPlaying || SceneManager.GetActiveScene().name != Level2SceneName)
        {
            IntroRedirectConsumed = false;
            SkipIntroOnce = false;
        }
    }

    private void OnEnable()
    {
        // Force requested challenge timer regardless of stale scene overrides.
        levelTimerSeconds = 60f;
        runtimeBuilt = false;

        if (Application.isPlaying)
        {
            if (!introGateChecked)
            {
                introGateChecked = true;
                if (ShouldRedirectToIntro())
                {
                    SceneManager.LoadScene(introSceneName);
                    return;
                }
            }

            BuildRuntimeScene();
            runtimeBuilt = true;
            return;
        }

        if (!Application.isPlaying && buildInEditMode && transform.Find(GeneratedRootName) == null)
        {
            RebuildEditorPreview();
        }
    }

    private void OnValidate()
    {
        // Keep preview stable; avoid auto-clearing/rebuilding on every inspector tweak.
    }

    private bool ShouldRedirectToIntro()
    {
        if (!showIntroBeforeGameplay || string.IsNullOrWhiteSpace(introSceneName))
        {
            return false;
        }

        if (SceneManager.GetActiveScene().name != Level2SceneName)
        {
            return false;
        }

        if (IntroRedirectConsumed)
        {
            Debug.Log("Level2SceneBootstrap: intro redirect already consumed; entering gameplay.");
            return false;
        }

        if (SkipIntroOnce)
        {
            SkipIntroOnce = false;
            IntroRedirectConsumed = true;
            Debug.Log("Level2SceneBootstrap: intro bypass consumed; entering gameplay.");
            return false;
        }

        IntroRedirectConsumed = true;
        Debug.Log("Level2SceneBootstrap: redirecting to ScrollIntro2 before gameplay.");
        return true;
    }

    [ContextMenu("Rebuild Preview")]
    public void RebuildEditorPreview()
    {
        if (Application.isPlaying || isBuilding)
        {
            return;
        }

        isBuilding = true;
        try
        {
            ResetCounters();
            ClearGenerated();
            SetupCamera();
            BuildEnvironment();
            BuildBoat(true);
            SpawnCollectibles(true);
        }
        catch (System.Exception ex)
        {
            Debug.LogException(ex, this);
        }
        finally
        {
            isBuilding = false;
        }
    }

    [ContextMenu("Clear Preview")]
    public void ClearEditorPreview()
    {
        if (Application.isPlaying)
        {
            return;
        }

        ClearGenerated();
    }

    private void BuildRuntimeScene()
    {
        ResetCounters();
        playerVisual = FindPlayerVisual();
        if (!keepManualSceneObjectsVisible)
        {
            HideLegacySceneArt();
        }
        SetupCamera();
        ForceClearGeneratedImmediate();
        BuildEnvironment();
        BuildBoat(false);
        SpawnCollectibles(false);
        timeRemaining = Mathf.Max(1f, levelTimerSeconds);
        levelFailed = false;
        levelComplete = false;
        currentBoatLoad = 0;
        dockInRange = false;
        timerStarted = false;
        showInstructionPopup = true;
        capacityWarningUntil = 0f;
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            TryAutoRepairPreview();
            return;
        }

        if (!runtimeBuilt)
        {
            BuildRuntimeScene();
            runtimeBuilt = true;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            TryFishCollectible();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryCompleteAtDock();
        }

        if (!levelFailed && !levelComplete && timerStarted)
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0f)
            {
                timeRemaining = 0f;
                TriggerTimeOutFail();
            }
        }
    }

    private void TryAutoRepairPreview()
    {
        if (!buildInEditMode || isBuilding)
        {
            return;
        }

        if (Time.realtimeSinceStartup < nextAutoPreviewTry)
        {
            return;
        }

        nextAutoPreviewTry = Time.realtimeSinceStartup + 1.5f;

        Transform root = transform.Find(GeneratedRootName);
        if (root == null || root.Find("L2_Boat") == null)
        {
            RebuildEditorPreview();
        }
    }

    private void ResetCounters()
    {
        totalCollectibles = 0;
        invasiveCollected = 0;
        trashCollected = 0;
        spawnedCollectiblePositions.Clear();
        timeRemaining = Mathf.Max(1f, levelTimerSeconds);
        levelFailed = false;
        levelComplete = false;
        currentBoatLoad = 0;
        dockInRange = false;
        timerStarted = false;
        showInstructionPopup = true;
        capacityWarningUntil = 0f;
    }

    private Transform GetGeneratedRoot()
    {
        Transform existing = transform.Find(GeneratedRootName);
        if (existing != null)
        {
            return existing;
        }

        GameObject root = new GameObject(GeneratedRootName);
        root.transform.SetParent(transform, false);
        return root.transform;
    }

    private void ClearGenerated()
    {
        Transform existing = transform.Find(GeneratedRootName);
        if (existing == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(existing.gameObject);
        }
        else
        {
            DestroyImmediate(existing.gameObject);
        }
    }

    private void ForceClearGeneratedImmediate()
    {
        while (true)
        {
            Transform existing = transform.Find(GeneratedRootName);
            if (existing == null)
            {
                break;
            }

            DestroyImmediate(existing.gameObject);
        }
    }

    private void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            return;
        }

        cam.transform.position = new Vector3(0f, 0f, -10f);
        cam.backgroundColor = new Color(0.35f, 0.62f, 0.78f);
        cam.orthographic = true;
        cam.orthographicSize = 5.5f;
        cam.rect = new Rect(0f, 0f, 1f, 1f);

        DisableComponentsByTypeName("PixelPerfectCamera");
        DisableComponentsByTypeName("Volume");
        UpdateBoundsFromCamera(cam);
    }

    private void UpdateBoundsFromCamera(Camera cam)
    {
        if (cam == null || !cam.orthographic)
        {
            return;
        }

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;
        float left = cam.transform.position.x - halfWidth;
        float right = cam.transform.position.x + halfWidth;
        float top = cam.transform.position.y + halfHeight;

        const float edgePadding = 0.25f;
        waterMin.x = left + edgePadding;
        waterMax.x = right - edgePadding;
        waterMax.y = top - edgePadding;
        // Keep bottom bound from inspector so boat still stays in water above beach.
    }

    private void DisableComponentsByTypeName(string typeName)
    {
        Behaviour[] behaviours = Resources.FindObjectsOfTypeAll<Behaviour>();
        foreach (Behaviour behaviour in behaviours)
        {
            if (behaviour == null || behaviour.gameObject == null)
            {
                continue;
            }

            if (!behaviour.gameObject.scene.IsValid())
            {
                continue;
            }

            if (behaviour.GetType().Name == typeName)
            {
                behaviour.enabled = false;
            }
        }
    }

    private GameObject FindPlayerVisual()
    {
        PlayerController controller = FindObjectOfType<PlayerController>();
        if (controller == null)
        {
            return null;
        }

        controller.enabled = false;
        controller.transform.position = Vector3.zero;
        controller.transform.localScale = Vector3.one;
        controller.gameObject.tag = "Untagged";

        SpriteRenderer playerRenderer = controller.GetComponent<SpriteRenderer>();
        if (playerRenderer != null)
        {
            playerRenderer.sortingOrder = 6;
            playerRenderer.enabled = true;
        }

        Rigidbody2D rb = controller.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = false;
        }

        return controller.gameObject;
    }

    private void HideLegacySceneArt()
    {
        foreach (SpriteRenderer renderer in FindObjectsOfType<SpriteRenderer>())
        {
            if (playerVisual != null && renderer.gameObject == playerVisual)
            {
                continue;
            }

            if (renderer.transform.IsChildOf(transform))
            {
                continue;
            }

            renderer.enabled = false;
        }

        foreach (Collider2D col in FindObjectsOfType<Collider2D>())
        {
            if (playerVisual != null && col.transform.IsChildOf(playerVisual.transform))
            {
                continue;
            }

            col.enabled = false;
        }
    }

    private void BuildEnvironment()
    {
        Transform root = GetGeneratedRoot();
        // Overscan to remove border.
        CreateSolidRect("WaterBase", new Vector3(0f, 0f, 0f), new Vector2(26f, 16f), new Color(0.05f, 0.16f, 0.33f), -20, root);
        TryCreateWaterOverlay("WaterOverlay_A", new Vector3(0f, 0.85f, 0f), new Vector2(30f, 6.6f), new Color(0.55f, 0.72f, 0.92f, 0.30f), 0.22f, 10f, -19, root);
        TryCreateWaterOverlay("WaterOverlay_B", new Vector3(0f, 0.45f, 0f), new Vector2(30f, 6.0f), new Color(0.35f, 0.55f, 0.78f, 0.25f), -0.18f, 10f, -18, root);
        CreateSolidRect("GrassUnderlay", new Vector3(0f, -5.55f, 0f), new Vector2(34f, 2.8f), new Color(0.12f, 0.38f, 0.17f), -11, root);
        CreateSolidRect("Grass", new Vector3(0f, -4.4f, 0f), new Vector2(26f, 1.8f), new Color(0.15f, 0.47f, 0.20f), -10, root);
        CreateSolidRect("ShoreEdge", new Vector3(0f, -3.6f, 0f), new Vector2(26f, 0.18f), new Color(0.82f, 0.90f, 0.78f), -9, root);
        BuildBushLayer(root);
        BuildDock(root);
    }

    private void BuildBushLayer(Transform root)
    {
        // Bushy depth across the lower grass band.
        CreateBush("BushA", new Vector3(-8.2f, -4.45f, 0f), new Vector3(2.1f, 1.0f, 1f), root);
        CreateBush("BushB", new Vector3(-5.9f, -4.52f, 0f), new Vector3(1.7f, 0.9f, 1f), root);
        CreateBush("BushC", new Vector3(-3.1f, -4.42f, 0f), new Vector3(2.3f, 1.1f, 1f), root);
        CreateBush("BushD", new Vector3(-0.4f, -4.55f, 0f), new Vector3(1.9f, 0.9f, 1f), root);
        CreateBush("BushE", new Vector3(2.9f, -4.46f, 0f), new Vector3(2.2f, 1.0f, 1f), root);
    }

    private void CreateBush(string name, Vector3 pos, Vector3 scale, Transform root)
    {
        GameObject bush = new GameObject(name);
        bush.transform.SetParent(root, false);
        bush.transform.position = pos;
        bush.transform.localScale = scale;

        SpriteRenderer back = bush.AddComponent<SpriteRenderer>();
        back.sprite = MakeCircleSprite(new Color(0.10f, 0.34f, 0.15f, 1f));
        back.sortingOrder = -8;

        GameObject front = new GameObject("Front");
        front.transform.SetParent(bush.transform, false);
        front.transform.localPosition = new Vector3(0.26f, -0.04f, 0f);
        front.transform.localScale = new Vector3(0.72f, 0.72f, 1f);
        SpriteRenderer frontRenderer = front.AddComponent<SpriteRenderer>();
        frontRenderer.sprite = MakeCircleSprite(new Color(0.18f, 0.52f, 0.21f, 1f));
        frontRenderer.sortingOrder = -7;
    }

    private void BuildDock(Transform root)
    {
        GameObject dock = new GameObject("DockBlock");
        dock.transform.SetParent(root, false);
        dock.transform.position = new Vector3(9.35f, -4.95f, 0f);
        dock.transform.localScale = new Vector3(7.1f, 3.35f, 1f);

        SpriteRenderer dockRenderer = dock.AddComponent<SpriteRenderer>();
        dockRenderer.sprite = MakeRectSprite(new Color(0.53f, 0.32f, 0.17f, 1f));
        dockRenderer.sortingOrder = -7;

        GameObject dockZone = new GameObject("DockZone");
        dockZone.transform.SetParent(dock.transform, false);
        dockZone.transform.localPosition = new Vector3(-1.8f, 2.15f, 0f);

        BoxCollider2D trigger = dockZone.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        trigger.size = new Vector2(4.4f, 4.2f);

        dockZone.AddComponent<Level2DockZone>();
    }

    private void TryCreateWaterOverlay(
        string name,
        Vector3 pos,
        Vector2 size,
        Color tint,
        float speed,
        float wrapWidth,
        int sortingOrder,
        Transform parent)
    {
        try
        {
            CreateWaterOverlay(name, pos, size, tint, speed, wrapWidth, sortingOrder, parent);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Failed to create {name}. Falling back without this layer. {ex.Message}", this);
        }
    }

    private void BuildBoat(bool editPreview)
    {
        Transform root = GetGeneratedRoot();
        Sprite boatSprite = LoadSprite(boatPath);
        if (boatSprite == null)
        {
            boatSprite = LoadSprite(fallbackBoatPath);
            if (boatSprite == null)
            {
                Debug.LogWarning("Level2: Boat sprite missing. Using generated fallback boat.", this);
            }
        }

        boatObject = new GameObject("L2_Boat");
        boatObject.transform.SetParent(root, false);
        boatObject.transform.position = new Vector3(0f, -0.2f, 0f);
        if (TagExists("Player"))
        {
            boatObject.tag = "Player";
        }

        Rigidbody2D rb = boatObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        CircleCollider2D bodyCollider = boatObject.AddComponent<CircleCollider2D>();
        bodyCollider.radius = 0.35f;

        boatController = boatObject.AddComponent<Level2BoatController>();
        boatController.minBounds = waterMin;
        boatController.maxBounds = waterMax;

        GameObject boatVisual = new GameObject("BoatVisual");
        boatVisual.transform.SetParent(boatObject.transform, false);
        boatVisual.transform.localPosition = Vector3.zero;
        boatVisual.transform.localScale = new Vector3(1.12f, 1.12f, 1f);
        SpriteRenderer boatRenderer = boatVisual.AddComponent<SpriteRenderer>();
        boatRenderer.sprite = boatSprite != null ? boatSprite : MakeFallbackBoatSprite();
        boatRenderer.sortingOrder = 8;

        if (playerVisual == null)
        {
            playerVisual = FindPlayerVisual();
        }

        if (editPreview)
        {
            if (playerVisual != null)
            {
                AttachRiderToBoat(playerVisual, boatObject.transform);
            }
            else
            {
                CreatePreviewPlayer(boatObject.transform);
            }
            EnsureSingleBoatRider(boatObject.transform);
            return;
        }

        if (playerVisual != null)
        {
            AttachRiderToBoat(playerVisual, boatObject.transform);
        }
        else
        {
            CreatePreviewPlayer(boatObject.transform);
        }

        EnsureSingleBoatRider(boatObject.transform);
    }

    private bool TagExists(string tagName)
    {
        try
        {
            GameObject probe = new GameObject("TagProbe");
            probe.tag = tagName;
            if (Application.isPlaying)
            {
                Destroy(probe);
            }
            else
            {
                DestroyImmediate(probe);
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    private Sprite MakeFallbackBoatSprite()
    {
        const int width = 64;
        const int height = 24;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color clear = new Color(0f, 0f, 0f, 0f);
        Color hull = new Color(0.68f, 0.42f, 0.24f, 1f);
        Color trim = new Color(0.35f, 0.18f, 0.10f, 1f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                tex.SetPixel(x, y, clear);
            }
        }

        for (int x = 4; x < width - 4; x++)
        {
            for (int y = 6; y < 14; y++)
            {
                tex.SetPixel(x, y, hull);
            }
        }

        for (int x = 6; x < width - 6; x++)
        {
            tex.SetPixel(x, 6, trim);
            tex.SetPixel(x, 13, trim);
        }

        tex.filterMode = FilterMode.Point;
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 32f);
    }

    private void CreatePreviewPlayer(Transform parent)
    {
        Sprite previewSprite = null;

        PlayerController existing = FindObjectOfType<PlayerController>();
        if (existing != null)
        {
            SpriteRenderer sr = existing.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                previewSprite = sr.sprite;
            }
        }

        if (previewSprite == null)
        {
            previewSprite = LoadFirstVisibleSprite(fallbackPlayerPath);
        }

        if (previewSprite == null)
        {
            Debug.LogWarning("Level2: Could not find player sprite for boat rider.", this);
            return;
        }

        RemoveAllPreviewPlayers();

        GameObject preview = new GameObject("PreviewPlayer");
        preview.transform.SetParent(parent, false);
        preview.transform.localPosition = new Vector3(0f, 0.04f, 0f);
        preview.transform.localScale = Vector3.one;

        SpriteRenderer previewRenderer = preview.AddComponent<SpriteRenderer>();
        previewRenderer.sprite = previewSprite;
        previewRenderer.sortingOrder = 7;

        AttachRiderToBoat(preview, parent);
    }

    private void AttachRiderToBoat(GameObject rider, Transform boatParent)
    {
        if (rider == null || boatParent == null)
        {
            return;
        }

        rider.transform.SetParent(boatParent, false);
        rider.transform.localPosition = new Vector3(0f, 0.42f, 0f);

        SpriteRenderer renderer = rider.GetComponent<SpriteRenderer>();
        if (renderer == null || renderer.sprite == null)
        {
            return;
        }

        float spriteHeight = renderer.sprite.bounds.size.y;
        if (spriteHeight <= 0.001f)
        {
            rider.transform.localScale = Vector3.one;
        }
        else
        {
            float targetHeightInBoat = 1.22f;
            float scale = Mathf.Clamp(targetHeightInBoat / spriteHeight, 0.55f, 3.8f);
            rider.transform.localScale = new Vector3(scale, scale, 1f);
        }

        renderer.enabled = true;
        renderer.sortingOrder = 7;
    }

    private Sprite LoadFirstVisibleSprite(string projectRelativePath)
    {
        if (string.IsNullOrWhiteSpace(projectRelativePath))
        {
            return null;
        }

        string key = $"firstvisible::{projectRelativePath}";
        if (spriteCache.TryGetValue(key, out Sprite cached))
        {
            return cached;
        }

        string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? Directory.GetCurrentDirectory();
        string fullPath = Path.Combine(projectRoot, projectRelativePath);
        if (!File.Exists(fullPath))
        {
            return null;
        }

        byte[] bytes = File.ReadAllBytes(fullPath);
        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!texture.LoadImage(bytes))
        {
            return null;
        }

        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        Color32[] pixels = texture.GetPixels32();
        int width = texture.width;
        int height = texture.height;

        bool ColumnHasOpaquePixel(int x)
        {
            for (int y = 0; y < height; y++)
            {
                if (pixels[y * width + x].a > 10)
                {
                    return true;
                }
            }
            return false;
        }

        int minX = -1;
        int maxX = -1;
        for (int x = 0; x < width; x++)
        {
            if (!ColumnHasOpaquePixel(x))
            {
                if (minX >= 0)
                {
                    break;
                }
                continue;
            }

            if (minX < 0)
            {
                minX = x;
            }

            maxX = x;
        }

        if (minX < 0 || maxX < minX)
        {
            return null;
        }

        int minY = height - 1;
        int maxY = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                if (pixels[y * width + x].a > 10)
                {
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }
        }

        if (maxY < minY)
        {
            return null;
        }

        Rect rect = new Rect(minX, minY, (maxX - minX) + 1, (maxY - minY) + 1);
        Sprite sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), 32f);
        spriteCache[key] = sprite;
        return sprite;
    }

    private void RemoveAllPreviewPlayers()
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child == null || child.name != "PreviewPlayer")
            {
                continue;
            }

            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }

    private void EnsureSingleBoatRider(Transform boat)
    {
        if (boat == null)
        {
            return;
        }

        PlayerController[] controllers = FindObjectsOfType<PlayerController>();
        foreach (PlayerController controller in controllers)
        {
            if (controller == null || controller.gameObject == playerVisual)
            {
                continue;
            }

            SpriteRenderer sr = controller.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.enabled = false;
            }
        }

        Transform[] children = GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child == null || child == boat || child == playerVisual?.transform)
            {
                continue;
            }

            if (child.name != "PreviewPlayer")
            {
                continue;
            }

            if (!child.IsChildOf(boat))
            {
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }
    }

    private void SpawnCollectibles(bool editPreview)
    {
        Transform root = GetGeneratedRoot();
        spawnedCollectiblePositions.Clear();

        for (int i = 0; i < invasiveToSpawn; i++)
        {
            SpawnOne(root, invasivePaths, true, "Invasive_" + (i + 1), new Vector3(0.75f, 0.75f, 1f), editPreview);
        }

        for (int i = 0; i < trashToSpawn; i++)
        {
            SpawnOne(root, trashPaths, false, "Trash_" + (i + 1), new Vector3(0.65f, 0.65f, 1f), editPreview);
        }
    }

    private void SpawnOne(Transform root, string[] paths, bool invasive, string name, Vector3 scale, bool editPreview)
    {
        if (paths == null || paths.Length == 0)
        {
            return;
        }

        string path = paths[Random.Range(0, paths.Length)];
        Sprite sprite = LoadSprite(path);
        if (sprite == null)
        {
            return;
        }

        GameObject item = new GameObject(name);
        item.transform.SetParent(root, false);
        float minSpacing = invasive ? 1.05f : 1.55f;
        item.transform.position = PickSpawnPosition(minSpacing);
        item.transform.localScale = scale;

        SpriteRenderer renderer = item.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 3;

        CircleCollider2D trigger = item.AddComponent<CircleCollider2D>();
        trigger.isTrigger = true;
        trigger.radius = 0.35f;

        Level2Collectible collectible = item.AddComponent<Level2Collectible>();
        collectible.isInvasive = invasive;

        if (!editPreview)
        {
            totalCollectibles++;
        }

        spawnedCollectiblePositions.Add(item.transform.position);
    }

    private Vector3 PickSpawnPosition(float minSpacing)
    {
        const int attempts = 64;

        for (int i = 0; i < attempts; i++)
        {
            Vector2 candidate = new Vector2(
                Random.Range(waterMin.x + 0.6f, waterMax.x - 0.6f),
                Random.Range(waterMin.y + 0.6f, waterMax.y - 0.6f)
            );

            bool tooClose = false;
            for (int p = 0; p < spawnedCollectiblePositions.Count; p++)
            {
                if (Vector2.Distance(candidate, spawnedCollectiblePositions[p]) < minSpacing)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
            {
                return new Vector3(candidate.x, candidate.y, 0f);
            }
        }

        return new Vector3(
            Random.Range(waterMin.x + 0.6f, waterMax.x - 0.6f),
            Random.Range(waterMin.y + 0.6f, waterMax.y - 0.6f),
            0f
        );
    }

    private void TryFishCollectible()
    {
        if (!timerStarted || showInstructionPopup)
        {
            return;
        }

        if (currentBoatLoad >= Mathf.Max(1, boatCapacity))
        {
            capacityWarningUntil = Time.time + 2.2f;
            return;
        }

        if (boatObject == null)
        {
            return;
        }

        if (boatController != null)
        {
            boatController.PlayFishingAction();
        }

        Vector2 origin = boatObject.transform.position;
        Vector2 facing = boatController != null ? boatController.LastLookDirection : Vector2.right;
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, fishingRange);

        Level2Collectible best = null;
        float bestScore = float.NegativeInfinity;

        foreach (Collider2D hit in hits)
        {
            Level2Collectible collectible = hit.GetComponent<Level2Collectible>();
            if (collectible == null)
            {
                continue;
            }

            Vector2 toTarget = (Vector2)collectible.transform.position - origin;
            float distanceScore = -toTarget.magnitude;
            float facingScore = Vector2.Dot(facing.normalized, toTarget.normalized) * 2f;
            float totalScore = distanceScore + facingScore;

            if (totalScore > bestScore)
            {
                bestScore = totalScore;
                best = collectible;
            }
        }

        if (best != null)
        {
            Collect(best);
        }
    }

    public void Collect(Level2Collectible collectible)
    {
        if (collectible == null)
        {
            return;
        }

        if (collectible.isInvasive)
        {
            invasiveCollected++;
        }
        else
        {
            trashCollected++;
        }
        currentBoatLoad++;

        Destroy(collectible.gameObject);

        EvaluateCompletionState();
    }

    public void TryCompleteAtDock()
    {
        if (!dockInRange || currentBoatLoad <= 0)
        {
            return;
        }

        currentBoatLoad = 0;
        capacityWarningUntil = 0f;
        EvaluateCompletionState();
    }

    public void SetDockInRange(bool inRange)
    {
        dockInRange = inRange;
    }

    private void TriggerTimeOutFail()
    {
        if (levelFailed)
        {
            return;
        }

        levelFailed = true;
        GameFlow.FailCurrentLevel();
    }

    private void OnGUI()
    {
        if (!Application.isPlaying || levelFailed || levelComplete)
        {
            return;
        }

        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 28,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white },
            alignment = TextAnchor.UpperLeft
        };

        int seconds = Mathf.CeilToInt(timeRemaining);
        string timerText = timerStarted ? $"Time Left: {seconds}s" : "Time Left: READY";
        GUI.Label(new Rect(28f, 20f, 420f, 50f), timerText, style);

        string loadText = $"Load: {currentBoatLoad}/{Mathf.Max(1, boatCapacity)}";
        GUI.Label(new Rect(28f, 62f, 420f, 46f), loadText, style);

        if (Time.time < capacityWarningUntil)
        {
            GUI.Label(new Rect(28f, 146f, 820f, 46f), "Boat full. Go to dock and press E.", style);
        }

        if (showInstructionPopup)
        {
            DrawInstructionPopup();
        }
    }

    private void EvaluateCompletionState()
    {
        bool allCollected = (invasiveCollected + trashCollected) >= totalCollectibles;
        if (!allCollected || currentBoatLoad > 0)
        {
            return;
        }

        levelComplete = true;
        timerStarted = false;
    }

    private void DrawInstructionPopup()
    {
        Color oldColor = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.72f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);

        float width = Mathf.Min(980f, Screen.width - 80f);
        float height = 430f;
        Rect panel = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);

        GUI.color = new Color(0.07f, 0.16f, 0.22f, 0.97f);
        GUI.DrawTexture(panel, Texture2D.whiteTexture);
        GUI.color = oldColor;

        GUIStyle title = new GUIStyle(GUI.skin.label)
        {
            fontSize = 38,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.UpperCenter,
            normal = { textColor = Color.white }
        };

        GUIStyle body = new GUIStyle(GUI.skin.label)
        {
            fontSize = 26,
            wordWrap = true,
            alignment = TextAnchor.UpperLeft,
            normal = { textColor = new Color(0.88f, 0.96f, 1f) }
        };

        GUI.Label(new Rect(panel.x + 24f, panel.y + 20f, panel.width - 48f, 52f), "Level 2 Instructions", title);
        string text =
            $"Collect invasive plants and floating trash with F.\n" +
            $"Boat capacity is {Mathf.Max(1, boatCapacity)} items.\n" +
            "When full, return to the brown dock on the sand and press E to empty.\n" +
            "Collect everything before time runs out.";
        GUI.Label(new Rect(panel.x + 38f, panel.y + 88f, panel.width - 76f, 220f), text, body);

        GUIStyle button = new GUIStyle(GUI.skin.button)
        {
            fontSize = 30,
            fontStyle = FontStyle.Bold
        };

        Rect continueRect = new Rect(panel.x + (panel.width - 240f) * 0.5f, panel.y + panel.height - 86f, 240f, 52f);
        if (GUI.Button(continueRect, "Continue", button))
        {
            showInstructionPopup = false;
            timerStarted = true;
        }
    }

    private Sprite LoadSprite(string projectRelativePath)
    {
        if (string.IsNullOrWhiteSpace(projectRelativePath))
        {
            return null;
        }

        if (spriteCache.TryGetValue(projectRelativePath, out Sprite cached))
        {
            return cached;
        }

        string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? Directory.GetCurrentDirectory();
        string fullPath = Path.Combine(projectRoot, projectRelativePath);
        if (!File.Exists(fullPath))
        {
            return null;
        }

        byte[] bytes = File.ReadAllBytes(fullPath);
        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!texture.LoadImage(bytes))
        {
            return null;
        }

        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            32f
        );

        spriteCache[projectRelativePath] = sprite;
        return sprite;
    }

    private Sprite MakeCircleSprite(Color color)
    {
        string key = "circle_" + ColorUtility.ToHtmlStringRGBA(color);
        if (spriteCache.TryGetValue(key, out Sprite cached))
        {
            return cached;
        }

        const int size = 32;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = size * 0.42f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), center);
                tex.SetPixel(x, y, d <= radius ? color : new Color(0, 0, 0, 0));
            }
        }

        tex.filterMode = FilterMode.Point;
        tex.Apply();

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
        spriteCache[key] = sprite;
        return sprite;
    }

    private Sprite MakeRectSprite(Color color)
    {
        string key = "rect_" + ColorUtility.ToHtmlStringRGBA(color);
        if (spriteCache.TryGetValue(key, out Sprite cached))
        {
            return cached;
        }

        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, color);
        tex.filterMode = FilterMode.Point;
        tex.Apply();

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        spriteCache[key] = sprite;
        return sprite;
    }

    private void CreateSolidRect(string name, Vector3 pos, Vector2 size, Color color, int sortingOrder, Transform parent)
    {
        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, color);
        tex.Apply();
        tex.filterMode = FilterMode.Point;

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);

        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = sortingOrder;
    }

    private void CreateWaterOverlay(
        string name,
        Vector3 pos,
        Vector2 size,
        Color tint,
        float speed,
        float wrapWidth,
        int sortingOrder,
        Transform parent)
    {
        Texture2D tex = MakeWaveTexture(tint);
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 16f);

        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = sortingOrder;
        sr.color = Color.white;

        WaterDrift drift = go.AddComponent<WaterDrift>();
        drift.speed = speed;
        drift.wrapWidth = wrapWidth;
    }

    private Texture2D MakeWaveTexture(Color tint)
    {
        const int width = 128;
        const int height = 64;

        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color clear = new Color(0f, 0f, 0f, 0f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                tex.SetPixel(x, y, clear);
            }
        }

        // Dense pixel highlights to create visible water texture.
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool shortLine = ((x + y * 2) % 19) < 6 && (y % 9 == 2 || y % 9 == 6);
                bool sparkle = ((x * 3 + y * 7) % 61) == 0;

                if (shortLine || sparkle)
                {
                    tex.SetPixel(x, y, tint);
                }
            }
        }

        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Repeat;
        tex.Apply();
        return tex;
    }
}
