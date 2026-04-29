using UnityEngine;

public class Food : MonoBehaviour
{
    [Header("Food Effects")]
    public float energyValue = 25f;
    public float sizeIncrease = 0.1f;
    public int xpvalue = 1;

    private bool consumed;

    private void OnTriggerEnter(Collider other)
    {
        TryConsume(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryConsume(collision.collider);
    }

    private void TryConsume(Collider other)
    {
        if (consumed || other == null)
            return;

        Energy energy = other.GetComponentInParent<Energy>();
        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        PlayerLevel level = other.GetComponentInParent<PlayerLevel>();
        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();

        // Treat an object as the player if any core player gameplay component exists on it.
        if (energy == null && health == null && level == null && player == null)
            return;

        consumed = true;

        if (energy != null)
        {
            energy.AddEnergy(energyValue);
        }

        if (health != null)
        {
            float healAmount = health.healthFromFood > 0f ? health.healthFromFood : energyValue;
            health.Heal(healAmount);
        }

        if (level != null)
        {
            level.AddFood(xpvalue);
        }

        if (player != null)
        {
            player.sizeMultiplier += sizeIncrease;
        }

        Destroy(gameObject);
    }
}

