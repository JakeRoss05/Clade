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
            UnlockCombat();
            UpgradeShield();
            GrowPlayer();
            IncreaseHealth();
        }
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