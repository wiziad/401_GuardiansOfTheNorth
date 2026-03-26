using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 5;
    private int currentHealth;

    public float knockbackForce = 8f;
    public float knockbackDuration = 0.2f;

    private Rigidbody2D rb;
    private bool isKnockedBack = false;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
    }

    public void TakeDamage(int damage, Vector2 hitDirection)
    {
        currentHealth -= damage;
        Debug.Log("Player took damage. HP: " + currentHealth);

        StartCoroutine(ApplyKnockback(hitDirection));

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator ApplyKnockback(Vector2 direction)
    {
        isKnockedBack = true;

        rb.linearVelocity = Vector2.zero;

        // Add slight upward force for better feel
        Vector2 knockback = new Vector2(direction.x, 0.5f).normalized;
        rb.AddForce(knockback * knockbackForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        isKnockedBack = false;
    }

    public bool IsKnockedBack()
    {
        return isKnockedBack;
    }

    void Die()
    {
        Debug.Log("Player died");
        gameObject.SetActive(false);
    }
}