using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class Level2BoatController : MonoBehaviour
{
    [SerializeField] private float speed = 4.5f;
    public Vector2 LastLookDirection { get; private set; } = Vector2.right;
    public Vector2 CurrentMove { get; private set; }
    public bool useBounds = true;
    public Vector2 minBounds = new Vector2(-7.6f, -3.1f);
    public Vector2 maxBounds = new Vector2(5.8f, 2.5f);

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 move;
    private Coroutine actionRoutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void Update()
    {
        move.x = Input.GetAxisRaw("Horizontal");
        move.y = Input.GetAxisRaw("Vertical");
        CurrentMove = move;

        if (animator != null)
        {
            if (move.sqrMagnitude > 0.001f)
            {
                animator.SetFloat("MoveX", move.x);
                animator.SetFloat("MoveY", move.y);
                LastLookDirection = move.normalized;
            }

            animator.SetBool("IsMoving", move.sqrMagnitude > 0.001f);
            animator.SetBool("IsJumping", false);
            animator.SetBool("IsDashing", false);
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = move.normalized * speed;

        if (!useBounds)
        {
            return;
        }

        Vector2 p = rb.position;
        p.x = Mathf.Clamp(p.x, minBounds.x, maxBounds.x);
        p.y = Mathf.Clamp(p.y, minBounds.y, maxBounds.y);
        rb.position = p;
    }

    public void PlayFishingAction()
    {
        if (actionRoutine != null)
        {
            StopCoroutine(actionRoutine);
        }

        actionRoutine = StartCoroutine(FishingActionRoutine());
    }

    private IEnumerator FishingActionRoutine()
    {
        if (animator != null)
        {
            // Uses existing animator parameters so this works with current controller.
            animator.SetBool("IsMoving", false);
            animator.SetFloat("MoveX", 0f);
            animator.SetFloat("MoveY", 0f);
        }
        yield return new WaitForSeconds(0.05f);
        actionRoutine = null;
    }
}
