using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneRoutes
{
    public const string MainMenuScene = "MainMenu";
    public const string GameIntroScene = "GameIntro";
    public const string Map1Scene = "Map1";
    public const string ScrollIntro2Scene = "ScrollIntro2";
    public const string Level1Scene = "Level_01_Test";
    public const string Level1VictoryScene = "Level01_Victory";
    public const string Map2Scene = "Map2";
    public const string ScrollIntroScene = "ScrollIntro";
    public const string Level2Scene = "Level_02_WaterCleanup";
    public const string Level2VictoryScene = "Level02_Victory";
    public const string EndCongratsScene = "End_Congrats";
    public const string GameOverScene = "GameOver";

    public static void LoadScene(string sceneName, string fallbackScenePath = "")
    {
        if (!string.IsNullOrWhiteSpace(sceneName) && Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
            return;
        }

        if (!string.IsNullOrWhiteSpace(fallbackScenePath))
        {
            SceneManager.LoadScene(fallbackScenePath);
            return;
        }

        Debug.LogError($"SceneRoutes: Could not load scene '{sceneName}'. Add it to Build Settings.");
    }
}
