using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject weakEnemyPrefab;
    public GameObject mediumEnemyPrefab;
    public GameObject maxEnemyPrefab;

    public float spawnInterval = 5f;
    public float spawnRadius = 10f;
    public int maxEnemies = 20;

    private PlayerLevel playerLevel;

    void Start()
    {
        // Remove any pre-placed Enemy instances in the scene so the spawner controls all spawns
        GameObject[] preplaced = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < preplaced.Length; i++)
        {
            Debug.Log($"[EnemySpawner] Destroying pre-placed enemy: {preplaced[i].name}");
            Destroy(preplaced[i]);
        }

        InvokeRepeating("SpawnEnemy", spawnInterval, spawnInterval);
        playerLevel = FindFirstObjectByType<PlayerLevel>();
        Debug.Log($"[EnemySpawner] Initialized. spawnerPos={transform.position}, spawnRadius={spawnRadius}, playerLevel={(playerLevel!=null?playerLevel.level:-1)}");
    }

    // Debug option to force medium enemies for testing
    public bool debugForceMedium = false;

    void SpawnEnemy()
    {
        if (GameObject.FindGameObjectsWithTag("Enemy").Length >= maxEnemies)
            return;

        int level = playerLevel != null ? playerLevel.level : 1;

        GameObject enemyToSpawn;
        float roll = Random.value;

        if (level <= 1)
        {
            // Level 1: only weak enemies
            enemyToSpawn = weakEnemyPrefab;
        }
        else if (level == 2)
        {
            // Level 2: mostly weak, some medium
            enemyToSpawn = roll < 0.7f ? weakEnemyPrefab : mediumEnemyPrefab;
        }
        else if (level == 3)
        {
            // Level 3: mix of weak and medium
            enemyToSpawn = roll < 0.4f ? weakEnemyPrefab : mediumEnemyPrefab;
        }
        else if (level == 4)
        {
            // Level 4: medium and max
            enemyToSpawn = roll < 0.5f ? mediumEnemyPrefab : maxEnemyPrefab;
        }
        else
        {
            // Level 5: mostly max enemies
            if (roll < 0.2f)
                enemyToSpawn = mediumEnemyPrefab;
            else
                enemyToSpawn = maxEnemyPrefab;
        }

        Vector3 spawnPosition = transform.position + Random.insideUnitSphere * spawnRadius;
        spawnPosition.y = 0.5f;

        if (debugForceMedium && mediumEnemyPrefab != null)
            enemyToSpawn = mediumEnemyPrefab;

        Debug.Log($"[EnemySpawner] Spawning '{(enemyToSpawn!=null?enemyToSpawn.name:"null")}' at {spawnPosition}");

        if (enemyToSpawn != null)
            Instantiate(enemyToSpawn, spawnPosition, Quaternion.identity);
    }
}
