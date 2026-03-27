using UnityEngine;

// Attach this to a persistent UI object and wire button OnClick events to these methods.
public class GameFlowConnector : MonoBehaviour
{
    [Header("Main Flow")]
    [SerializeField] private string mainMenuScene = SceneRoutes.MainMenuScene;
    [SerializeField] private string gameIntroScene = SceneRoutes.GameIntroScene;
    [SerializeField] private string map1Scene = SceneRoutes.Map1Scene;
    [SerializeField] private string level1IntroScene = SceneRoutes.ScrollIntro2Scene;
    [SerializeField] private string level1Scene = SceneRoutes.Level1Scene;
    [SerializeField] private string level1VictoryScene = SceneRoutes.Level1VictoryScene;
    [SerializeField] private string map2Scene = SceneRoutes.Map2Scene;
    [SerializeField] private string level2IntroScene = SceneRoutes.ScrollIntroScene;
    [SerializeField] private string level2Scene = SceneRoutes.Level2Scene;
    [SerializeField] private string level2VictoryScene = SceneRoutes.Level2VictoryScene;
    [SerializeField] private string endCongratsScene = SceneRoutes.EndCongratsScene;
    [SerializeField] private string gameOverScene = SceneRoutes.GameOverScene;

    public void GoToMainMenu() => SceneRoutes.LoadScene(mainMenuScene);
    public void EnterRealm() => SceneRoutes.LoadScene(gameIntroScene);
    public void OpenMap1() => SceneRoutes.LoadScene(map1Scene);
    public void OpenLevel1Intro() => SceneRoutes.LoadScene(level1IntroScene);
    public void StartLevel1() => SceneRoutes.LoadScene(level1Scene);
    public void OpenLevel1Victory() => SceneRoutes.LoadScene(level1VictoryScene);
    public void OpenMap2() => SceneRoutes.LoadScene(map2Scene);
    public void OpenLevel2Intro() => SceneRoutes.LoadScene(level2IntroScene);
    public void StartLevel2() => SceneRoutes.LoadScene(level2Scene);
    public void OpenLevel2Victory() => SceneRoutes.LoadScene(level2VictoryScene);
    public void OpenEndCongrats() => SceneRoutes.LoadScene(endCongratsScene);
    public void OpenGameOver() => SceneRoutes.LoadScene(gameOverScene);

    public void RetryFromGameOver()
    {
        string retryScene = PlayerPrefs.GetString(GameOverSceneController.RetryScenePrefKey, SceneRoutes.Level1Scene);
        SceneRoutes.LoadScene(retryScene);
    }
}
