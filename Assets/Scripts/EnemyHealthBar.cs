using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("Health Bar")]
    public Canvas targetCanvas;
    public Slider healthBarPrefab;
    public Vector3 worldOffset = new Vector3(0f, 2.5f, 0f);

    [Header("Fallback Bar (If No Prefab)")]
    public Vector2 barSize = new Vector2(120f, 14f);
    public Color backgroundColor = new Color(0f, 0f, 0f, 0.6f);
    public Color fillColor = new Color(0.9f, 0.1f, 0.1f, 1f);

    private MicrobeEnemy enemy;
    private Camera mainCamera;
    private Slider spawnedBar;
    private RectTransform spawnedBarRect;
    private bool forceHidden;

    void Awake()
    {
        enemy = GetComponent<MicrobeEnemy>();
        mainCamera = Camera.main;
        EnsureCanvas();
        BuildBarIfNeeded();
        Refresh();
    }

    void LateUpdate()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (enemy == null || mainCamera == null || spawnedBar == null)
            return;

        if (spawnedBarRect == null)
            spawnedBarRect = spawnedBar.GetComponent<RectTransform>();

        Vector3 worldPosition = transform.position + worldOffset;
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
        bool visible = screenPosition.z > 0f && !forceHidden;

        if (spawnedBar.gameObject.activeSelf != visible)
            spawnedBar.gameObject.SetActive(visible);

        if (visible)
        {
            spawnedBarRect.position = new Vector3(screenPosition.x, screenPosition.y, 0f);
        }

        Refresh();
    }

    public void Refresh()
    {
        if (spawnedBar == null || enemy == null)
            return;

        spawnedBar.value = enemy.health;
        spawnedBar.maxValue = Mathf.Max(1f, enemy.maxHealth);
    }

    public void SetVisible(bool visible)
    {
        forceHidden = !visible;
        if (spawnedBar != null)
            spawnedBar.gameObject.SetActive(visible);
    }

    void BuildBarIfNeeded()
    {
        if (targetCanvas == null)
            return;

        if (spawnedBar != null)
            return;

        if (healthBarPrefab != null)
        {
            spawnedBar = Instantiate(healthBarPrefab, targetCanvas.transform);
            spawnedBarRect = spawnedBar.GetComponent<RectTransform>();
            spawnedBar.name = "EnemyHealthBarUI";
            spawnedBar.minValue = 0f;
            spawnedBar.maxValue = Mathf.Max(1f, enemy != null ? enemy.maxHealth : 1f);
            spawnedBar.value = enemy != null ? enemy.health : 0f;
        }

        if (spawnedBar == null)
        {
            spawnedBar = CreateFallbackBar(targetCanvas.transform);
            spawnedBarRect = spawnedBar.GetComponent<RectTransform>();
            spawnedBar.name = "EnemyHealthBarUI";
        }
    }

    Slider CreateFallbackBar(Transform parent)
    {
        GameObject root = new GameObject("EnemyHealthBarRoot");
        root.transform.SetParent(parent, false);

        RectTransform rootRect = root.AddComponent<RectTransform>();
        rootRect.sizeDelta = barSize;
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);

        Slider slider = root.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        slider.interactable = false;

        GameObject background = new GameObject("Background");
        background.transform.SetParent(root.transform, false);
        Image backgroundImage = background.AddComponent<Image>();
        backgroundImage.color = backgroundColor;

        RectTransform backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(background.transform, false);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = fillColor;

        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.targetGraphic = fillImage;
        slider.direction = Slider.Direction.LeftToRight;

        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        return slider;
    }

    void EnsureCanvas()
    {
        if (targetCanvas != null)
            return;

        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < canvases.Length; i++)
        {
            if (canvases[i].renderMode == RenderMode.ScreenSpaceOverlay || canvases[i].renderMode == RenderMode.ScreenSpaceCamera)
            {
                targetCanvas = canvases[i];
                return;
            }
        }
    }

    void OnDestroy()
    {
        if (spawnedBar != null)
            Destroy(spawnedBar.gameObject);
    }
}