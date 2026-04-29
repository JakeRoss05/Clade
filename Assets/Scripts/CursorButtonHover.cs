using UnityEngine;
using UnityEngine.EventSystems;

public class CursorButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private bool isHovering;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isHovering)
            return;

        isHovering = true;
        if (UIManager.Instance != null)
            UIManager.Instance.BeginUIHover();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isHovering)
            return;

        isHovering = false;
        if (UIManager.Instance != null)
            UIManager.Instance.EndUIHover();
    }

    void OnDisable()
    {
        if (!isHovering)
            return;

        isHovering = false;
        if (UIManager.Instance != null)
            UIManager.Instance.EndUIHover();
    }
}