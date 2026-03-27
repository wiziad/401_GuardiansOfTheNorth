using UnityEngine;

// Attach to any GameObject in your level scenes.
// Call NotifyLevel1Complete() from your existing win condition logic.
public class LevelCompleteNotifier : MonoBehaviour
{
    public void NotifyLevel1Complete()
    {
        if (GameProgress.Instance != null)
            GameProgress.Instance.SetLevel1Complete();
        if (CloudSaveManager.Instance != null)
            CloudSaveManager.Instance.SaveCurrentProgress();

        SceneRoutes.LoadScene(SceneRoutes.Level1VictoryScene);
    }

    public void NotifyLevel2Complete()
    {
        if (CloudSaveManager.Instance != null)
            CloudSaveManager.Instance.SaveCurrentProgress();
        SceneRoutes.LoadScene(SceneRoutes.Level2VictoryScene);
    }

    // Optional fallback methods if any old button/event still expects map navigation.
    public void ReturnToMap1()
    {
        SceneRoutes.LoadScene(SceneRoutes.Map1Scene);
    }

    public void ReturnToMap2()
    {
        SceneRoutes.LoadScene(SceneRoutes.Map2Scene);
    }
}
