using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class EvolutionChoiceTooltip : MonoBehaviour
{
    const int TooltipSortingOrder = 5000;

    [Header("UI References")]
    public RectTransform tooltipRoot;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI statsText;

    [Header("Follow Cursor")]
    public Vector2 screenOffset = Vector2.zero;

    [Header("Text Colors")]
    public Color statsColor = Color.green;
    public Color negativeStatsColor = Color.red;

    private Canvas parentCanvas;
    private Canvas tooltipCanvas;
    private Camera uiCamera;
    private bool isVisible;

    void Awake()
    {
        if (tooltipRoot == null)
        {
            tooltipRoot = transform as RectTransform;
        }

        tooltipCanvas = GetComponent<Canvas>();
        if (tooltipCanvas == null && tooltipRoot != null)
        {
            tooltipCanvas = tooltipRoot.gameObject.AddComponent<Canvas>();
        }

        if (tooltipCanvas != null)
        {
            tooltipCanvas.overrideSorting = true;
            tooltipCanvas.sortingOrder = TooltipSortingOrder;
        }

        CanvasGroup group = GetComponent<CanvasGroup>();
        if (group != null)
        {
            group.blocksRaycasts = false;
            group.interactable = false;
        }

        Graphic[] graphics = GetComponentsInChildren<Graphic>(true);
        for (int i = 0; i < graphics.Length; i++)
        {
            if (graphics[i] != null)
            {
                graphics[i].raycastTarget = false;
            }
        }

        parentCanvas = tooltipRoot != null && tooltipRoot.parent != null
            ? tooltipRoot.parent.GetComponentInParent<Canvas>()
            : null;
        if (parentCanvas != null)
        {
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
        {
            tooltipRoot.gameObject.SetActive(true);
            tooltipRoot.SetAsLastSibling();
        }

        if (tooltipCanvas != null)
        {
            tooltipCanvas.overrideSorting = true;
            tooltipCanvas.sortingOrder = TooltipSortingOrder;
        }

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

        if (parentCanvas == null || parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            tooltipRoot.position = screenPosition;
            return;
        }

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(parentCanvas.transform as RectTransform, screenPosition, uiCamera, out Vector3 worldPoint))
        {
            tooltipRoot.position = worldPoint;
        }
    }

    bool IsNegativeStat(string optionStats)
    {
        if (string.IsNullOrWhiteSpace(optionStats))
            return false;

        string trimmed = optionStats.TrimStart();
        return trimmed.StartsWith("-") || trimmed.StartsWith("−");
    }
}
