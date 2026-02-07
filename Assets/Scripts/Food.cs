using UnityEngine;

public class Food : MonoBehaviour
{
    [Header("Food Effects")]
    public float energyValue = 25f;
    public float sizeIncrease = 0.1f;

    private void OnTriggerEnter(Collider other)
    {
        // ENERGY
        Energy energy = other.GetComponent<Energy>();
        if (energy != null)
        {
            energy.AddEnergy(energyValue);
        }

        // OPTIONAL: size growth (player-specific or organism-wide)
        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player != null)
        {
            player.sizeMultiplier += sizeIncrease;
        }

        // If *anything* successfully consumed it, destroy food
        if (energy != null || player != null)
        {
            Destroy(gameObject);
        }
    }
}

