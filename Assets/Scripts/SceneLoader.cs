using UnityEngine;

// Attach to any GameObject. Hook its LoadScene() method to a button's OnClick.
// Type the exact scene name into the sceneName field in the Inspector.
public class SceneLoader : MonoBehaviour
{
    public string sceneName;

    public void LoadScene()
    {
        SceneRoutes.LoadScene(sceneName);
    }
}
