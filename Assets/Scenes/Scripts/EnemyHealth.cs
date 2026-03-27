using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 10;
    private int currentHealth;

    [Header("Knockback")]
    public float knockbackForce = 6f;
    public float knockbackDuration = 0.2f;

    [Header("UI - assign the health bar Canvas that is a child of this enemy")]
    // Drag the child Canvas (World Space) that holds the health bar here,
    // OR leave it null and the script will find it automatically.
    public Image healthFillImage;

    private Rigidbody2D rb;
    private bool isKnockedBack = false;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();

        // Auto-find fill image if not assigned
        if (healthFillImage == null)
        {
            // Looks for any Image named "Fill" or "HealthFill" in children
            Image[] images = GetComponentsInChildren<Image>(includeInactive: true);
            foreach (Image img in images)
            {
                if (img.name.Contains("Fill") || img.name.Contains("fill"))
                {
                    healthFillImage = img;
                    break;
                }
            }
        }

        RefreshUI();
    }

    // ── Health ────────────────────────────────────────────────────────────────

    public void TakeDamage(int damage, Vector2 hitDirection)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        Debug.Log(gameObject.name + " took damage. HP: " + currentHealth);

        RefreshUI();
        StartCoroutine(ApplyKnockback(hitDirection));

        if (currentHealth <= 0)
            Die();
    }

    // ── UI ────────────────────────────────────────────────────────────────────

    void RefreshUI()
    {
        if (healthFillImage != null)
            healthFillImage.fillAmount = (float)currentHealth / maxHealth;
    }

    // ── Knockback ─────────────────────────────────────────────────────────────

    IEnumerator ApplyKnockback(Vector2 direction)
    {
        isKnockedBack = true;
        rb.linearVelocity = Vector2.zero;

        Vector2 knockback = new Vector2(direction.x, 0.5f).normalized;
        rb.AddForce(knockback * knockbackForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);
        isKnockedBack = false;
    }

    public bool IsKnockedBack() => isKnockedBack;

    // ── Death ─────────────────────────────────────────────────────────────────

    void Die()
    {
        bool shouldCompleteLevel1 = ShouldCompleteLevel1Now();
        Debug.Log(gameObject.name + " died");
        Destroy(gameObject);

        if (shouldCompleteLevel1)
        {
            if (GameProgress.Instance != null)
            {
                GameProgress.Instance.SetLevel1Complete();
            }

            SceneRoutes.LoadScene(SceneRoutes.Level1VictoryScene);
        }
    }

    private bool ShouldCompleteLevel1Now()
    {
        if (SceneManager.GetActiveScene().name != SceneRoutes.Level1Scene)
        {
            return false;
        }

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        return enemies.Length <= 1;
    }
}
