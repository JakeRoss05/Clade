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

    [Header("Level 5 Upgrade Choice")]
    public GameObject level5ChoiceWindow;
    public bool pauseGameWhenChoosingLevel5 = true;

    private bool isAwaitingLevel3Choice;
    private bool isAwaitingLevel4Choice;
    private bool isAwaitingLevel5Choice;

    void Start()
    {
        isAwaitingLevel3Choice = false;
        isAwaitingLevel4Choice = false;
        isAwaitingLevel5Choice = false;

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

        if (level5ChoiceWindow != null)
        {
            level5ChoiceWindow.SetActive(false);
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
        else if (level == 5)
        {
            ShowLevel5ChoiceWindow();
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

    void ShowLevel5ChoiceWindow()
    {
        isAwaitingLevel5Choice = true;

        if (level5ChoiceWindow != null)
        {
            level5ChoiceWindow.SetActive(true);
        }

        if (ShouldPauseForLevel5())
        {
            Time.timeScale = 0f;
        }

        Debug.Log("Level 5 reached! Waiting for player upgrade choice.");
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

    void UpgradeCombatMastery()
    {
        if (playerCombat != null)
        {
            if (!playerCombat.combatUnlocked)
            {
                playerCombat.Unlock();
            }

            playerCombat.attackDamage += 6f;
            playerCombat.attackCooldown = Mathf.Max(0.2f, playerCombat.attackCooldown - 0.1f);
        }
    }

    void UpgradeShieldMastery()
    {
        if (playerShield != null)
        {
            playerShield.UpgradeCharges(4);
            playerShield.shieldDuration = Mathf.Max(playerShield.shieldDuration, 4f);
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

    public void ChooseCombatMastery()
    {
        if (!isAwaitingLevel5Choice)
            return;

        // At level 5 this choice unlocks a dash ability (space) with visual effects
        PlayerMovement pm = GetComponent<PlayerMovement>();
        if (pm != null)
        {
            pm.EnableDash();
            Debug.Log("Dash ability unlocked at level 5.");
        }

        CompleteLevel5Choice();
    }

    public void ChooseShieldMastery()
    {
        if (!isAwaitingLevel5Choice)
            return;

        UpgradeShieldMastery();
        CompleteLevel5Choice();
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

    void CompleteLevel5Choice()
    {
        isAwaitingLevel5Choice = false;

        if (level5ChoiceWindow != null)
        {
            level5ChoiceWindow.SetActive(false);
        }

        if (ShouldPauseForLevel5())
        {
            Time.timeScale = 1f;
        }
    }

    void OnDisable()
    {
        if ((ShouldPauseForLevel3() || ShouldPauseForLevel4() || ShouldPauseForLevel5()) && Time.timeScale == 0f)
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

    bool ShouldPauseForLevel5()
    {
        return pauseGameWhenChoosingLevel5;
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