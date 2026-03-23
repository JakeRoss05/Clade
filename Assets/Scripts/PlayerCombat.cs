using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Combat")]
    public bool combatUnlocked = false;
    public float attackDamage = 10f;
    public float attackRange = 2f;
    public float attackCooldown = 0.5f;
    public int shieldChargesPerKill = 1;

    private float lastAttackTime;
    private PlayerShield playerShield;

    void Start()
    {
        playerShield = GetComponent<PlayerShield>();
    }

    void Update()
    {
        if (!combatUnlocked)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame && Time.time - lastAttackTime >= attackCooldown)
        {
            Attack();
            lastAttackTime = Time.time;
        }
    }

    void Attack()
    {
        Debug.Log("Player attacks!");

        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange);
        foreach (Collider hit in hits)
        {
            MicrobeEnemy enemy = hit.GetComponent<MicrobeEnemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(attackDamage);

                // If the enemy dies from this hit, refill shield charges
                if (enemy.health <= 0 && playerShield != null)
                {
                    playerShield.RefillCharges(shieldChargesPerKill);
                }
            }
        }
    }

    public void Unlock()
    {
        combatUnlocked = true;
        Debug.Log("Combat ability unlocked!");
    }
}