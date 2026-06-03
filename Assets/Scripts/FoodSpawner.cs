using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    [Header("Food")]
    public GameObject foodPrefab;

    [Header("Spawning")]
    public int maxFood = 100;
    public int spawnPerTick = 3;
    public float spawnInterval = 0.75f;  

    [Header("Spawn Area")]
    public float spawnRadius = 14f;

    private float timer;

    void Awake()
    {
        if (transform.parent != null)
        {
            transform.SetParent(null, true);
        }

        maxFood = Mathf.Max(maxFood, 100);
        spawnPerTick = Mathf.Max(spawnPerTick, 3);
        spawnInterval = Mathf.Min(spawnInterval, 0.75f);
        spawnRadius = Mathf.Max(spawnRadius, 14f);
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