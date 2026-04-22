using UnityEngine;

public class PlayerLevel : MonoBehaviour
{
    public int level = 1;

    public int foodCollected = 0;
    public int foodToLevelUp = 5;

    public PlayerHealth playerHealth;
    public PlayerShield playerShield;
    public PlayerCombat playerCombat;

    public Transform playerModel;

    [Header("Level 3 Upgrade Choice")]
    public GameObject level3ChoiceWindow;
    public bool pauseGameWhenChoosing = true;

    private bool isAwaitingLevel3Choice;

    void Start()
    {
        isAwaitingLevel3Choice = false;

        if (level3ChoiceWindow != null)
        {
            level3ChoiceWindow.SetActive(false);
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
    }

    void ShowLevel3ChoiceWindow()
    {
        isAwaitingLevel3Choice = true;

        if (level3ChoiceWindow != null)
        {
            level3ChoiceWindow.SetActive(true);
        }

        if (pauseGameWhenChoosing)
        {
            Time.timeScale = 0f;
        }

        Debug.Log("Level 3 reached! Waiting for player upgrade choice.");
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

    void CompleteLevel3Choice()
    {
        isAwaitingLevel3Choice = false;

        if (level3ChoiceWindow != null)
        {
            level3ChoiceWindow.SetActive(false);
        }

        if (pauseGameWhenChoosing)
        {
            Time.timeScale = 1f;
        }
    }

    void OnDisable()
    {
        if (pauseGameWhenChoosing && Time.timeScale == 0f)
        {
            Time.timeScale = 1f;
        }
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