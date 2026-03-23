using UnityEngine;
using UnityEngine.EventSystems;

// ============================================================
//  MapPin.cs
//  Attach to each pin GameObject (one per level province).
//  The pin needs:
//    - A SpriteRenderer with your pin sprite
//    - A CircleCollider2D (set radius so it's easy to click)
//
//  HOW TO SET UP A PIN:
//  1. Create empty GameObject, name it e.g. "Pin_Alberta"
//  2. Add SpriteRenderer → assign a pin/marker sprite
//  3. Add CircleCollider2D
//  4. Add this script
//  5. Assign the matching PinData asset in Inspector
//  6. Assign MapManager in Inspector
//  7. Position the GameObject over the correct province on your map
// ============================================================

public class MapPin : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public PinData      data;
    public MapManager   manager;

    [Header("Hover effect")]
    public float hoverScaleMultiplier = 1.25f;

    private Vector3 baseScale;
    private bool    isInteractable = true;

    void Start()
    {
        baseScale = transform.localScale;
    }

    // ── Mouse hover — scale up ────────────────────────────────────────────
    void OnMouseEnter()
    {
        if (!isInteractable) return;
        transform.localScale = baseScale * hoverScaleMultiplier;
    }

    void OnMouseExit()
    {
        transform.localScale = baseScale;
    }

    // ── Click — trigger the full sequence ────────────────────────────────
    void OnMouseDown()
    {
        if (!isInteractable) return;
        transform.localScale = baseScale;
        manager.OnPinClicked(this);
    }

    public void SetInteractable(bool value)
    {
        isInteractable = value;
        // Dim pin visually when locked out
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = value ? Color.white : new Color(1f, 1f, 1f, 0.4f);
    }
}
