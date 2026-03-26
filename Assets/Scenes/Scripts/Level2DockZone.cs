using UnityEngine;

public class Level2DockZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Level2SceneBootstrap.Instance?.SetDockInRange(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Level2SceneBootstrap.Instance?.SetDockInRange(false);
        }
    }

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
