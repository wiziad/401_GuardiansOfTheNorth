using UnityEngine;

public class GameProgress : MonoBehaviour
{
    public static GameProgress Instance;
    private const string Level1CompleteKey = "Level1Complete";

    public bool level1Complete = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        level1Complete = PlayerPrefs.GetInt(Level1CompleteKey, 0) == 1;
    }

    public void SetLevel1Complete()
    {
        level1Complete = true;
        PlayerPrefs.SetInt(Level1CompleteKey, 1);
        PlayerPrefs.Save();
    }

    public static int GetCloudLevel()
    {
        bool completed = Instance != null
            ? Instance.level1Complete
            : PlayerPrefs.GetInt(Level1CompleteKey, 0) == 1;
        return completed ? 2 : 1;
    }

    public static void ApplyCloudLevel(int level)
    {
        bool completed = level >= 2;
        PlayerPrefs.SetInt(Level1CompleteKey, completed ? 1 : 0);
        PlayerPrefs.Save();

        if (Instance != null)
        {
            Instance.level1Complete = completed;
        }
    }
}
