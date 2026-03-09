using UnityEngine;

public class Food : MonoBehaviour
{
    [Header("Food Effects")]
    public float energyValue = 25f;
    public float sizeIncrease = 0.1f;
    public int xpvalue = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Energy energy = other.GetComponent<Energy>();
        if (energy != null)
        {
            energy.AddEnergy(energyValue);
        }

        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.Heal(energyValue);
        }

        PlayerLevel level = other.GetComponent<PlayerLevel>();
        if (level != null)
        {
            level.AddFood(xpvalue);
        }

        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player != null)
        {
            player.sizeMultiplier += sizeIncrease;
        }

        Destroy(gameObject);

    }
}

