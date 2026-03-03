using System.Drawing;
using UnityEngine;

public class Energy : MonoBehaviour
{
    private bool canReproduce = true;
    private PlayerMovement playerMovement;
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
        
        if (currentEnergy >= reprodctionThreshold && canReproduce)
        {
            Reproduce();
            canReproduce = false;
        }

        if (currentEnergy < reprodctionThreshold * 0.5f)
        {
            canReproduce = true;
        }
    } 

    void DrainEnergy()
    {
        float sizeFactor = playerMovement != null ? playerMovement.sizeMultiplier : 1f;

        currentEnergy -= energyDrainPerSecond * sizeFactor * Time.deltaTime;
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

        if (playerMovement != null)
        {
            playerMovement.sizeMultiplier += 0.2f;
        }
        
        maxEnergy += 10f;
    }

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
    } 
}