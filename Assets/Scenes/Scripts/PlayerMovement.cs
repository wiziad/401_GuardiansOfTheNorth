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

    private PlayerHealth playerHealth;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerHealth = GetComponent<PlayerHealth>();
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
            if (!isDashing && !playerHealth.IsKnockedBack())
            {
                rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);
            }
        }

    // ---------------- ATTACK FUNCTION ----------------
  
void Attack()
    {
        Debug.Log("ATTACK");

        animator.SetFloat("MoveX", lastDirection.x);
        animator.SetFloat("MoveY", lastDirection.y);
        animator.SetTrigger("Attack");
    }

    // ---------------- DEAL DAMAGE FUNCTION ----------------
public void DealDamage()
    {
        Debug.Log("DEAL DAMAGE FRAME");

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemyObj in enemies)
        {
            float distance = Mathf.Abs(transform.position.x - enemyObj.transform.position.x);

            if (distance <= attackRange)
            {
                EnemyHealth enemy = enemyObj.GetComponent<EnemyHealth>();

                if (enemy != null)
                {
                    Vector2 direction = (enemyObj.transform.position - transform.position).normalized;
                    enemy.TakeDamage(1, direction);

                    Debug.Log("Hit enemy: " + enemyObj.name);
                }
            }
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