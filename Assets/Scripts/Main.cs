using UnityEngine;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour
{
    [Header("Launcher")]
    [SerializeField] private bool runFromMainMenuOnPlay = true;

    private void Start()
    {
        if (!runFromMainMenuOnPlay)
        {
            return;
        }

        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene != SceneRoutes.MainMenuScene)
        {
            SceneRoutes.LoadScene(SceneRoutes.MainMenuScene);
        }
    }

    // Hook this to a button if you want a manual "start everything" trigger.
    public void RunGame()
    {
        SceneRoutes.LoadScene(SceneRoutes.MainMenuScene);
    }
}
