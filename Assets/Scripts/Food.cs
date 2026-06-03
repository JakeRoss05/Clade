using UnityEngine;

public class Food : MonoBehaviour
{
    [Header("Food Effects")]
    public float energyValue = 25f;
    public float sizeIncrease = 0.1f;
    public int xpvalue = 1;

    private bool consumed;

    private void Awake()
    {
        EnsureConsumePhysics();
    }

    private void EnsureConsumePhysics()
    {
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        if (rigidbody == null)
        {
            rigidbody = gameObject.AddComponent<Rigidbody>();
        }

        rigidbody.useGravity = false;
        rigidbody.isKinematic = true;

        Collider[] colliders = GetComponentsInChildren<Collider>();
        if (colliders.Length == 0)
        {
            SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            sphereCollider.center = Vector3.zero;

            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                Bounds bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }

                sphereCollider.center = transform.InverseTransformPoint(bounds.center);

                float maxScale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
                if (maxScale > 0f)
                {
                    sphereCollider.radius = Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z) / maxScale;
                }
            }
            else
            {
                sphereCollider.radius = 0.5f;
            }

            return;
        }

        foreach (Collider collider in colliders)
        {
            collider.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        TryConsume(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryConsume(collision.collider);
    }

    private void TryConsume(Collider other)
    {
        if (consumed || other == null)
            return;

        Energy energy = other.GetComponentInParent<Energy>();
        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        PlayerLevel level = other.GetComponentInParent<PlayerLevel>();
        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();

        // Treat an object as the player if any core player gameplay component exists on it.
        if (energy == null && health == null && level == null && player == null)
            return;

        consumed = true;

        if (energy != null)
        {
            energy.AddEnergy(energyValue);
        }

        if (health != null)
        {
            float healAmount = health.healthFromFood > 0f ? health.healthFromFood : energyValue;
            health.Heal(healAmount);
        }

        if (level != null)
        {
            level.AddFood(xpvalue);
        }

        if (player != null)
        {
            player.sizeMultiplier += sizeIncrease;
        }

        Destroy(gameObject);
    }
}

