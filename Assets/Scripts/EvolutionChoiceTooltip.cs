using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class EvolutionChoiceTooltip : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform tooltipRoot;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI statsText;

    [Header("Follow Cursor")]
    public Vector2 screenOffset = new Vector2(18f, -18f);

    [Header("Text Colors")]
    public Color statsColor = Color.green;
    public Color negativeStatsColor = Color.red;

    private Canvas parentCanvas;
    private RectTransform canvasRect;
    private Camera uiCamera;
    private bool isVisible;

    void Awake()
    {
        if (tooltipRoot == null)
        {
            tooltipRoot = transform as RectTransform;
        }

        CanvasGroup group = GetComponent<CanvasGroup>();
        if (group != null)
        {
            group.blocksRaycasts = false;
            group.interactable = false;
        }

        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            canvasRect = parentCanvas.GetComponent<RectTransform>();
            uiCamera = parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : parentCanvas.worldCamera;
        }

        Hide();
    }

    void LateUpdate()
    {
        if (!isVisible || tooltipRoot == null)
            return;

        FollowCursor();
    }

    public void Show(string optionTitle, string optionDescription, string optionStats)
    {
        if (titleText != null)
            titleText.text = optionTitle;

        if (descriptionText != null)
            descriptionText.text = optionDescription;

        if (statsText != null)
        {
            statsText.text = optionStats;
            statsText.color = IsNegativeStat(optionStats) ? negativeStatsColor : statsColor;
        }

        isVisible = true;

        if (tooltipRoot != null)
            tooltipRoot.gameObject.SetActive(true);

        FollowCursor();
    }

    public void Hide()
    {
        isVisible = false;

        if (tooltipRoot != null)
            tooltipRoot.gameObject.SetActive(false);
    }

    void FollowCursor()
    {
        if (tooltipRoot == null || !isVisible)
            return;

        Vector2 mousePos;
        try
        {
            mousePos = Mouse.current.position.ReadValue();
        }
        catch
        {
            return;
        }

        Vector2 screenPosition = (Vector2)mousePos + screenOffset;

        if (canvasRect == null)
        {
            if (tooltipRoot != null)
                tooltipRoot.position = screenPosition;
            return;
        }

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, uiCamera, out Vector2 localPoint))
            return;

        Vector2 size = tooltipRoot.rect.size;
        Vector2 pivot = tooltipRoot.pivot;

        float halfWidth = canvasRect.rect.width * 0.5f;
        float halfHeight = canvasRect.rect.height * 0.5f;

        float minX = -halfWidth + (size.x * pivot.x);
        float maxX = halfWidth - (size.x * (1f - pivot.x));
        float minY = -halfHeight + (size.y * pivot.y);
        float maxY = halfHeight - (size.y * (1f - pivot.y));

        localPoint.x = Mathf.Clamp(localPoint.x, minX, maxX);
        localPoint.y = Mathf.Clamp(localPoint.y, minY, maxY);

        tooltipRoot.anchoredPosition = localPoint;
    }

    bool IsNegativeStat(string optionStats)
    {
        if (string.IsNullOrWhiteSpace(optionStats))
            return false;

        string trimmed = optionStats.TrimStart();
        return trimmed.StartsWith("-") || trimmed.StartsWith("−");
    }
}
