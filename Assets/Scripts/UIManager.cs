using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
public class UIManager : MonoBehaviour
{
    public Energy playerEnergy;
    public Slider energySlider;

    void Update()
    {
        if (playerEnergy != null && energySlider != null)
        {
            energySlider.maxValue = playerEnergy.maxEnergy;
            energySlider.value = playerEnergy.currentEnergy;
        }
    }
}
