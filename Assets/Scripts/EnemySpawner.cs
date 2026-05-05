using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] public Transform[] spawnPoints;
    [SerializeField] private int maxEnemies = 5;
    [SerializeField] private float spawnInterval = 10f;
    [SerializeField] private bool autoSpawn = false;

    [Header("Enemy Settings")]
    [SerializeField] private string[] enemyNames = { "Enemy", "Bot", "Target" };

    // Tracking
    private List<EnemyAI> activeEnemies = new List<EnemyAI>();
    private float nextSpawnTime = 0f;
    private int enemyCounter = 0;

    // Events
    public System.Action<int> OnEnemyCountChanged;

    void Start()
    {
        // Create default enemy prefab if not assigned
        if (enemyPrefab == null)
        {
            CreateDefaultEnemyPrefab();
        }

        // Warn if no spawn points
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("EnemySpawner: No spawn points assigned! Drag spawn points from map to Inspector.");
        }
    }

    void Update()
    {
        // Clean up dead enemies
        activeEnemies.RemoveAll(e => e == null || e.IsDead());

        // Auto spawn
        if (autoSpawn && Time.time >= nextSpawnTime && activeEnemies.Count < maxEnemies)
        {
            SpawnEnemy();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    void CreateDefaultEnemyPrefab()
    {
        enemyPrefab = new GameObject("EnemyPrefab");
        enemyPrefab.SetActive(false);
        enemyPrefab.transform.SetParent(transform);

        // Add capsule for body
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(enemyPrefab.transform);
        body.transform.localPosition = Vector3.up;
        body.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
        body.GetComponent<Renderer>().material.color = Color.white;

        // Add sphere for head (with "Head" tag for headshots)
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.tag = "Head"; // Tag for headshot detection
        head.transform.SetParent(enemyPrefab.transform);
        head.transform.localPosition = new Vector3(0, 2.1f, 0);
        head.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        head.GetComponent<Renderer>().material.color = new Color(0.5f, 0.5f, 0.5f);

        // Add weapon visual
        GameObject weapon = GameObject.CreatePrimitive(PrimitiveType.Cube);
        weapon.name = "Weapon";
        weapon.transform.SetParent(enemyPrefab.transform);
        weapon.transform.localPosition = new Vector3(0.3f, 1.1f, 0.3f);
        weapon.transform.localScale = new Vector3(0.08f, 0.08f, 0.35f);
        weapon.GetComponent<Renderer>().material.color = new Color(0.2f, 0.2f, 0.2f);
        Collider weaponCol = weapon.GetComponent<Collider>();
        if (weaponCol != null) Destroy(weaponCol);

        // Use capsule collider for body
        CapsuleCollider bodyCollider = enemyPrefab.AddComponent<CapsuleCollider>();
        bodyCollider.center = new Vector3(0, 1f, 0);
        bodyCollider.height = 2f;
        bodyCollider.radius = 0.4f;

        // Add components
        enemyPrefab.AddComponent<EnemyAI>();
        enemyPrefab.AddComponent<EnemyHealthBar>();
    }

    void CreateDefaultSpawnPoints()
    {
        spawnPoints = new Transform[4];

        for (int i = 0; i < 4; i++)
        {
            GameObject spawnPoint = new GameObject($"SpawnPoint_{i}");
            spawnPoint.transform.SetParent(transform);

            // Position spawn points in a square around origin
            float x = (i % 2 == 0) ? -10f : 10f;
            float z = (i < 2) ? -10f : 10f;
            spawnPoint.transform.position = new Vector3(x, 0, z);

            spawnPoints[i] = spawnPoint.transform;
        }
    }

    public void SpawnEnemy()
    {
        if (activeEnemies.Count >= maxEnemies) return;
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        // Random spawn point
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // Spawn enemy
        GameObject enemyObj = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        enemyObj.SetActive(true);

        EnemyAI enemy = enemyObj.GetComponent<EnemyAI>();
        if (enemy != null)
        {
            // Set random name
            string randomName = enemyNames[Random.Range(0, enemyNames.Length)] + "_" + enemyCounter;
            enemy.enemyName = randomName;
            enemyObj.name = randomName;

            // Subscribe to death event
            enemy.OnEnemyDeath += OnEnemyDeath;

            activeEnemies.Add(enemy);
            enemyCounter++;
        }

        OnEnemyCountChanged?.Invoke(activeEnemies.Count);

        Debug.Log($"Spawned {enemyObj.name} at {spawnPoint.name}");
    }

    void OnEnemyDeath(EnemyAI enemy, string killer)
    {
        if (enemy != null)
        {
            enemy.OnEnemyDeath -= OnEnemyDeath;
        }

        activeEnemies.Remove(enemy);
        OnEnemyCountChanged?.Invoke(activeEnemies.Count);
    }

    public void SpawnWave(int count)
    {
        for (int i = 0; i < count && activeEnemies.Count < maxEnemies; i++)
        {
            SpawnEnemy();
        }
    }

    public void ClearAllEnemies()
    {
        foreach (EnemyAI enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
        }
        activeEnemies.Clear();
        OnEnemyCountChanged?.Invoke(0);
    }

    // Public getters
    public int GetActiveEnemyCount() => activeEnemies.Count;
    public List<EnemyAI> GetActiveEnemies() => new List<EnemyAI>(activeEnemies);

    // Gizmos
    void OnDrawGizmosSelected()
    {
        if (spawnPoints != null)
        {
            Gizmos.color = Color.cyan;
            foreach (Transform point in spawnPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, 0.5f);
                    Gizmos.DrawLine(point.position, point.position + Vector3.up * 2f);
                }
            }
        }
    }
}