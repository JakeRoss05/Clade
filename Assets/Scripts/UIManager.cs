using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public enum CursorMode
    {
        Default,
        Pointer,
        HandOpen,
        HandClosed,
        Sword
    }

    [Header("Player References")]
    public Energy playerEnergy;
    public PlayerHealth playerHealth;
    public PlayerLevel playerLevel;
    public PlayerShield playerShield;
    public PlayerCombat playerCombat;

    [Header("HUD Bars")]
    public Slider energySlider;
    public Slider foodSlider;
    public Slider HealthSlider;

    [Header("Cursor UI")]
    public RectTransform cursorRoot;
    public Image cursorImage;
    public RectTransform cursorSpinnerRoot;
    public Image cursorSpinnerImage;
    public bool hideSystemCursor = true;
    public float spinnerRotationSpeed = 540f;
    public Sprite defaultCursorSprite;
    public Sprite cursorPointerSprite;
    public Sprite swordCursorSprite;
    public Sprite handOpenCursorSprite;
    public Sprite handClosedCursorSprite;

    [Header("Shield UI")]
    public GameObject shieldChargePanel;
    public Image[] shieldChargeIcons;
    public Color chargeActiveColor = new Color(0f, 0.7f, 1f, 1f);
    public Color chargeEmptyColor = Color.white;

    [Header("Shield Prompt")]
    public TextMeshProUGUI shieldPrompt;
    public Vector3 shieldPromptOffset = new Vector3(0f, 3f, 0f);
    public float shieldPromptPopupDuration = 0.2f;
    public float shieldPromptFadeDuration = 0.6f;
    public float shieldPromptFloatAmplitude = 10f;
    public float shieldPromptFloatFrequency = 1.5f;

    [Header("Unlock Notification")]
    public GameObject shieldUnlockNotification;
    public float notificationVisibleDuration = 3f;
    public float notificationFadeDuration = 0.5f;

    private CursorMode currentCursorMode = CursorMode.Default;
    private int uiHoverCount;
    private int dragHoverCount;
    private int swordHoverCount;
    private bool isWindowDragging;
    private bool hasShownShieldUnlock;
    private bool hasShownShieldPrompt;
    private bool isShieldPromptRunning;
    private bool isShieldUnlockNotificationRunning;
    private Transform playerTransform;
    private RectTransform shieldPromptRect;
    private CanvasGroup shieldPromptGroup;
    private CanvasGroup shieldUnlockNotificationGroup;
    private Camera mainCam;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

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
            playerCombat = player.GetComponent<PlayerCombat>();
            playerTransform = player.transform;
        }

        if (hideSystemCursor)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.None;
        }

        if (cursorRoot != null)
            cursorRoot.gameObject.SetActive(true);

        if (cursorImage == null && cursorRoot != null)
            cursorImage = cursorRoot.GetComponent<Image>();

        if (cursorImage != null)
            cursorImage.raycastTarget = false;

        if (cursorSpinnerImage == null && cursorSpinnerRoot != null)
            cursorSpinnerImage = cursorSpinnerRoot.GetComponent<Image>();

        if (cursorSpinnerImage != null)
            cursorSpinnerImage.raycastTarget = false;

        ApplyCursorMode(currentCursorMode);

        if (cursorSpinnerRoot != null)
            cursorSpinnerRoot.gameObject.SetActive(false);

        if (shieldPrompt != null)
        {
            shieldPromptRect = shieldPrompt.GetComponent<RectTransform>();
            shieldPromptGroup = shieldPrompt.GetComponent<CanvasGroup>();
            if (shieldPromptGroup == null)
                shieldPromptGroup = shieldPrompt.gameObject.AddComponent<CanvasGroup>();

            shieldPromptGroup.alpha = 0f;
            shieldPrompt.gameObject.SetActive(false);
        }

        if (shieldUnlockNotification != null)
        {
            shieldUnlockNotificationGroup = shieldUnlockNotification.GetComponent<CanvasGroup>();
            if (shieldUnlockNotificationGroup == null)
                shieldUnlockNotificationGroup = shieldUnlockNotification.gameObject.AddComponent<CanvasGroup>();

            shieldUnlockNotificationGroup.alpha = 0f;
            shieldUnlockNotification.SetActive(false);
        }

        if (energySlider == null)
            energySlider = transform.Find("EnergyBar")?.GetComponent<Slider>();
        if (foodSlider == null)
            foodSlider = transform.Find("LevelProgressBar")?.GetComponent<Slider>();
        if (HealthSlider == null)
            HealthSlider = transform.Find("HealthBar")?.GetComponent<Slider>();

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
        ResolveCursorState();
        UpdateCursorSpinner();
    }

    void LateUpdate()
    {
        UpdateShieldPromptPosition();
        UpdateCursorPosition();
    }

    public void SetDefaultCursor()
    {
        SetCursorMode(CursorMode.Default);
    }

    public void SetPointerCursor()
    {
        SetCursorMode(CursorMode.Pointer);
    }

    public void SetHandOpenCursor()
    {
        SetCursorMode(CursorMode.HandOpen);
    }

    public void SetHandClosedCursor()
    {
        SetCursorMode(CursorMode.HandClosed);
    }

    public void SetSwordCursor()
    {
        SetCursorMode(CursorMode.Sword);
    }

    public void SetCursorMode(CursorMode mode)
    {
        currentCursorMode = mode;
        ApplyCursorMode(mode);
    }

    public void BeginUIHover()
    {
        uiHoverCount++;
    }

    public void EndUIHover()
    {
        uiHoverCount = Mathf.Max(0, uiHoverCount - 1);
    }

    public void BeginDragHover()
    {
        dragHoverCount++;
    }

    public void EndDragHover()
    {
        dragHoverCount = Mathf.Max(0, dragHoverCount - 1);
    }

    public void BeginWindowDrag()
    {
        isWindowDragging = true;
    }

    public void EndWindowDrag()
    {
        isWindowDragging = false;
    }

    public void SetSwordHover(bool active)
    {
        if (active)
            swordHoverCount++;
        else
            swordHoverCount = Mathf.Max(0, swordHoverCount - 1);
    }

    void ResolveCursorState()
    {
        CursorMode nextMode = CursorMode.Default;

        if (isWindowDragging)
            nextMode = CursorMode.HandClosed;
        else if (dragHoverCount > 0)
            nextMode = CursorMode.HandOpen;
        else if (swordHoverCount > 0)
            nextMode = CursorMode.Sword;
        else if (uiHoverCount > 0)
            nextMode = CursorMode.Pointer;

        if (nextMode != currentCursorMode)
            SetCursorMode(nextMode);
    }

    void ApplyCursorMode(CursorMode mode)
    {
        if (cursorImage == null)
            return;

        Sprite nextSprite = defaultCursorSprite;

        switch (mode)
        {
            case CursorMode.Pointer:
                nextSprite = cursorPointerSprite != null ? cursorPointerSprite : defaultCursorSprite;
                break;
            case CursorMode.HandOpen:
                nextSprite = handOpenCursorSprite != null ? handOpenCursorSprite : defaultCursorSprite;
                break;
            case CursorMode.HandClosed:
                nextSprite = handClosedCursorSprite != null ? handClosedCursorSprite : defaultCursorSprite;
                break;
            case CursorMode.Sword:
                nextSprite = swordCursorSprite != null ? swordCursorSprite : defaultCursorSprite;
                break;
        }

        if (nextSprite != null)
            cursorImage.sprite = nextSprite;
    }

    void UpdateCursorSpinner()
    {
        if (cursorSpinnerRoot == null || cursorSpinnerImage == null)
            return;

        bool showSpinner = playerCombat != null && playerCombat.IsAttackCoolingDown();
        cursorSpinnerRoot.gameObject.SetActive(showSpinner);

        if (showSpinner)
            cursorSpinnerRoot.Rotate(0f, 0f, -spinnerRotationSpeed * Time.unscaledDeltaTime);
    }

    void UpdateCursorPosition()
    {
        if (cursorRoot == null)
            return;

        if (Mouse.current == null)
            return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        cursorRoot.position = mousePosition;
        cursorRoot.SetAsLastSibling();
    }

    void UpdateShieldUI()
    {
        if (playerShield == null)
            return;

        if (playerShield.shieldUnlocked && !hasShownShieldUnlock)
        {
            hasShownShieldUnlock = true;

            if (shieldChargePanel != null)
                shieldChargePanel.SetActive(true);

            if (shieldUnlockNotification != null && !isShieldUnlockNotificationRunning)
                StartCoroutine(PlayShieldUnlockNotificationPopup());
        }

        if (!playerShield.shieldUnlocked)
            return;

        if (shieldPrompt != null)
        {
            bool canActivate = !playerShield.shieldActive
                && playerShield.currentShieldCharges > 0
                && playerEnergy != null
                && playerEnergy.currentEnergy >= playerShield.shieldEnergyCost;

            if (canActivate && !hasShownShieldPrompt && !isShieldPromptRunning)
                StartCoroutine(PlayShieldPromptPopup());
        }

        if (shieldChargeIcons == null)
            return;

        for (int i = 0; i < shieldChargeIcons.Length; i++)
        {
            if (shieldChargeIcons[i] == null)
                continue;

            shieldChargeIcons[i].gameObject.SetActive(i < playerShield.maxShieldCharges);
            shieldChargeIcons[i].color = i < playerShield.currentShieldCharges ? chargeActiveColor : chargeEmptyColor;
        }
    }

    void UpdateShieldPromptPosition()
    {
        if (shieldPrompt == null || !shieldPrompt.gameObject.activeSelf)
            return;

        if (playerTransform == null || shieldPromptRect == null || mainCam == null)
            return;

        shieldPromptRect.position = GetPromptScreenPosition();
    }

    Vector3 GetPromptScreenPosition()
    {
        if (playerTransform == null || mainCam == null)
            return shieldPromptRect != null ? shieldPromptRect.position : Vector3.zero;

        Vector3 worldPos = playerTransform.position + shieldPromptOffset;
        Vector3 screenPos = mainCam.WorldToScreenPoint(worldPos);
        float floatOffset = Mathf.Sin(Time.unscaledTime * shieldPromptFloatFrequency) * shieldPromptFloatAmplitude;

        return new Vector3(screenPos.x, screenPos.y + floatOffset, screenPos.z);
    }

    IEnumerator PlayShieldPromptPopup()
    {
        if (shieldPrompt == null || shieldPromptRect == null || shieldPromptGroup == null || playerTransform == null)
            yield break;

        isShieldPromptRunning = true;
        hasShownShieldPrompt = true;

        shieldPrompt.gameObject.SetActive(true);

        Vector3 startPosition = GetPromptScreenPosition() + new Vector3(0f, -20f, 0f);
        Vector3 endPosition = GetPromptScreenPosition();
        Vector3 startScale = Vector3.one * 0.85f;
        Vector3 endScale = Vector3.one;

        shieldPromptRect.position = startPosition;
        shieldPromptRect.localScale = startScale;
        shieldPromptGroup.alpha = 0f;

        float elapsed = 0f;
        while (elapsed < shieldPromptPopupDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / shieldPromptPopupDuration);
            shieldPromptGroup.alpha = Mathf.Lerp(0f, 1f, t);
            shieldPromptRect.position = Vector3.Lerp(startPosition, endPosition, t);
            shieldPromptRect.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        shieldPromptGroup.alpha = 1f;
        shieldPromptRect.localScale = endScale;

        while (playerShield != null && !playerShield.shieldActive)
        {
            UpdateShieldPromptPosition();
            yield return null;
        }

        Vector3 fadeStartPosition = shieldPromptRect.position;
        Vector3 fadeEndPosition = fadeStartPosition + new Vector3(0f, 20f, 0f);

        elapsed = 0f;
        while (elapsed < shieldPromptFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / shieldPromptFadeDuration);
            shieldPromptGroup.alpha = Mathf.Lerp(1f, 0f, t);
            shieldPromptRect.position = Vector3.Lerp(fadeStartPosition, fadeEndPosition, t);
            yield return null;
        }

        shieldPromptGroup.alpha = 0f;
        shieldPrompt.gameObject.SetActive(false);
        isShieldPromptRunning = false;
    }

    IEnumerator PlayShieldUnlockNotificationPopup()
    {
        if (shieldUnlockNotification == null || shieldUnlockNotificationGroup == null)
            yield break;

        isShieldUnlockNotificationRunning = true;

        shieldUnlockNotification.SetActive(true);
        shieldUnlockNotificationGroup.alpha = 0f;

        float elapsed = 0f;
        while (elapsed < notificationFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / notificationFadeDuration);
            shieldUnlockNotificationGroup.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }

        shieldUnlockNotificationGroup.alpha = 1f;

        elapsed = 0f;
        while (elapsed < notificationVisibleDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < notificationFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / notificationFadeDuration);
            shieldUnlockNotificationGroup.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }

        shieldUnlockNotificationGroup.alpha = 0f;
        shieldUnlockNotification.SetActive(false);
        isShieldUnlockNotificationRunning = false;
    }
}
