using UnityEngine;
using System.Collections;

public class MapZoom : MonoBehaviour
{
    [Header("Drag your MapBackground RectTransform here")]
    public RectTransform mapRect;

    [Header("Zoom Settings")]
    public float zoomScale = 2.0f;
    public float zoomDuration = 0.6f;

    [Header("Province Focus Points (0 to 1). Tune after testing.")]
    public Vector2 albertaFocusPoint = new Vector2(0.28f, 0.35f);
    public Vector2 ontarioFocusPoint = new Vector2(0.60f, 0.30f);

    private Vector3 originalScale;
    private Vector2 originalPosition;
    private Coroutine currentCoroutine;

    void Start()
    {
        if (mapRect != null)
        {
            originalScale = mapRect.localScale;
            originalPosition = mapRect.anchoredPosition;
        }
    }

    public void ZoomToAlberta(System.Action onComplete)
    {
        ZoomTo(albertaFocusPoint, onComplete);
    }

    public void ZoomToOntario(System.Action onComplete)
    {
        ZoomTo(ontarioFocusPoint, onComplete);
    }

    public void ZoomOut(System.Action onComplete = null)
    {
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(AnimateZoom(
            mapRect.localScale, originalScale,
            mapRect.anchoredPosition, originalPosition,
            onComplete));
    }

    void ZoomTo(Vector2 focusPoint, System.Action onComplete)
    {
        if (mapRect == null) { onComplete?.Invoke(); return; }
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);

        Vector2 size = mapRect.rect.size;
        Vector2 targetPos = new Vector2(
            (0.5f - focusPoint.x) * size.x * zoomScale,
            (0.5f - focusPoint.y) * size.y * zoomScale
        );

        currentCoroutine = StartCoroutine(AnimateZoom(
            mapRect.localScale, Vector3.one * zoomScale,
            mapRect.anchoredPosition, targetPos,
            onComplete));
    }

    IEnumerator AnimateZoom(
        Vector3 fromScale, Vector3 toScale,
        Vector2 fromPos, Vector2 toPos,
        System.Action onComplete)
    {
        float elapsed = 0f;
        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / zoomDuration);
            t = t * t * (3f - 2f * t); // smooth step
            mapRect.localScale = Vector3.Lerp(fromScale, toScale, t);
            mapRect.anchoredPosition = Vector2.Lerp(fromPos, toPos, t);
            yield return null;
        }
        mapRect.localScale = toScale;
        mapRect.anchoredPosition = toPos;
        onComplete?.Invoke();
    }
}
