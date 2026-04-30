using UnityEngine;

public class PlayerLevel : MonoBehaviour
{
    public int level = 1;

    public int foodCollected = 0;
    public int foodToLevelUp = 5;

    public Energy playerEnergy;
    public PlayerHealth playerHealth;
    public PlayerShield playerShield;
    public PlayerCombat playerCombat;

    public Transform playerModel;

    [Header("Level 3 Upgrade Choice")]
    public GameObject level3ChoiceWindow;
    public bool pauseGameWhenChoosingLevel3 = true;

    [Header("Level 4 Upgrade Choice")]
    public GameObject level4ChoiceWindow;
    public bool pauseGameWhenChoosingLevel4 = true;

    private bool isAwaitingLevel3Choice;
    private bool isAwaitingLevel4Choice;

    void Start()
    {
        isAwaitingLevel3Choice = false;
        isAwaitingLevel4Choice = false;

        if (playerEnergy == null)
        {
            playerEnergy = GetComponent<Energy>();
        }

        if (level3ChoiceWindow != null)
        {
            level3ChoiceWindow.SetActive(false);
        }

        if (level4ChoiceWindow != null)
        {
            level4ChoiceWindow.SetActive(false);
        }
    }

    public void AddFood(int amount)
    {
        foodCollected += amount;

        if (foodCollected >= foodToLevelUp)
        {
            LevelUp();
        }
    }

    void LevelUp()
    {
        level++;

        foodCollected = 0;
        foodToLevelUp += 3;

        Debug.Log("Player leveled up! Current level: " + level);

        ApplyLevelRewards();
    }

    void ApplyLevelRewards()
    {
        if (level == 2)
        {
            UnlockShield();
            GrowPlayer();
            IncreaseHealth();
        }
        else if (level == 3)
        {
            ShowLevel3ChoiceWindow();
            GrowPlayer();
            IncreaseHealth();
        }
        else if (level == 4)
        {
            ShowLevel4ChoiceWindow();
            GrowPlayer();
            IncreaseHealth();
        }
    }

    void ShowLevel3ChoiceWindow()
    {
        isAwaitingLevel3Choice = true;

        if (level3ChoiceWindow != null)
        {
            level3ChoiceWindow.SetActive(true);
        }

        if (ShouldPauseForLevel3())
        {
            Time.timeScale = 0f;
        }

        Debug.Log("Level 3 reached! Waiting for player upgrade choice.");
    }

    void ShowLevel4ChoiceWindow()
    {
        isAwaitingLevel4Choice = true;

        if (level4ChoiceWindow != null)
        {
            level4ChoiceWindow.SetActive(true);
        }

        if (ShouldPauseForLevel4())
        {
            Time.timeScale = 0f;
        }

        Debug.Log("Level 4 reached! Waiting for player upgrade choice.");
    }

    void UnlockShield()
    {
        if (playerShield != null)
        {
            playerShield.Unlock(); // Gives 1 charge
        }
    }

    void UnlockCombat()
    {
        if (playerCombat != null)
        {
            playerCombat.Unlock();
        }
    }

    void UpgradeShield()
    {
        if (playerShield != null)
        {
            playerShield.UpgradeCharges(3); // Upgrade to 3 charges at level 3
        }
    }

    void UpgradeEnergyCapacity()
    {
        if (playerEnergy != null)
        {
            playerEnergy.IncreaseMaxEnergy(25f);
            playerEnergy.ImproveBoostEfficiency(0.8f);
        }
    }

    void UnlockFoodAbsorption()
    {
        if (playerEnergy != null)
        {
            playerEnergy.UnlockFoodAbsorb();
            playerEnergy.ImproveFoodAbsorb(1.5f);
        }
    }

    public void ChooseCombatUnlock()
    {
        if (!isAwaitingLevel3Choice)
            return;

        UnlockCombat();
        CompleteLevel3Choice();
    }

    public void ChooseShieldUpgrade()
    {
        if (!isAwaitingLevel3Choice)
            return;

        UpgradeShield();
        CompleteLevel3Choice();
    }

    public void ChooseEnergyUpgrade()
    {
        if (!isAwaitingLevel4Choice)
            return;

        UpgradeEnergyCapacity();
        CompleteLevel4Choice();
    }

    public void ChooseFoodAbsorbUpgrade()
    {
        if (!isAwaitingLevel4Choice)
            return;

        UnlockFoodAbsorption();
        CompleteLevel4Choice();
    }

    void CompleteLevel3Choice()
    {
        isAwaitingLevel3Choice = false;

        if (level3ChoiceWindow != null)
        {
            level3ChoiceWindow.SetActive(false);
        }

        if (ShouldPauseForLevel3())
        {
            Time.timeScale = 1f;
        }
    }

    void CompleteLevel4Choice()
    {
        isAwaitingLevel4Choice = false;

        if (level4ChoiceWindow != null)
        {
            level4ChoiceWindow.SetActive(false);
        }

        if (ShouldPauseForLevel4())
        {
            Time.timeScale = 1f;
        }
    }

    void OnDisable()
    {
        if ((ShouldPauseForLevel3() || ShouldPauseForLevel4()) && Time.timeScale == 0f)
        {
            Time.timeScale = 1f;
        }
    }

    bool ShouldPauseForLevel3()
    {
        return pauseGameWhenChoosingLevel3;
    }

    bool ShouldPauseForLevel4()
    {
        return pauseGameWhenChoosingLevel4;
    }

    void GrowPlayer()
    {
        if (playerModel != null)
        {
            playerModel.localScale *= 1.2f;
        }
    }

    void IncreaseHealth()
    {
        if (playerHealth != null)
        {
            playerHealth.maxHealth += 20;
            playerHealth.currentHealth = playerHealth.maxHealth;
        }
    }
}