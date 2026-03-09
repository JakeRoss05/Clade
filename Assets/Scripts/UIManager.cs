using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
public class UIManager : MonoBehaviour
{
    public Energy playerEnergy;
    public PlayerHealth playerHealth;
    public PlayerLevel playerLevel;
    public Slider energySlider;
    public Slider foodSlider;
    public Slider HealthSlider;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerEnergy = player.GetComponent<Energy>();
            playerHealth = player.GetComponent<PlayerHealth>();
            playerLevel = player.GetComponent<PlayerLevel>();
        }

        // Auto-find sliders if not assigned
        if (energySlider == null)
            energySlider = transform.Find("EnergyBar")?.GetComponent<Slider>();
        if (foodSlider == null)
            foodSlider = transform.Find("LevelProgressBar")?.GetComponent<Slider>();
        if (HealthSlider == null)
            HealthSlider = transform.Find("HealthBar")?.GetComponent<Slider>();
    }

    void Update()
    {
        if (playerEnergy != null && energySlider != null)
        {
            energySlider.maxValue = playerEnergy.maxEnergy;
            energySlider.value = playerEnergy.currentEnergy;
        }

        if (playerLevel != null && foodSlider != null)
        {
            foodSlider.maxValue = playerLevel.foodToLevelUp;
            foodSlider.value = playerLevel.foodCollected;
        }

        if (playerHealth != null && HealthSlider != null)
        {
            HealthSlider.maxValue = playerHealth.maxHealth;
            HealthSlider.value = playerHealth.currentHealth;
        }
    }
}
