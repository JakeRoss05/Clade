using UnityEngine;
using UnityEngine.EventSystems;

public class MicrobeEnemy : MonoBehaviour
{
    public enum EnemyTier { Weak, Medium, Max }

    public EnemyTier tier = EnemyTier.Weak;

    public float speed = 2f;
    public float detectionRange = 10f;
    public float wanderSpeed = 1f;
    public float damage = 5f;
    public float attackCooldown = 1f;
    public float maxHealth = 20f;
    public float health = 20f;

    [Header("Hover Highlight")]
    public Color hoverTint = new Color(1f, 1f, 0.6f, 1f);
    public Color selectedTint = new Color(1f, 0.9f, 0.35f, 1f);

    private float lastAttackTime;
    private Vector3 wanderDirection;

    private Transform player;
    private Rigidbody enemyRigidbody;
    private Collider enemyCollider;
    private Renderer[] cachedRenderers;
    private Material[][] cachedMaterials;
    private Color[][] cachedMaterialColors;
    private bool isHighlighted;
    private bool isSelected;
    private EnemyHealthBar healthBar;
    private EnemyAttackTarget attackTarget;

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

        CacheVisuals();
        EnsureSupportComponents();
    }

    void ApplyTierStats()
    {
        switch (tier)
        {
            case EnemyTier.Weak:
                damage = 6f;
                health = 20f;
                maxHealth = 20f;
                speed = 2f;
                break;
            case EnemyTier.Medium:
                damage = 12f;
                health = 40f;
                maxHealth = 40f;
                speed = 3f;
                break;
            case EnemyTier.Max:
                damage = 24f;
                health = 80f;
                maxHealth = 80f;
                speed = 4f;
                break;
        }
    }

    public void SetTier(EnemyTier newTier)
    {
        tier = newTier;
        ApplyTierStats();

        if (healthBar != null)
            healthBar.Refresh();
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
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.0001f)
        {
            direction.Normalize();
        }

        transform.position += direction * speed * Time.deltaTime;

        Vector3 position = transform.position;
        position.y = 0.5f;
        transform.position = position;

        FaceFlatDirection(direction);
    }

    void Wander()
    {
        transform.position += wanderDirection * wanderSpeed * Time.deltaTime;

        Vector3 position = transform.position;
        position.y = 0.5f;
        transform.position = position;

        FaceFlatDirection(wanderDirection);

        if (Random.Range(0f, 1f) < 0.01f)
        {
            wanderDirection = Random.insideUnitSphere;
            wanderDirection.y = 0;
            wanderDirection.Normalize();
        }
    }

    void FaceFlatDirection(Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f)
            return;

        transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (healthBar != null)
            healthBar.Refresh();

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

    public float GetHealthNormalized()
    {
        if (maxHealth <= 0f)
            return 0f;

        return Mathf.Clamp01(health / maxHealth);
    }

    public void SetHighlighted(bool highlighted)
    {
        isHighlighted = highlighted;
        ApplyVisualState();
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        ApplyVisualState();
    }

    void CacheVisuals()
    {
        cachedRenderers = GetComponentsInChildren<Renderer>(true);
        cachedMaterials = new Material[cachedRenderers.Length][];
        cachedMaterialColors = new Color[cachedRenderers.Length][];

        for (int i = 0; i < cachedRenderers.Length; i++)
        {
            Renderer renderer = cachedRenderers[i];
            Material[] instanceMaterials = renderer.materials;
            cachedMaterials[i] = instanceMaterials;
            cachedMaterialColors[i] = new Color[instanceMaterials.Length];

            for (int j = 0; j < instanceMaterials.Length; j++)
            {
                Material material = instanceMaterials[j];
                cachedMaterialColors[i][j] = GetMaterialColor(material);
            }
        }

        ApplyVisualState();
    }

    void EnsureSupportComponents()
    {
        enemyRigidbody = GetComponent<Rigidbody>();
        if (enemyRigidbody != null)
        {
            enemyRigidbody.useGravity = false;
            enemyRigidbody.isKinematic = true;
        }

        enemyCollider = GetComponent<Collider>();
        if (enemyCollider != null)
            enemyCollider.isTrigger = true;

        if (healthBar == null)
            healthBar = GetComponent<EnemyHealthBar>();

        if (healthBar == null)
            healthBar = gameObject.AddComponent<EnemyHealthBar>();

        if (attackTarget == null)
            attackTarget = GetComponent<EnemyAttackTarget>();

        if (attackTarget == null)
            attackTarget = gameObject.AddComponent<EnemyAttackTarget>();
    }

    void ApplyVisualState()
    {
        if (cachedMaterials == null)
            return;

        bool shouldTint = isSelected || isHighlighted;
        Color tint = isSelected ? selectedTint : hoverTint;

        for (int i = 0; i < cachedMaterials.Length; i++)
        {
            Material[] materials = cachedMaterials[i];
            if (materials == null)
                continue;

            for (int j = 0; j < materials.Length; j++)
            {
                Material material = materials[j];
                if (material == null)
                    continue;

                Color targetColor = shouldTint ? tint : cachedMaterialColors[i][j];
                targetColor.a = cachedMaterialColors[i][j].a;

                if (material.HasProperty("_BaseColor"))
                    material.SetColor("_BaseColor", targetColor);

                if (material.HasProperty("_Color"))
                    material.SetColor("_Color", targetColor);
            }
        }
    }

    Color GetMaterialColor(Material material)
    {
        if (material == null)
            return Color.white;

        if (material.HasProperty("_BaseColor"))
            return material.GetColor("_BaseColor");

        if (material.HasProperty("_Color"))
            return material.GetColor("_Color");

        return Color.white;
    }

    void OnDisable()
    {
        if (healthBar != null)
            healthBar.SetVisible(false);
    }

    private void OnTriggerStay(Collider other)
    {
        TryAttackPlayer(other);
    }

    private void OnCollisionStay(Collision collision)
    {
        TryAttackPlayer(collision.collider);
    }

    private void TryAttackPlayer(Collider other)
    {
        if (other == null || !other.CompareTag("Player") || Time.time - lastAttackTime < attackCooldown)
            return;

        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();
        if (playerHealth == null)
            playerHealth = other.GetComponent<PlayerHealth>();

        if (playerHealth == null)
            return;

        playerHealth.TakeDamage(damage);
        lastAttackTime = Time.time;
    }
}
