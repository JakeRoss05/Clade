using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    [Header("Food")]
    public GameObject foodPrefab;

    [Header("Spawning")]
    public int maxFood = 50;
    public float spawnInterval = 1.5f;  

    [Header("Spawn Area")]
    public float spawnRadius = 10f;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            timer = 0f;

            if (GameObject.FindGameObjectsWithTag("Food").Length < maxFood)
            {
                SpawnFood();
            }
        }
    }   

    void SpawnFood()
    {
        Vector3 randomPos = transform.position +
            new Vector3(
                Random.Range(-spawnRadius, spawnRadius),
                0f,
                Random.Range(-spawnRadius, spawnRadius)
            );

        Instantiate(foodPrefab, randomPos, Quaternion.identity);
    }
}