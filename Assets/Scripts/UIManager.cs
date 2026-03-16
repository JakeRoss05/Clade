using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public Energy playerEnergy;
    public PlayerHealth playerHealth;
    public PlayerLevel playerLevel;
    public PlayerShield playerShield;

    public Slider energySlider;
    public Slider foodSlider;
    public Slider HealthSlider;

    [Header("Shield UI")]
    public GameObject shieldChargePanel;
    public UnityEngine.UI.Image[] shieldChargeIcons;
    public Color chargeActiveColor = new Color(0f, 0.7f, 1f, 1f);
    public Color chargeEmptyColor = Color.white;

    [Header("Shield Prompt")]
    public TextMeshProUGUI shieldPrompt;
    public Vector3 shieldPromptOffset = new Vector3(0f, 3f, 0f);

    [Header("Unlock Notification")]
    public GameObject shieldUnlockNotification;
    public float notificationDuration = 3f;

    private bool hasShownShieldUnlock = false;
    private Transform playerTransform;
    private RectTransform shieldPromptRect;
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerEnergy = player.GetComponent<Energy>();
            playerHealth = player.GetComponent<PlayerHealth>();
            playerLevel = player.GetComponent<PlayerLevel>();
            playerShield = player.GetComponent<PlayerShield>();
            playerTransform = player.transform;
        }

        if (shieldPrompt != null)
        {
            shieldPromptRect = shieldPrompt.GetComponent<RectTransform>();
            shieldPrompt.gameObject.SetActive(false);
        }

        // Auto-find sliders if not assigned
        if (energySlider == null)
            energySlider = transform.Find("EnergyBar")?.GetComponent<Slider>();
        if (foodSlider == null)
            foodSlider = transform.Find("LevelProgressBar")?.GetComponent<Slider>();
        if (HealthSlider == null)
            HealthSlider = transform.Find("HealthBar")?.GetComponent<Slider>();

        // Hide shield UI until unlocked
        if (shieldChargePanel != null)
            shieldChargePanel.SetActive(false);
        if (shieldUnlockNotification != null)
            shieldUnlockNotification.SetActive(false);
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

        UpdateShieldUI();
    }

    void LateUpdate()
    {
        UpdateShieldPromptPosition();
    }

    void UpdateShieldUI()
    {
        if (playerShield == null)
            return;

        // Show unlock notification once
        if (playerShield.shieldUnlocked && !hasShownShieldUnlock)
        {
            hasShownShieldUnlock = true;

            if (shieldChargePanel != null)
                shieldChargePanel.SetActive(true);

            if (shieldUnlockNotification != null)
            {
                shieldUnlockNotification.SetActive(true);
                Invoke(nameof(HideUnlockNotification), notificationDuration);
            }
        }

        // Hide charge panel if shield not unlocked
        if (!playerShield.shieldUnlocked)
            return;

        // Update shield prompt visibility
        if (shieldPrompt != null)
        {
            bool canActivate = !playerShield.shieldActive
                && playerShield.currentShieldCharges > 0
                && playerEnergy != null
                && playerEnergy.currentEnergy >= playerShield.shieldEnergyCost;

            shieldPrompt.gameObject.SetActive(canActivate);
        }

        // Update charge icons
        if (shieldChargeIcons == null)
            return;

        for (int i = 0; i < shieldChargeIcons.Length; i++)
        {
            if (shieldChargeIcons[i] == null)
                continue;

            // Show only icons up to max charges
            shieldChargeIcons[i].gameObject.SetActive(i < playerShield.maxShieldCharges);

            // Blue = has charge, White = used
            shieldChargeIcons[i].color = i < playerShield.currentShieldCharges
                ? chargeActiveColor
                : chargeEmptyColor;
        }
    }

    void UpdateShieldPromptPosition()
    {
        if (shieldPrompt == null || !shieldPrompt.gameObject.activeSelf)
            return;

        if (playerTransform == null || shieldPromptRect == null || mainCam == null)
            return;

        Vector3 worldPos = playerTransform.position + shieldPromptOffset;
        Vector3 screenPos = mainCam.WorldToScreenPoint(worldPos);

        // Only show if in front of camera
        if (screenPos.z > 0)
        {
            shieldPromptRect.position = screenPos;
        }
    }

    void HideUnlockNotification()
    {
        if (shieldUnlockNotification != null)
            shieldUnlockNotification.SetActive(false);
    }
}
