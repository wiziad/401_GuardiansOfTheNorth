using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class CloudSaveManager : MonoBehaviour
{
    private const string DefaultBaseUrl = "https://four01-guardiansofthenorth.onrender.com";
    private const string AuthTokenKey = "AuthToken";
    private const string SaveIdKey = "CloudSaveId";
    private const string BaseUrlKey = "CloudBackendBaseUrl";
    private const string AutosaveSlotName = "Autosave";
    private const float AutosaveIntervalSeconds = 20f;

    public static CloudSaveManager Instance { get; private set; }

    private string backendBaseUrl = DefaultBaseUrl;
    private string currentSaveId = string.Empty;
    private SaveSlotData pendingLoadedSave;
    private float nextAutosaveAt;
    private bool saveInFlight;
    private bool sceneApplyInFlight;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureExists()
    {
        if (Instance != null)
        {
            return;
        }

        GameObject go = new GameObject(nameof(CloudSaveManager));
        go.AddComponent<CloudSaveManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        backendBaseUrl = PlayerPrefs.GetString(BaseUrlKey, DefaultBaseUrl);
        currentSaveId = PlayerPrefs.GetString(SaveIdKey, string.Empty);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    private void Update()
    {
        if (!HasAuthToken() || string.IsNullOrWhiteSpace(currentSaveId) || saveInFlight)
        {
            return;
        }

        if (Time.unscaledTime < nextAutosaveAt)
        {
            return;
        }

        nextAutosaveAt = Time.unscaledTime + AutosaveIntervalSeconds;
        StartCoroutine(SaveCurrentProgressCoroutine());
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveCurrentProgress();
        }
    }

    private void OnApplicationQuit()
    {
        SaveCurrentProgress();
    }

    public static bool HasSavedAuthToken()
    {
        return !string.IsNullOrWhiteSpace(PlayerPrefs.GetString(AuthTokenKey, string.Empty));
    }

    public void ConfigureBackend(string baseUrl)
    {
        string trimmed = string.IsNullOrWhiteSpace(baseUrl) ? DefaultBaseUrl : baseUrl.Trim();
        if (trimmed.EndsWith("/"))
        {
            trimmed = trimmed.Substring(0, trimmed.Length - 1);
        }

        backendBaseUrl = trimmed;
        PlayerPrefs.SetString(BaseUrlKey, backendBaseUrl);
        PlayerPrefs.Save();
    }

    public IEnumerator BeginAuthenticatedPlay(string baseUrl, string fallbackSceneName, Action<string> onError)
    {
        ConfigureBackend(baseUrl);

        if (!HasAuthToken())
        {
            onError?.Invoke("Please log in first.");
            yield break;
        }

        SaveRequestResult latestSaveResult = SaveRequestResult.Fail("Could not fetch saves.");

        yield return FetchLatestSave(result =>
        {
            latestSaveResult = result;
        });

        if (!latestSaveResult.Success)
        {
            onError?.Invoke(latestSaveResult.Message);
            yield break;
        }

        if (latestSaveResult.List != null && latestSaveResult.List.Length > 0)
        {
            SaveSlotData latest = latestSaveResult.List[0];
            SetCurrentSaveId(latest.id);
            pendingLoadedSave = latest;
            SceneManager.LoadScene(NormalizeSceneName(latest.mapId, fallbackSceneName));
            yield break;
        }

        yield return CreateInitialSaveAndEnterScene(fallbackSceneName, onError);
    }

    public void SaveCurrentProgress()
    {
        string activeSceneName = SceneManager.GetActiveScene().name;
        if (!Application.isPlaying || saveInFlight || !HasAuthToken() || string.IsNullOrWhiteSpace(currentSaveId) || ShouldSkipSceneSave(activeSceneName))
        {
            return;
        }

        StartCoroutine(SaveCurrentProgressCoroutine());
    }

    private IEnumerator CreateInitialSaveAndEnterScene(string fallbackSceneName, Action<string> onError)
    {
        SavePayload payload = BuildCurrentPayload(fallbackSceneName);

        yield return CreateSave(payload, result =>
        {
            if (!result.Success || result.Single == null)
            {
                onError?.Invoke(result.Message);
                return;
            }

            SaveSlotData created = result.Single;
            SetCurrentSaveId(created.id);
            pendingLoadedSave = created;
            SceneManager.LoadScene(NormalizeSceneName(created.mapId, fallbackSceneName));
        });
    }

    private IEnumerator SaveCurrentProgressCoroutine()
    {
        if (saveInFlight)
        {
            yield break;
        }

        if (!HasAuthToken() || string.IsNullOrWhiteSpace(currentSaveId))
        {
            yield break;
        }

        saveInFlight = true;
        SavePayload payload = BuildCurrentPayload(SceneManager.GetActiveScene().name);

        yield return UpdateSave(currentSaveId, payload, result =>
        {
            if (result.Success && result.Single != null)
            {
                pendingLoadedSave = result.Single;
            }
            else if (!string.IsNullOrWhiteSpace(result.Message))
            {
                Debug.LogWarning("CloudSaveManager autosave failed: " + result.Message);
            }
        });

        saveInFlight = false;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        nextAutosaveAt = Time.unscaledTime + AutosaveIntervalSeconds;

        if (sceneApplyInFlight || pendingLoadedSave == null)
        {
            return;
        }

        if (!string.Equals(scene.name, pendingLoadedSave.mapId, StringComparison.Ordinal))
        {
            return;
        }

        StartCoroutine(ApplyPendingSaveState(scene.name));
    }

    private IEnumerator ApplyPendingSaveState(string targetSceneName)
    {
        if (sceneApplyInFlight)
        {
            yield break;
        }

        sceneApplyInFlight = true;
        GameProgress.ApplyCloudLevel(pendingLoadedSave.level);

        float timeoutAt = Time.realtimeSinceStartup + 3f;
        PlayerController player = null;

        while (Time.realtimeSinceStartup < timeoutAt)
        {
            player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                break;
            }

            yield return null;
        }

        if (player != null && string.Equals(SceneManager.GetActiveScene().name, targetSceneName, StringComparison.Ordinal))
        {
            player.transform.position = new Vector3(pendingLoadedSave.playerX, pendingLoadedSave.playerY, player.transform.position.z);

            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.ApplyCloudState(pendingLoadedSave.hp, pendingLoadedSave.mana);
            }
        }

        pendingLoadedSave = null;
        sceneApplyInFlight = false;
    }

    private bool HasAuthToken()
    {
        return HasSavedAuthToken();
    }

    private string BuildEndpointUrl(string route)
    {
        string baseUrl = string.IsNullOrWhiteSpace(backendBaseUrl) ? DefaultBaseUrl : backendBaseUrl;
        return route.StartsWith("/") ? baseUrl + route : baseUrl + "/" + route;
    }

    private SavePayload BuildCurrentPayload(string fallbackSceneName)
    {
        Scene activeScene = SceneManager.GetActiveScene();
        string mapId = activeScene.IsValid() && !string.IsNullOrWhiteSpace(activeScene.name)
            ? activeScene.name
            : fallbackSceneName;

        Vector2 playerPosition = Vector2.zero;
        int hp = 5;
        int mana = 100;

        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            playerPosition = player.transform.position;

            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                hp = Mathf.Max(0, playerHealth.currentHealth);
                mana = Mathf.Max(0, playerHealth.currentEnergy);
            }
        }

        return new SavePayload
        {
            slotName = AutosaveSlotName,
            mapId = mapId,
            playerX = playerPosition.x,
            playerY = playerPosition.y,
            hp = hp,
            mana = mana,
            level = GameProgress.GetCloudLevel()
        };
    }

    private string GetToken()
    {
        return PlayerPrefs.GetString(AuthTokenKey, string.Empty);
    }

    private string NormalizeSceneName(string mapId, string fallbackSceneName)
    {
        if (!string.IsNullOrWhiteSpace(mapId) && Application.CanStreamedLevelBeLoaded(mapId))
        {
            return mapId;
        }

        return fallbackSceneName;
    }

    private static bool ShouldSkipSceneSave(string sceneName)
    {
        return string.Equals(sceneName, "MainMenu", StringComparison.Ordinal) ||
            string.Equals(sceneName, "GameOver", StringComparison.Ordinal);
    }

    private void SetCurrentSaveId(string saveId)
    {
        currentSaveId = saveId ?? string.Empty;
        PlayerPrefs.SetString(SaveIdKey, currentSaveId);
        PlayerPrefs.Save();
    }

    private IEnumerator FetchLatestSave(Action<SaveRequestResult> onComplete)
    {
        using UnityWebRequest request = UnityWebRequest.Get(BuildEndpointUrl("/api/saves"));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", "Bearer " + GetToken());

        yield return request.SendWebRequest();

        HandleListResponse(request, onComplete);
    }

    private IEnumerator CreateSave(SavePayload payload, Action<SaveRequestResult> onComplete)
    {
        yield return SendSaveMutation("/api/saves", UnityWebRequest.kHttpVerbPOST, payload, onComplete);
    }

    private IEnumerator UpdateSave(string saveId, SavePayload payload, Action<SaveRequestResult> onComplete)
    {
        yield return SendSaveMutation("/api/saves/" + saveId, UnityWebRequest.kHttpVerbPUT, payload, onComplete);
    }

    private IEnumerator SendSaveMutation(string route, string method, SavePayload payload, Action<SaveRequestResult> onComplete)
    {
        using UnityWebRequest request = new UnityWebRequest(BuildEndpointUrl(route), method);
        byte[] body = Encoding.UTF8.GetBytes(JsonUtility.ToJson(payload));
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + GetToken());

        yield return request.SendWebRequest();

        HandleSingleResponse(request, onComplete);
    }

    private void HandleListResponse(UnityWebRequest request, Action<SaveRequestResult> onComplete)
    {
        string responseText = request.downloadHandler != null ? request.downloadHandler.text : string.Empty;
        if (!IsRequestSuccessful(request))
        {
            ErrorEnvelope errorEnvelope = TryParse<ErrorEnvelope>(responseText);
            onComplete?.Invoke(SaveRequestResult.Fail(GetErrorMessage(request, errorEnvelope)));
            return;
        }

        SaveListEnvelope envelope = TryParse<SaveListEnvelope>(responseText);
        if (envelope == null || !envelope.success)
        {
            onComplete?.Invoke(SaveRequestResult.Fail(string.IsNullOrWhiteSpace(envelope != null ? envelope.message : string.Empty)
                ? "Could not read save list."
                : envelope.message));
            return;
        }

        onComplete?.Invoke(SaveRequestResult.OkList(envelope.data ?? Array.Empty<SaveSlotData>()));
    }

    private void HandleSingleResponse(UnityWebRequest request, Action<SaveRequestResult> onComplete)
    {
        string responseText = request.downloadHandler != null ? request.downloadHandler.text : string.Empty;
        if (!IsRequestSuccessful(request))
        {
            ErrorEnvelope errorEnvelope = TryParse<ErrorEnvelope>(responseText);
            onComplete?.Invoke(SaveRequestResult.Fail(GetErrorMessage(request, errorEnvelope)));
            return;
        }

        SaveSingleEnvelope envelope = TryParse<SaveSingleEnvelope>(responseText);
        if (envelope == null || !envelope.success || envelope.data == null)
        {
            onComplete?.Invoke(SaveRequestResult.Fail(string.IsNullOrWhiteSpace(envelope != null ? envelope.message : string.Empty)
                ? "Could not read save response."
                : envelope.message));
            return;
        }

        onComplete?.Invoke(SaveRequestResult.OkSingle(envelope.data));
    }

    private static bool IsRequestSuccessful(UnityWebRequest request)
    {
        return request.result == UnityWebRequest.Result.Success &&
            request.responseCode >= 200 &&
            request.responseCode < 300;
    }

    private static string GetErrorMessage(UnityWebRequest request, ErrorEnvelope errorEnvelope)
    {
        if (errorEnvelope != null && !string.IsNullOrWhiteSpace(errorEnvelope.message))
        {
            return errorEnvelope.message;
        }

        if (!string.IsNullOrWhiteSpace(request.error))
        {
            return request.error;
        }

        return "Request failed (" + request.responseCode + ").";
    }

    private static T TryParse<T>(string json) where T : class
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonUtility.FromJson<T>(json);
        }
        catch
        {
            return null;
        }
    }

    [Serializable]
    private class SavePayload
    {
        public string slotName;
        public string mapId;
        public float playerX;
        public float playerY;
        public int hp;
        public int mana;
        public int level;
    }

    [Serializable]
    private class SaveSlotData
    {
        public string id;
        public string slotName;
        public string mapId;
        public float playerX;
        public float playerY;
        public int hp;
        public int mana;
        public int level;
    }

    [Serializable]
    private class SaveListEnvelope
    {
        public bool success;
        public string message;
        public SaveSlotData[] data;
    }

    [Serializable]
    private class SaveSingleEnvelope
    {
        public bool success;
        public string message;
        public SaveSlotData data;
    }

    [Serializable]
    private class ErrorEnvelope
    {
        public bool success;
        public string message;
    }

    private struct SaveRequestResult
    {
        public bool Success { get; }
        public string Message { get; }
        public SaveSlotData Single { get; }
        public SaveSlotData[] List { get; }

        private SaveRequestResult(bool success, string message, SaveSlotData single, SaveSlotData[] list)
        {
            Success = success;
            Message = message;
            Single = single;
            List = list;
        }

        public static SaveRequestResult OkSingle(SaveSlotData data)
        {
            return new SaveRequestResult(true, string.Empty, data, null);
        }

        public static SaveRequestResult OkList(SaveSlotData[] data)
        {
            return new SaveRequestResult(true, string.Empty, null, data);
        }

        public static SaveRequestResult Fail(string message)
        {
            return new SaveRequestResult(false, string.IsNullOrWhiteSpace(message) ? "Request failed." : message, null, null);
        }
    }
}
