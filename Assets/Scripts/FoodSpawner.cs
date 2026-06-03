using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    [Header("Food")]
    public GameObject foodPrefab;

    [Header("Spawning")]
    public int maxFood = 44;
    public int spawnPerTick = 2;
    public float spawnInterval = 1.6f;  

    [Header("Spawn Area")]
    public float spawnRadius = 12f;

    private float timer;

    void Awake()
    {
        if (transform.parent != null)
        {
            transform.SetParent(null, true);
        }

        maxFood = Mathf.Clamp(maxFood, 24, 60);
        spawnPerTick = Mathf.Clamp(spawnPerTick, 1, 3);
        spawnInterval = Mathf.Clamp(spawnInterval, 1.2f, 3f);
        spawnRadius = Mathf.Clamp(spawnRadius, 10f, 18f);
    }

    void Start()
    {
        SpawnFoodUntilMax();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnFoodUntilMax();
        }
    }   

    void SpawnFoodUntilMax()
    {
        int currentFoodCount = GameObject.FindGameObjectsWithTag("Food").Length;
        int foodToSpawn = Mathf.Min(spawnPerTick, Mathf.Max(0, maxFood - currentFoodCount));

        for (int i = 0; i < foodToSpawn; i++)
        {
            SpawnFood();
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

        if (foodPrefab == null)
        {
            Debug.LogError("FoodSpawner has no foodPrefab assigned.");
            return;
        }

        GameObject spawnedFood = Instantiate(foodPrefab, randomPos, Quaternion.identity);
        spawnedFood.tag = "Food";

        if (spawnedFood.GetComponent<Food>() == null)
        {
            spawnedFood.AddComponent<Food>();
        }
    }
}