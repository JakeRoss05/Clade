using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float healthDrainPerSecond = 2f;
    public float healthFromFood = 10f;

    private PlayerMovement playerMovement;
    private PlayerShield playerShield;

    void Start()
    {
        currentHealth = maxHealth;
        playerMovement = GetComponent<PlayerMovement>();
        playerShield = GetComponent<PlayerShield>();
    }

    void Update()
    {
        DrainHealth();
    }

    void DrainHealth()
    {
        float sizeFactor = playerMovement != null ? playerMovement.sizeMultiplier : 1f;
        currentHealth -= healthDrainPerSecond * sizeFactor * Time.deltaTime;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
    }

    public void TakeDamage(float damage)
    {
        // Shield reduces damage by 75%
        if (playerShield != null && playerShield.shieldActive)
        {
            damage *= 0.25f;
            Debug.Log("Shield reduced the hit! Damage taken: " + damage);
        }

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    void Die()
    { 
        Debug.Log("Player has died.");
        Destroy(gameObject);
    }
}
