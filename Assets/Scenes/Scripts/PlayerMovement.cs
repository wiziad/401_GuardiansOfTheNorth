using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    // Movement
    public float moveSpeed = 5f;
    private float moveX;
    private float moveY;

    // Components
    private Rigidbody2D rb;
    private Animator animator;

    // Jump
    public float jumpForce = 8f;
    private bool isGrounded = true;

    // Dash
    public float dashForce = 10f;
    public float dashTime = 0.2f;
    private bool isDashing = false;

    // Attack
    public float attackRange = 0.6f;
    public Transform attackPoint;

    // Direction (IMPORTANT FIX)
    private Vector2 lastDirection = Vector2.right;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // ---------------- MOVEMENT INPUT ----------------
        moveX = Input.GetAxisRaw("Horizontal");
        moveY = Input.GetAxisRaw("Vertical");

        // Prevent diagonal movement
        if (moveX != 0)
        {
            moveY = 0;
        }

        // ---------------- ANIMATION ----------------
        if (moveX != 0 || moveY != 0)
        {
            animator.SetFloat("MoveX", moveX);
            animator.SetFloat("MoveY", moveY);
        }

        bool isMoving = moveX != 0 || moveY != 0;
        animator.SetBool("IsMoving", isMoving);

        // ---------------- STORE LAST DIRECTION ----------------
        if (moveX != 0 || moveY != 0)
        {
            lastDirection = new Vector2(moveX, moveY).normalized;
        }

        // ---------------- POSITION ATTACK POINT ----------------
        attackPoint.localPosition = lastDirection * attackRange;

        // ---------------- ATTACK INPUT ----------------
        if (Input.GetMouseButtonDown(0))
        {
            Attack();
        }

        // ---------------- JUMP ----------------
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            animator.SetBool("IsJumping", true);
            isGrounded = false;
        }

        // ---------------- DASH ----------------
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing)
        {
            StartCoroutine(Dash());
        }
    }

    void FixedUpdate()
    {
        if (!isDashing)
        {
            rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);
        }
    }

    // ---------------- ATTACK FUNCTION ----------------
    // void Attack()
    // {
    //     Debug.Log("ATTACK");

    //     // Force correct layer detection (no inspector issues)
    //     int enemyLayerMask = LayerMask.GetMask("Enemy");

    //     Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
    //         attackPoint.position,
    //         attackRange,
    //         enemyLayerMask
    //     );

    //     Debug.Log("Enemies detected: " + hitEnemies.Length);

    //     foreach (Collider2D enemy in hitEnemies)
    //     {
    //         Debug.Log("Hit " + enemy.name);
    //     }
    // }

    void Attack()
{
    Debug.Log("ATTACK");

    Collider2D[] hits = Physics2D.OverlapCircleAll(
        attackPoint.position,
        attackRange
    );

    Debug.Log("Total hits: " + hits.Length);

    foreach (Collider2D hit in hits)
    {
        Debug.Log("Found: " + hit.name + " | Layer: " + LayerMask.LayerToName(hit.gameObject.layer));
    }
}

    // ---------------- DASH ----------------
    IEnumerator Dash()
    {
        isDashing = true;
        animator.SetBool("IsDashing", true);

        Vector2 dashDirection = lastDirection;

        rb.linearVelocity = dashDirection * dashForce;

        yield return new WaitForSeconds(dashTime);

        isDashing = false;
        animator.SetBool("IsDashing", false);
    }

    // ---------------- GROUND CHECK ----------------
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            animator.SetBool("IsJumping", false);
        }
    }

    // ---------------- DEBUG VISUAL ----------------
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}