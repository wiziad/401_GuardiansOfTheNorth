using System;
using System.Collections;
using UnityEngine;

// ============================================================
//  CharacterDropIn.cs
//  Attach to your Character GameObject (keep it inactive at start).
//
//  The character drops in from ABOVE the pin position,
//  lands with a small squash animation, then faces forward.
//
//  HOW TO SET UP:
//  1. Have your character GameObject with a SpriteRenderer
//  2. Set it INACTIVE in the Hierarchy at start
//  3. Attach this script
//  4. Set the front-facing sprite in Inspector (the idle/front sprite)
// ============================================================

public class CharacterDropIn : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public SpriteRenderer spriteRenderer;
    public Sprite         frontFacingSprite;    // idle front-facing sprite
    public Animator       animator;             // optional — if you have animations

    [Header("Drop settings")]
    public float dropHeight     = 3f;    // how far above pin to start drop from
    public float dropDuration   = 0.45f; // seconds to fall
    public float squashDuration = 0.15f; // seconds for landing squash
    public AnimationCurve dropCurve = AnimationCurve.EaseInOut(0,0,1,1);

    private Action  onLandedCallback;
    private Vector3 baseScale;

    void Awake()
    {
        baseScale = transform.localScale;
        gameObject.SetActive(false);
    }

    // ── Drop the character in at a world position ─────────────────────────
    public void DropAt(Vector3 targetWorldPos, Action onLanded)
    {
        onLandedCallback = onLanded;

        // Position character above the target
        Vector3 startPos = targetWorldPos + Vector3.up * dropHeight;
        transform.position   = startPos;
        transform.localScale = baseScale;
        gameObject.SetActive(true);

        // Switch to front-facing sprite
        if (frontFacingSprite != null)
            spriteRenderer.sprite = frontFacingSprite;

        // Trigger fall-in animation if Animator exists
        if (animator != null)
            animator.SetTrigger("Drop");

        StopAllCoroutines();
        StartCoroutine(DropRoutine(startPos, targetWorldPos));
    }

    IEnumerator DropRoutine(Vector3 startPos, Vector3 endPos)
    {
        // Phase 1: Fall down
        float elapsed = 0f;
        while (elapsed < dropDuration)
        {
            elapsed += Time.deltaTime;
            float t  = dropCurve.Evaluate(Mathf.Clamp01(elapsed / dropDuration));
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
        transform.position = endPos;

        // Phase 2: Landing squash — squish wide, then spring back
        float squashElapsed = 0f;
        while (squashElapsed < squashDuration)
        {
            squashElapsed += Time.deltaTime;
            float t = squashElapsed / squashDuration;
            // squash: wide and flat on impact, back to normal by end
            float squashX = Mathf.Lerp(1.3f, 1f, t);
            float squashY = Mathf.Lerp(0.7f, 1f, t);
            transform.localScale = new Vector3(
                baseScale.x * squashX,
                baseScale.y * squashY,
                baseScale.z
            );
            yield return null;
        }
        transform.localScale = baseScale;

        // Trigger idle animation
        if (animator != null)
            animator.SetTrigger("Idle");

        onLandedCallback?.Invoke();
    }

    // ── Hide character (called on reset) ─────────────────────────────────
    public void Hide()
    {
        StopAllCoroutines();
        gameObject.SetActive(false);
    }
}
