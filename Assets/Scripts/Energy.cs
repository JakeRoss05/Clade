using UnityEngine;

public class Energy : MonoBehaviour
{
    [Header("Energy")]
    public float maxEnergy = 100f;
    public float currentEnergy = 50f;

    [Header("Metabolism")]
    public float energyDrainPerSecond = 2f;

    [Header("Reproduction")]
    public float reprodctionThreshold = 80f;
    public float reproductionCost = 40f;

    void Update()
    {
        DrainEnergy();

        if (currentEnergy <= 0)
        {
            Die();
        }

        if (currentEnergy >= reprodctionThreshold)
        {
            Reproduce();
        }
    }

    void DrainEnergy()
    {
        currentEnergy -= energyDrainPerSecond * Time.deltaTime;
        currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);
    }

    public void AddEnergy(float amount)
    {
        currentEnergy += amount;
        currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);
    }

    void Die()
    {
        Destroy(gameObject);
    }

    void Reproduce()
    {
        currentEnergy -= reproductionCost;

        GameObject child = Instantiate(
            gameObject,
            transform.position + Random.insideUnitSphere * 1.5f,
            Quaternion.identity
        );

        Energy childEnergy = child.GetComponent<Energy>();
        childEnergy.currentEnergy = currentEnergy * 0.5f;
    }
}