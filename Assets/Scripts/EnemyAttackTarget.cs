using UnityEngine;
using UnityEngine.EventSystems;

public class EnemyAttackTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private MicrobeEnemy enemy;
    private PlayerCombat playerCombat;
    private bool isHovering;
    private bool hasSwordRequest;

    void Awake()
    {
        enemy = GetComponent<MicrobeEnemy>();
    }

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerCombat = player.GetComponent<PlayerCombat>();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;

        if (enemy != null)
            enemy.SetHighlighted(true);

        UpdateSwordRequest();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;

        if (enemy != null)
            enemy.SetHighlighted(false);

        ClearSwordRequest();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (playerCombat == null || enemy == null)
            return;

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (playerCombat.TryAttackTarget(enemy))
        {
            enemy.SetSelected(true);
        }
    }

    void OnDisable()
    {
        isHovering = false;
        ClearSwordRequest();

        if (enemy != null)
        {
            enemy.SetHighlighted(false);
            enemy.SetSelected(false);
        }
    }

    void Update()
    {
        if (!isHovering)
            return;

        UpdateSwordRequest();
    }

    void UpdateSwordRequest()
    {
        bool canShowSword = UIManager.Instance != null
            && playerCombat != null
            && enemy != null
            && playerCombat.CanAttackTarget(enemy);

        if (canShowSword && !hasSwordRequest)
        {
            UIManager.Instance.SetSwordHover(true);
            hasSwordRequest = true;
        }
        else if (!canShowSword && hasSwordRequest)
        {
            ClearSwordRequest();
        }
    }

    void ClearSwordRequest()
    {
        if (!hasSwordRequest)
            return;

        if (UIManager.Instance != null)
            UIManager.Instance.SetSwordHover(false);

        hasSwordRequest = false;
    }
}