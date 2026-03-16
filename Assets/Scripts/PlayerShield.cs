using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShield : MonoBehaviour
{
    public GameObject shieldVisual;
    public bool shieldUnlocked = false;
    public bool shieldActive = false;
    public float shieldEnergyCost = 15f;
    public float shieldDuration = 3f;

    [Header("Shield Charges")]
    public int maxShieldCharges = 1;
    public int currentShieldCharges = 0;

    private Energy energy;

    void Start()
    {
        energy = GetComponent<Energy>();
    }

    void Update()
    {
        if (!shieldUnlocked)
            return;

        if (Keyboard.current.qKey.wasPressedThisFrame && !shieldActive && currentShieldCharges > 0)
        {
            if (energy != null && energy.currentEnergy >= shieldEnergyCost)
            {
                energy.currentEnergy -= shieldEnergyCost;
                currentShieldCharges--;

                shieldActive = true;

                CancelInvoke(nameof(DisableShield));

                if (shieldVisual != null)
                    shieldVisual.SetActive(true);

                Invoke(nameof(DisableShield), shieldDuration);

                Debug.Log("Shield activated! Charges remaining: " + currentShieldCharges);
            }
        }
    }

    void DisableShield()
    {
        shieldActive = false;
        if (shieldVisual != null)
            shieldVisual.SetActive(false);
    }

    public void Unlock()
    {
        shieldUnlocked = true;
        maxShieldCharges = 1;
        currentShieldCharges = 1;
        Debug.Log("Shield ability unlocked! (1 charge)");
    }

    public void UpgradeCharges(int newMax)
    {
        maxShieldCharges = newMax;
        currentShieldCharges = newMax;
        Debug.Log("Shield upgraded! Max charges: " + newMax);
    }

    public void RefillCharges(int amount)
    {
        currentShieldCharges = Mathf.Min(currentShieldCharges + amount, maxShieldCharges);
        Debug.Log("Shield recharged! Charges: " + currentShieldCharges + "/" + maxShieldCharges);
    }
}