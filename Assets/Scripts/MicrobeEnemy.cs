using UnityEngine;

public class MicrobeEnemy : MonoBehaviour
{
    public enum EnemyTier { Weak, Medium, Max }

    public EnemyTier tier = EnemyTier.Weak;

    public float speed = 2f;
    public float detectionRange = 10f;
    public float wanderSpeed = 1f;
    public float damage = 5.5f;
    public float attackCooldown = 1f;
    public float health = 20f;

    private float lastAttackTime;
    private Vector3 wanderDirection;

    private Transform player;

    void Start()
    {
        ApplyTierStats();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        wanderDirection = Random.insideUnitSphere;
        wanderDirection.y = 0;
        wanderDirection.Normalize();
    }

    void ApplyTierStats()
    {
        switch (tier)
        {
            case EnemyTier.Weak:
                damage = 5.5f;
                health = 20f;
                speed = 2f;
                break;
            case EnemyTier.Medium:
                damage = 11f;
                health = 40f;
                speed = 3f;
                break;
            case EnemyTier.Max:
                damage = 22f;
                health = 80f;
                speed = 4f;
                break;
        }
    }

    void Update()
    {
        if (player == null)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectionRange)
        {
            MoveTowardsPlayer();
        }

        else
        {
            Wander();
        }
    }

    void MoveTowardsPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;

        transform.position += direction * speed * Time.deltaTime;

        transform.LookAt(player);
    }

    void Wander()
    {
        transform.position += wanderDirection * wanderSpeed * Time.deltaTime;

        if (Random.Range(0f, 1f) < 0.01f)
        {
            wanderDirection = Random.insideUnitSphere;
            wanderDirection.y = 0;
            wanderDirection.Normalize();
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Enemy killed!");
        Destroy(gameObject);
    }

    private void OnCollisionStay(Collision other)
    {
        if (other.gameObject.CompareTag("Player") && Time.time - lastAttackTime >= attackCooldown)
        {
            PlayerHealth playerHealth = other.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                lastAttackTime = Time.time;
            }
        }
    }
}
