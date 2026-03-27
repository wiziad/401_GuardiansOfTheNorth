using UnityEngine;
using UnityEngine.SceneManagement;

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

        SceneManager.LoadScene("Map1");
    }

    public void NotifyLevel2Complete()
    {
        if (CloudSaveManager.Instance != null)
            CloudSaveManager.Instance.SaveCurrentProgress();
        SceneManager.LoadScene("Map2");
    }
}
