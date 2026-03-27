using UnityEngine;

public class GameProgress : MonoBehaviour
{
    public static GameProgress Instance;

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

        level1Complete = PlayerPrefs.GetInt("Level1Complete", 0) == 1;
    }

    public void SetLevel1Complete()
    {
        level1Complete = true;
        PlayerPrefs.SetInt("Level1Complete", 1);
        PlayerPrefs.Save();
    }
}
