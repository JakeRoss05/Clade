using UnityEngine;

public class MicrobeEnemy : MonoBehaviour
{
    public float speed = 2f;
    public float detectionRange = 10f;
    public float wanderSpeed = 1f;
    public float damage = 2f;
    public float attackCooldown = 1f;

    private float lastAttackTime;
    private Vector3 wanderDirection;

    private Transform player;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        wanderDirection = Random.insideUnitSphere;
        wanderDirection.y = 0;
        wanderDirection.Normalize();
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
