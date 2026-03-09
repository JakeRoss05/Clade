using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShield : MonoBehaviour
{
    public GameObject shieldVisual;
    public bool shieldUnlocked = false;
    public bool shieldActive = false;
    public float shieldEnergyCost = 15f;
    public float shieldDuration = 3f;

    private Energy energy;

    void Start()
    {
        energy = GetComponent<Energy>();
    }

    void Update()
    {
        if (!shieldUnlocked)
            return;

        if (Keyboard.current.qKey.wasPressedThisFrame && !shieldActive)
        {
            if (energy != null && energy.currentEnergy >= shieldEnergyCost)
            {
                energy.currentEnergy -= shieldEnergyCost;

                shieldActive = true;

                CancelInvoke(nameof(DisableShield));

                if (shieldVisual != null)
                    shieldVisual.SetActive(true);

                Invoke(nameof(DisableShield), shieldDuration);
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
        Debug.Log("Shield ability unlocked!");
    }
}