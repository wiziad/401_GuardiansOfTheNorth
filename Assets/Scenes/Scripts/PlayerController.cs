using UnityEngine;
using System.Collections;
using UnityEngine.UI;

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
    public int dashEnergyCost = 20;         // energy used per dash

    // Attack
    public float attackRange = 0.6f;
    public Transform attackPoint;
    public int attackEnergyCost = 10;       // energy used per attack
    private Coroutine energyFlashCoroutine;
    private Coroutine healthFlashCoroutine;

    [Header("Audio")]
    public AudioClip attackSound;
    public AudioClip jumpSound;
    public AudioClip dashSound;
    private AudioSource audioSource;

    // Direction
    private Vector2 lastDirection = Vector2.right;

    // Energy regen
    public float energyRegenPerSecond = 2f;
    private float energyRegenAccumulator = 0f;

    private PlayerHealth playerHealth;

    void Start()
    {
        // Get required components
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerHealth = GetComponent<PlayerHealth>();
        audioSource = GetComponent<AudioSource>();

        // Validate that all required components are present
        if (rb == null)
            Debug.LogError($"PlayerController ({gameObject.name}): Rigidbody2D component not found!");
        if (animator == null)
            Debug.LogError($"PlayerController ({gameObject.name}): Animator component not found! This will cause crashes in OnCollisionEnter2D.");
        if (playerHealth == null)
            Debug.LogError($"PlayerController ({gameObject.name}): PlayerHealth component not found!");
        if (attackPoint == null)
            Debug.LogError($"PlayerController ({gameObject.name}): attackPoint is not assigned in the Inspector!");
    }

    void Update()
    {
        // ---------------- MOVEMENT INPUT ----------------
        moveX = Input.GetAxisRaw("Horizontal");
        moveY = Input.GetAxisRaw("Vertical");

        // Prevent diagonal movement
        if (moveX != 0)
            moveY = 0;

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
            lastDirection = new Vector2(moveX, moveY).normalized;

        // ---------------- POSITION ATTACK POINT ----------------
        attackPoint.localPosition = lastDirection * attackRange;

        // ---------------- ATTACK INPUT ----------------
        if (Input.GetMouseButtonDown(0))
            Attack();

        // ---------------- JUMP ----------------
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            animator.SetBool("IsJumping", true);

            if (audioSource != null && jumpSound != null)
                audioSource.PlayOneShot(jumpSound);
            isGrounded = false;
        }

        // ---------------- DASH (costs energy) ----------------
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing)
        {
            // Only dash if we have enough energy
            if (playerHealth.UseEnergy(dashEnergyCost))
                StartCoroutine(Dash());
        }

        // ---------------- ENERGY REGEN ----------------
        energyRegenAccumulator += energyRegenPerSecond * Time.deltaTime;
        if (energyRegenAccumulator >= 1f)
        {
            int toRegen = Mathf.FloorToInt(energyRegenAccumulator);
            playerHealth.GainEnergy(toRegen);
            energyRegenAccumulator -= toRegen;
        }

        // ---------------- ENERGY BLINK WARNING ----------------
        if (playerHealth.currentEnergy <= playerHealth.maxEnergy * 0.5f)
        {
            if (energyFlashCoroutine == null)
                energyFlashCoroutine = StartCoroutine(FlashEnergyBar());
        }
        else
        {
            // Energy is above 50% — stop flashing and restore color
            if (energyFlashCoroutine != null)
            {
                StopCoroutine(energyFlashCoroutine);
                energyFlashCoroutine = null;
                playerHealth.energyFillImage.color = Color.yellow;
            }
        }

        // ---------------- HEALTH BLINK WARNING ----------------
        if (playerHealth.currentHealth <= playerHealth.maxHealth * 0.5f)
        {
            if (healthFlashCoroutine == null)
                healthFlashCoroutine = StartCoroutine(FlashHealthBar());
        }
        else
        {
            if (healthFlashCoroutine != null)
            {
                StopCoroutine(healthFlashCoroutine);
                healthFlashCoroutine = null;
                playerHealth.healthFillImage.color = Color.red;
            }
        }
    }

    void FixedUpdate()
    {
        if (!isDashing && !playerHealth.IsKnockedBack())
            rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);
    }

    // ---------------- ATTACK ----------------
    void Attack()
    {
        if (!playerHealth.UseEnergy(attackEnergyCost))
        {
            // Flash the energy bar red to show player they're out of energy
            if (energyFlashCoroutine != null)
                StopCoroutine(energyFlashCoroutine);
            energyFlashCoroutine = StartCoroutine(FlashEnergyBar());
            return;
        }
    
        Debug.Log("ATTACK");
        animator.SetFloat("MoveX", lastDirection.x);
        animator.SetFloat("MoveY", lastDirection.y);
        animator.SetTrigger("Attack");

        if (audioSource != null && attackSound != null)
            audioSource.PlayOneShot(attackSound);
    }
    
    // ADD this coroutine anywhere in PlayerController:
    IEnumerator FlashEnergyBar()
    {
        Image energyFill = playerHealth.energyFillImage;
        if (energyFill == null) yield break;

        Color originalColor = Color.yellow;
        Color flashColor = Color.red;

        // Loop forever until stopped from outside
        while (true)
        {
            energyFill.color = flashColor;
            yield return new WaitForSeconds(0.2f);
            energyFill.color = originalColor;
            yield return new WaitForSeconds(0.2f);
        }
    }
    IEnumerator FlashHealthBar()
    {
        Image healthFill = playerHealth.healthFillImage;
        if (healthFill == null) yield break;

        Color originalColor = Color.red;
        Color flashColor = Color.white;

        while (true)
        {
            healthFill.color = flashColor;
            yield return new WaitForSeconds(0.2f);
            healthFill.color = originalColor;
            yield return new WaitForSeconds(0.2f);
        }
    }

    // ---------------- DEAL DAMAGE (called by animation event) ----------------
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

        if (audioSource != null && dashSound != null)
            audioSource.PlayOneShot(dashSound);

        rb.linearVelocity = lastDirection * dashForce;

        yield return new WaitForSeconds(dashTime);

        isDashing = false;
        animator.SetBool("IsDashing", false);
    }

    // ---------------- GROUND CHECK ----------------
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Defensive null checks to prevent NullReferenceException
        if (collision == null)
        {
            Debug.LogError("OnCollisionEnter2D: collision parameter is null!");
            return;
        }

        if (collision.gameObject == null)
        {
            Debug.LogError("OnCollisionEnter2D: collision.gameObject is null!");
            return;
        }

        // Check if animator exists (critical for this scene)
        if (animator == null)
        {
            Debug.LogError($"OnCollisionEnter2D ({gameObject.name}): animator is null! Player GameObject is missing Animator component or it wasn't initialized. Check the Inspector.");
            return;
        }

        // Now safely check the tag and set ground state
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            animator.SetBool("IsJumping", false);
            Debug.Log($"{gameObject.name} landed on ground.");
        }
    }

    // ---------------- DEBUG ----------------
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}