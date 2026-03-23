using UnityEngine;
using UnityEngine.SceneManagement;

// ============================================================
//  MapManager.cs
//  Attach to an empty GameObject called "MapManager".
//  This is the central coordinator — everything talks to this.
// ============================================================

public class MapManager : MonoBehaviour
{
    [Header("Assign all scripts here")]
    public MapCameraController  cameraController;
    public CharacterDropIn      characterDropIn;
    public LevelSidePanel       sidePanel;
    public MapPin[]             allPins;          // drag all 3 pins here

    private MapPin   activePinInstance;
    private bool     sequenceRunning = false;

    // ── Called by MapPin when clicked ────────────────────────────────────
    public void OnPinClicked(MapPin pin)
    {
        if (sequenceRunning) return;
        sequenceRunning    = true;
        activePinInstance  = pin;

        // Lock all pins during sequence
        foreach (var p in allPins) p.SetInteractable(false);

        // 1. Drop character in at pin position
        characterDropIn.DropAt(pin.transform.position, OnCharacterLanded);
    }

    // ── Step 2: called when character finishes landing ───────────────────
    void OnCharacterLanded()
    {
        // 2. Zoom camera toward pin
        cameraController.ZoomToPin(activePinInstance.transform.position, OnZoomComplete);
    }

    // ── Step 3: called when zoom finishes ────────────────────────────────
    void OnZoomComplete()
    {
        // 3. Slide in side panel with level data
        sidePanel.Show(activePinInstance.data, OnYes, OnNo);
    }

    // ── Yes: load the level ───────────────────────────────────────────────
    void OnYes()
    {
        SceneManager.LoadScene(activePinInstance.data.levelSceneName);
    }

    // ── No: reset everything back to map state ────────────────────────────
    void OnNo()
    {
        sidePanel.Hide();
        cameraController.ResetCamera(OnCameraReset);
    }

    void OnCameraReset()
    {
        characterDropIn.Hide();
        sequenceRunning = false;

        // Re-enable all pins
        foreach (var p in allPins) p.SetInteractable(true);
    }
}
