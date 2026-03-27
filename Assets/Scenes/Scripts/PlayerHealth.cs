using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 5;
    public int currentHealth;

    [Header("Energy")]
    public int maxEnergy = 100;
    public int currentEnergy;

    [Header("Knockback")]
    public float knockbackForce = 8f;
    public float knockbackDuration = 0.2f;

    [Header("UI - drag these from your HUD Canvas")]
    public Image healthFillImage;   // the red fill image of the health bar
    public Image energyFillImage;   // the yellow/blue fill image of the energy bar

    private Rigidbody2D rb;
    private bool isKnockedBack = false;

    void Start()
    {
        currentHealth = maxHealth;
        currentEnergy = maxEnergy;
        rb = GetComponent<Rigidbody2D>();
        RefreshUI();
    }

    // ── Health ────────────────────────────────────────────────────────────────

    public void TakeDamage(int damage, Vector2 hitDirection)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        Debug.Log("Player took damage. HP: " + currentHealth);

        RefreshUI();
        StartCoroutine(ApplyKnockback(hitDirection));

        if (currentHealth <= 0)
            Die();
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        RefreshUI();
    }

    // ── Energy ────────────────────────────────────────────────────────────────

    /// <summary>Returns false if not enough energy.</summary>
    public bool UseEnergy(int amount)
    {
        Debug.Log("UseEnergy called! Current: " + currentEnergy + " Cost: " + amount);
        if (currentEnergy < amount)
        {
            Debug.Log("Not enough energy!");
            return false;
        }
        currentEnergy = Mathf.Max(0, currentEnergy - amount);
        Debug.Log("Energy after use: " + currentEnergy);
        RefreshUI();
        return true;
    }

    public void GainEnergy(int amount)
    {
        Debug.Log("GainEnergy called! Amount: " + amount);
        currentEnergy = Mathf.Min(maxEnergy, currentEnergy + amount);
        RefreshUI();
    }

    // ── UI ────────────────────────────────────────────────────────────────────

    void RefreshUI()
    {
        if (healthFillImage != null)
            healthFillImage.fillAmount = (float)currentHealth / maxHealth;

        if (energyFillImage != null)
            energyFillImage.fillAmount = (float)currentEnergy / maxEnergy;
    }

    public void ApplyCloudState(int hp, int mana)
    {
        currentHealth = Mathf.Clamp(hp, 0, maxHealth);
        currentEnergy = Mathf.Clamp(mana, 0, maxEnergy);
        RefreshUI();
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
        Debug.Log("Player died");

        // Save the current scene so "Play Again" reloads it
        PlayerPrefs.SetString(GameOverSceneController.RetryScenePrefKey, 
                            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        PlayerPrefs.Save();

        // Load the Game Over scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(GameOverSceneController.SceneName);
    }
}
