using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerCombat : MonoBehaviour
{
    [Header("Combat")]
    public bool combatUnlocked = false;
    public float attackDamage = 10f;
    public float attackRange = 2f;
    public float attackCooldown = 0.5f;
    public int shieldChargesPerKill = 1;

    [Header("Attack Windup")]
    public float attackWindupTime = 0.2f;
    public Animator combatAnimator;
    public string attackTriggerName = "Attack";

    private float lastAttackTime;
    private PlayerShield playerShield;
    private bool isWindingUp;

    void Start()
    {
        playerShield = GetComponent<PlayerShield>();
        EnsureCameraRaycaster();
    }

    public bool TryAttackTarget(MicrobeEnemy target)
    {
        if (!CanAttackTarget(target))
            return false;

        lastAttackTime = Time.time;
        StartCoroutine(WindupAndAttack(target));

        return true;
    }

    public bool CanAttackTarget(MicrobeEnemy target)
    {
        if (!combatUnlocked)
            return false;

        if (target == null)
            return false;

        if (isWindingUp)
            return false;

        if (Time.time - lastAttackTime < attackCooldown)
            return false;

        float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
        return distanceToTarget <= attackRange;
    }

    IEnumerator WindupAndAttack(MicrobeEnemy target)
    {
        isWindingUp = true;

        if (combatAnimator != null && !string.IsNullOrEmpty(attackTriggerName))
        {
            combatAnimator.SetTrigger(attackTriggerName);
        }

        float elapsed = 0f;
        while (elapsed < attackWindupTime)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (target == null)
        {
            isWindingUp = false;
            yield break;
        }

        float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
        if (distanceToTarget > attackRange)
        {
            isWindingUp = false;
            yield break;
        }

        Debug.Log("Player attacks " + target.name + "!");

        float targetHealthBeforeHit = target.health;
        target.TakeDamage(attackDamage);

        if (targetHealthBeforeHit > 0f && target.health <= 0f && playerShield != null)
        {
            playerShield.RefillCharges(shieldChargesPerKill);
        }

        isWindingUp = false;
    }

    public void Unlock()
    {
        combatUnlocked = true;
        Debug.Log("Combat ability unlocked!");
    }

    public bool IsAttackCoolingDown()
    {
        return combatUnlocked && Time.time - lastAttackTime < attackCooldown;
    }

    public float GetAttackCooldownRemaining()
    {
        if (!combatUnlocked)
            return 0f;

        float remaining = attackCooldown - (Time.time - lastAttackTime);
        return Mathf.Max(0f, remaining);
    }

    void EnsureCameraRaycaster()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
            return;

        if (mainCamera.GetComponent<PhysicsRaycaster>() == null)
            mainCamera.gameObject.AddComponent<PhysicsRaycaster>();
    }
}