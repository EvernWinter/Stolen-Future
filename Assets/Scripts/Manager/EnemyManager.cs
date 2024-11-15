using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> enemyList = new List<GameObject>();
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnInterval;
    [SerializeField] private float spawnIntervalMax; // Time interval for spawning enemies
    [SerializeField] private float spawnIntervalMin;
    [SerializeField] private float spawnTimer = 0.0f; // Timer for spawning enemies
    [SerializeField] private Transform[] spawnPoint;
    [SerializeField] private Transform[] endPoint;
    [SerializeField] private GameObject player;
    [SerializeField] private Transform[] turretStationPoints; // Array for multiple turret station points
    private Dictionary<Transform, bool> turretStationStatus;
    private Dictionary<Transform, bool> endPointStatus;

    void Start()
    {
        endPointStatus = new Dictionary<Transform, bool>();
        turretStationStatus = new Dictionary<Transform, bool>();
        foreach (var point in endPoint)
        {
            endPointStatus[point] = false; // Initially, all endpoints are unoccupied
        }
        foreach (var turretPoint in turretStationPoints)
        {
            turretStationStatus[turretPoint] = false;
        }
    }

    void Update()
    {
        SpawnEnemies();
    }

       
    private void SpawnEnemies()
    {
        // Increment the spawn timer
        spawnTimer += Time.deltaTime;

        // Check if we can spawn a new enemy
        if (spawnTimer >= Random.Range(spawnIntervalMin, spawnIntervalMax))
        {
            bool hasSpawned = false;

            // Loop through all spawn points
            foreach (var point in spawnPoint)
            {
                // Release any endpoints based on your game logic (e.g., enemies reaching their destination)
                ReleaseEndpoints();

                // Get an available endpoint for the enemy to target (for non-turret enemies)
                Transform targetEndpoint = GetAvailableEndpoint();
                Transform turretStation = GetAvailableTurretStation();
                // Check if we can spawn either a turret or a non-turret enemy
                if (targetEndpoint != null || turretStation != null)
                {
                    GameObject enemy = Instantiate(enemyPrefab, point.position, Quaternion.identity);
                    enemy.GetComponent<Enemy>().MaxHealth += 50 + (player.GetComponent<PlayerController>().currentLevel * 15f);
                    Enemy enemyComponent = enemy.GetComponent<Enemy>();
                    if (targetEndpoint != null)
                    {
                        enemyComponent.enemyType = (EnemyType)Random.Range(0, 2);  // Randomly assign enemy type
                        
                        if (enemyComponent != null && targetEndpoint != null)
                        {
                            enemyComponent.SetTarget(targetEndpoint, this);
                        }
                    }
                    else if (turretStation != null)
                    {
                        enemyComponent.enemyType = EnemyType.Turret;  // Fixed assign enemy type
                        if (turretStation != null)
                        {
                            enemy.transform.position = turretStation.position; // Place turret at the special station
                            turretStationStatus[turretStation] = true; // Mark turret station as occupied
                            enemyComponent.SetTarget(turretStation, this); // Turrets do not need to move towards an endpoint
                            Debug.Log($"Turret spawned at {turretStation.name}");
                        }
                        else
                        {
                            Debug.Log("No available turret station. Skipping turret spawn.");
                            continue; // Skip spawning turret if no station is available
                        }
                    }
                    // Add enemy to the list and mark the endpoint as occupied (if it's not a turret)
                    enemyList.Add(enemy);
                    if (enemyComponent.enemyType != EnemyType.Turret && targetEndpoint != null)
                    {
                        endPointStatus[targetEndpoint] = true;
                    }

                    Debug.Log($"Enemy spawned at {point.name} (Target: {targetEndpoint?.name ?? "None"})");

                    hasSpawned = true;
                }
                else
                {
                    Debug.Log("No available endpoints or turret stations to spawn an enemy.");
                }
            }

            // Reset spawn timer only if at least one enemy was spawned
            if (hasSpawned)
            {
                spawnTimer = 0.0f;
            }
        }
    }

    Transform GetAvailableEndpoint()
    {
        // Create a temporary list of endpoints
        List<Transform> shuffledEndpoints = new List<Transform>(endPoint);

        // Shuffle the list of endpoints
        for (int i = 0; i < shuffledEndpoints.Count; i++)
        {
            Transform temp = shuffledEndpoints[i];
            int randomIndex = Random.Range(i, shuffledEndpoints.Count);
            shuffledEndpoints[i] = shuffledEndpoints[randomIndex];
            shuffledEndpoints[randomIndex] = temp;
        }

        // Find the first available endpoint in the shuffled list
        foreach (var point in shuffledEndpoints)
        {
            if (!endPointStatus[point]) // If the endpoint is not occupied
            {
                return point;
            }
        }
        return null; // No available endpoint
    }

    public void ReleaseEndpoint(Transform endpoint, GameObject enemy)
    {
        endPointStatus[endpoint] = false; // Mark the endpoint as available
        enemyList.Remove(enemy); // Remove from tracking list
    }

    private void ReleaseEndpoints()
    {
        // Loop through the enemy list and clean up any enemies that are destroyed or no longer exist
        for (int i = enemyList.Count - 1; i >= 0; i--)
        {
            if (enemyList[i] == null)  // Check for any null references in the list
            {
                enemyList.RemoveAt(i);  // Remove the null reference
            }
        }
    }
    
    Transform GetAvailableTurretStation()
    {
        // Find the first available turret station in the dictionary
        foreach (var station in turretStationStatus)
        {
            if (!station.Value) // If the turret station is not occupied
            {
                return station.Key;
            }
        }
        return null; // No available turret station
    }

    public void ReleaseTurretStation(Transform turretStation, GameObject enemy)
    {
        if (turretStation != null)
        {
            turretStationStatus[turretStation] = false; // Mark the turret station as available
        }
        else
        {
            Debug.LogWarning("Attempted to release a null turret station.");
        }
        enemyList.Remove(enemy); // Remove from tracking list
    }
}
