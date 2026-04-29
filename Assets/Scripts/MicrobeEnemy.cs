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
                damage = 5f;
                health = 20f;
                maxHealth = 20f;
                speed = 2f;
                break;
            case EnemyTier.Medium:
                damage = 10f;
                health = 40f;
                maxHealth = 40f;
                speed = 3f;
                break;
            case EnemyTier.Max:
                damage = 20f;
                health = 80f;
                maxHealth = 80f;
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
