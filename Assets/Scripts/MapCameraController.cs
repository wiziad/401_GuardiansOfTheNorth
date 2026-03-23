using System;
using System.Collections;
using UnityEngine;

// ============================================================
//  MapCameraController.cs
//  Attach to your Main Camera.
//  Smoothly zooms toward the clicked pin then resets.
// ============================================================

public class MapCameraController : MonoBehaviour
{
    [Header("Zoom settings")]
    public float zoomedOrthographicSize = 2.5f;   // smaller = more zoomed in
    public float zoomDuration           = 0.8f;   // seconds to zoom in
    public float resetDuration          = 0.6f;   // seconds to zoom back out
    public AnimationCurve zoomCurve     = AnimationCurve.EaseInOut(0,0,1,1);

    private Camera       cam;
    private float        defaultSize;
    private Vector3      defaultPosition;

    void Awake()
    {
        cam             = GetComponent<Camera>();
        defaultSize     = cam.orthographicSize;
        defaultPosition = transform.position;
    }

    // ── Zoom in toward a world position ──────────────────────────────────
    public void ZoomToPin(Vector3 pinWorldPos, Action onComplete)
    {
        StopAllCoroutines();
        StartCoroutine(ZoomRoutine(pinWorldPos, zoomedOrthographicSize, zoomDuration, onComplete));
    }

    // ── Reset camera back to default ─────────────────────────────────────
    public void ResetCamera(Action onComplete)
    {
        StopAllCoroutines();
        StartCoroutine(ZoomRoutine(defaultPosition, defaultSize, resetDuration, onComplete));
    }

    IEnumerator ZoomRoutine(Vector3 targetPos, float targetSize, float duration, Action onComplete)
    {
        float   startSize = cam.orthographicSize;
        Vector3 startPos  = transform.position;

        // Target position keeps the Z the same as camera
        Vector3 endPos = new Vector3(targetPos.x, targetPos.y, transform.position.z);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t  = zoomCurve.Evaluate(Mathf.Clamp01(elapsed / duration));

            cam.orthographicSize    = Mathf.Lerp(startSize, targetSize, t);
            transform.position      = Vector3.Lerp(startPos, endPos, t);

            yield return null;
        }

        cam.orthographicSize = targetSize;
        transform.position   = endPos;

        onComplete?.Invoke();
    }
}
