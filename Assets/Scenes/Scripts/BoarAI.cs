using UnityEngine;

public class BoarAI : MonoBehaviour
{
    [Header("Detection")]
    public float detectionRange = 5f;       // how close before boar reacts
    public Transform player;                // drag your player here in Inspector

    [Header("Attack")]
    public float attackRange = 1.5f;        // how close to actually swing
    public float attackCooldown = 2f;       // seconds between attacks

    [Header("Charge")]
    public float chargeSpeed = 8f;          // how fast the charge is
    public float chargeDuration = 0.6f;     // how long each charge lasts
    public float chargeChance = 0.3f;       // 30% chance to charge instead of attack
    public float chargeCooldown = 4f;       // seconds between charges

    [Header("Movement")]
    public float moveSpeed = 2f;            // normal walking speed toward player

    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private float attackTimer = 0f;
    private float chargeTimer = 0f;
    private float chargeDurationTimer = 0f;
    private bool isCharging = false;
    private Vector2 chargeDirection;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // Auto-find player if not assigned
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Count down timers
        attackTimer -= Time.deltaTime;
        chargeTimer -= Time.deltaTime;

        if (isCharging)
        {
            HandleCharge();
            return; // skip other logic while charging
        }

        if (distanceToPlayer <= detectionRange)
        {
            // Face the player
            FlipTowardPlayer();

            if (distanceToPlayer <= attackRange)
            {
                // Stop moving
                rb.linearVelocity = Vector2.zero;
                animator.SetBool("isWalking", false);

                // Try to charge or attack
                if (chargeTimer <= 0f && Random.value < chargeChance)
                {
                    StartCharge();
                }
                else if (attackTimer <= 0f)
                {
                    Attack();
                }
            }
            else
            {
                // Walk toward player
                MoveTowardPlayer();
            }
        }
        else
        {
            // Out of range — idle
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("isWalking", false);
        }
    }

    void MoveTowardPlayer()
    {
        animator.SetBool("isWalking", true);
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
    }

    void Attack()
    {
        //attackTimer = attackCooldown;
        animator.SetTrigger("Attack");
    }

    void StartCharge()
    {
        isCharging = true;
        chargeTimer = chargeCooldown;
        chargeDurationTimer = chargeDuration;
        chargeDirection = (player.position - transform.position).normalized;
        animator.SetTrigger("Charge"); // make sure you have a Charge animation
    }

    void HandleCharge()
    {
        chargeDurationTimer -= Time.deltaTime;
        rb.linearVelocity = chargeDirection * chargeSpeed;

        if (chargeDurationTimer <= 0f)
        {
            isCharging = false;
            rb.linearVelocity = Vector2.zero;
        }
    }

    void FlipTowardPlayer()
    {
        if (player.position.x < transform.position.x)
            spriteRenderer.flipX = false;  // face right
        else
            spriteRenderer.flipX = true; // face left
    }

    // Draw detection range in editor so you can see it
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}