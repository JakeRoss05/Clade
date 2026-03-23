using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class MenuTextFX : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public TextMeshProUGUI text;

    private Vector3 originalScale;
    private Color originalColor;

    public Color hoverColor = new Color(0.8f, 0.9f, 1f);
    public float hoverScale = 1.08f;
    void Start()
    {
        originalScale = transform.localScale;
        originalColor = text.color;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        text.color = hoverColor;
        transform.localScale = originalScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        text.color = originalColor;
        transform.localScale = originalScale;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        StartCoroutine(ClickAnim());
    }

    System.Collections.IEnumerator ClickAnim()
    {
        transform.localScale = originalScale * 0.9f;
        yield return new WaitForSeconds(0.05f);
        transform.localScale = originalScale;
    }
}