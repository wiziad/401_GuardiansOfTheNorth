using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class EndCongrats : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI congratsText;
    public RectTransform badge; // drag WinnerBadge here

    [Header("Spin Settings")]
    public float spinSpeed = 100f;

    private const string BASE_URL = "https://four01-guardiansofthenorth.onrender.com";
    private const string TOKEN_KEY = "AuthToken";

    void Start()
    {
        StartCoroutine(GetUsername());
    }

    void Update()
    {
        // Spin the badge
        if (badge != null)
        {
            badge.Rotate(0f,spinSpeed * Time.deltaTime ,0f);
        }
    }

    private IEnumerator GetUsername()
    {
        string token = PlayerPrefs.GetString(TOKEN_KEY, "");

        if (string.IsNullOrEmpty(token))
        {
            SetText("Guardian");
            yield break;
        }

        UnityWebRequest request = UnityWebRequest.Get(BASE_URL + "/api/auth/me");
        request.SetRequestHeader("Authorization", "Bearer " + token);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error: " + request.error);
            SetText("Guardian");
            yield break;
        }

        string json = request.downloadHandler.text;
        Debug.Log("Response: " + json);

        MeResponse res = JsonUtility.FromJson<MeResponse>(json);

        if (res != null && res.success && res.data != null)
        {
            SetText(res.data.username);
        }
        else
        {
            SetText("Guardian");
        }
    }

    private void SetText(string username)
    {
        if (congratsText != null)
        {
            congratsText.text = "Congratulations, " + username + "!" + "\nYou have saved the North!\nAbove is your badge of honor";
        }
    }

    // ===== JSON =====
    [System.Serializable]
    private class MeResponse
    {
        public bool success;
        public User data;
    }

    [System.Serializable]
    private class User
    {
        public string username;
    }
}