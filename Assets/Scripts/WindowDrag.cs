using UnityEngine;
using UnityEngine.EventSystems;

public class WindowDrag : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    private RectTransform window;
    private Vector2 offset;
    private bool isDragging;
    private bool isHovering;

    void Start()
    {
        window = transform.parent.GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isHovering)
            return;

        isHovering = true;
        if (UIManager.Instance != null)
            UIManager.Instance.BeginDragHover();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isHovering)
            return;

        isHovering = false;
        if (UIManager.Instance != null)
            UIManager.Instance.EndDragHover();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            window,
            eventData.position,
            eventData.pressEventCamera,
            out offset
        );

        if (UIManager.Instance != null)
            UIManager.Instance.BeginWindowDrag();
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            window.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint))
        {
            window.localPosition = localPoint - offset;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        if (UIManager.Instance != null)
            UIManager.Instance.EndWindowDrag();
    }

    void OnDisable()
    {
        if (isHovering && UIManager.Instance != null)
            UIManager.Instance.EndDragHover();

        if (isDragging && UIManager.Instance != null)
            UIManager.Instance.EndWindowDrag();

        isHovering = false;
        isDragging = false;
    }
}