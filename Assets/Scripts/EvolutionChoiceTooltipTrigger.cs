using UnityEngine;
using UnityEngine.EventSystems;

public class EvolutionChoiceTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Tooltip")]
    public EvolutionChoiceTooltip tooltip;

    [TextArea(1, 2)]
    public string optionTitle = "Gene";

    [TextArea(2, 6)]
    public string optionDescription = "Describe what this evolution does.";

    [TextArea(2, 6)]
    public string optionStats = "+Stat gains here";

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltip != null)
        {
            tooltip.Show(optionTitle, optionDescription, optionStats);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltip != null)
        {
            tooltip.Hide();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (tooltip != null)
        {
            tooltip.Hide();
        }
    }

    void OnDisable()
    {
        if (tooltip != null)
        {
            tooltip.Hide();
        }
    }
}
