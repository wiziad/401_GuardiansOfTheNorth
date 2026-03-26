using UnityEngine;

public class Level2DockZone : MonoBehaviour
{
    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            Level2SceneBootstrap.Instance?.TryCompleteAtDock();
        }
    }
}
