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

    [Header("Food Absorption Tuning")]
    public float absorbTickInterval = 0.12f;
    public int maxFoodPerTick = 1;
    public float maxAbsorbRange = 4.5f;

    private PlayerHealth playerHealth;
    private PlayerLevel playerLevel;
    private PlayerMovement playerMovement;
    private float absorbTickTimer;

    void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
        playerLevel = GetComponent<PlayerLevel>();
        playerMovement = GetComponent<PlayerMovement>();
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

        if (absorbRange < 3.5f)
        {
            absorbRange = 3.5f;
        }
    }

    public void ImproveFoodAbsorb(float rangeIncrease)
    {
        absorbFoodUnlocked = true;
        absorbRange = Mathf.Min(maxAbsorbRange, absorbRange + rangeIncrease);
        Debug.Log("Food absorption improved! Range: " + absorbRange);
    }

    void AbsorbNearbyFood()
    {
        if (absorbTickInterval > 0f)
        {
            absorbTickTimer -= Time.deltaTime;
            if (absorbTickTimer > 0f)
                return;

            absorbTickTimer = absorbTickInterval;
        }

        Collider[] food = Physics.OverlapSphere(transform.position, absorbRange);
        int absorbedThisTick = 0;

        foreach (Collider c in food)
        {
            if (!c.CompareTag("Food"))
                continue;

            Food foodComponent = c.GetComponentInParent<Food>();
            if (foodComponent == null)
                continue;

            AddEnergy(foodComponent.energyValue);

            if (playerHealth != null)
            {
                float healAmount = playerHealth.healthFromFood > 0f ? playerHealth.healthFromFood : foodComponent.energyValue;
                playerHealth.Heal(healAmount);
            }

            if (playerLevel != null)
            {
                playerLevel.AddFood(foodComponent.xpvalue);
            }

            if (playerMovement != null)
            {
                playerMovement.AddSizeMultiplier(foodComponent.sizeIncrease);
            }

            if (SoundEffectManager.instance != null)
            {
                SoundEffectManager.instance.FoodEatenSound();
            }

            Destroy(foodComponent.gameObject);

            absorbedThisTick++;
            if (absorbedThisTick >= Mathf.Max(1, maxFoodPerTick))
                break;
        }
    }
}