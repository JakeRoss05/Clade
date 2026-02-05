using UnityEngine;

public class Food : MonoBehaviour
{
    public float sizeIncrease = 0.1f;

    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement player = other.GetComponent<PlayerMovement>();

        if (player != null)
        {
            player.sizeMultiplier += sizeIncrease;
            Destroy(gameObject);
        }
    }
}

