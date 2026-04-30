using UnityEngine;

public class Energy : MonoBehaviour
{
    [Header("Energy (Stamina)")]
    public float maxEnergy = 100f;
    public float currentEnergy = 50f;

    [Header("Boost Efficiency")]
    public float boostEnergyCostMultiplier = 1f;

    [Header("Food Absorption")]
    public bool absorbFoodUnlocked = false;
    public float absorbRange = 3f;

    private PlayerHealth playerHealth;

    void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        if (absorbFoodUnlocked)
        {
            AbsorbNearbyFood();
        }
    }

    public void AddEnergy(float amount)
    {
        currentEnergy += amount;
        currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);
    }

    public void IncreaseMaxEnergy(float amount)
    {
        maxEnergy += amount;
        currentEnergy = Mathf.Clamp(currentEnergy + amount, 0f, maxEnergy);
    }

    public void ImproveBoostEfficiency(float multiplier)
    {
        boostEnergyCostMultiplier = Mathf.Max(0.1f, boostEnergyCostMultiplier * multiplier);
        Debug.Log("Boost efficiency improved! Multiplier: " + boostEnergyCostMultiplier);
    }

    public void UnlockFoodAbsorb()
    {
        absorbFoodUnlocked = true;
        Debug.Log("Food absorption ability unlocked!");

        if (absorbRange < 4.5f)
        {
            absorbRange = 4.5f;
        }
    }

    public void ImproveFoodAbsorb(float rangeIncrease)
    {
        absorbFoodUnlocked = true;
        absorbRange += rangeIncrease;
        Debug.Log("Food absorption improved! Range: " + absorbRange);
    }

    void AbsorbNearbyFood()
    {
        Collider[] food = Physics.OverlapSphere(transform.position, absorbRange);

        foreach (Collider c in food)
        {
            if (c.CompareTag("Food"))
            {
                Destroy(c.gameObject);
                AddEnergy(5f);
                if (playerHealth != null)
                    playerHealth.Heal(5f);
            }
        }
    }
}