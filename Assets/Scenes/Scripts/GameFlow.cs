using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameFlow
{
    public static void FailCurrentLevel()
    {
        string activeScene = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString(GameOverSceneController.RetryScenePrefKey, activeScene);
        PlayerPrefs.Save();

        if (Application.CanStreamedLevelBeLoaded(GameOverSceneController.SceneName))
        {
            SceneManager.LoadScene(GameOverSceneController.SceneName);
        }
        else
        {
            SceneManager.LoadScene(GameOverSceneController.ScenePath);
        }
    }
}
