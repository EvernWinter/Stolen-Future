using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> enemyList = new List<GameObject>();
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnInterval; // Time interval for spawning enemies
    [SerializeField] private float spawnTimer = 0.0f; // Timer for spawning enemies
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform[] endPoint;
    [SerializeField] private GameObject player;

    private Dictionary<Transform, bool> endPointStatus;

    void Start()
    {
        endPointStatus = new Dictionary<Transform, bool>();
        foreach (var point in endPoint)
        {
            endPointStatus[point] = false; // Initially, all endpoints are unoccupied
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
        if (spawnTimer >= spawnInterval)
        {
            ReleaseEndpoints();
            Transform targetEndpoint = GetAvailableEndpoint();
            if (targetEndpoint != null) // Only spawn if there's an available endpoint
            {
                GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
                
                Enemy enemyMovement = enemy.GetComponent<Enemy>();
                if (enemyMovement != null)
                {
                    enemyMovement.SetTarget(targetEndpoint, this);
                }

                enemyList.Add(enemy);
                endPointStatus[targetEndpoint] = true; // Mark endpoint as occupied
                Debug.Log($"Enemy spawned at {targetEndpoint.name}"); // Debug message for spawned enemy

                spawnTimer = 0.0f; // Reset spawn timer after spawning an enemy
            }
            else
            {
                Debug.Log("No available endpoints to spawn an enemy."); // Debug message when no endpoints are available
                spawnTimer = 0.0f;
            }
        }
        else
        {
            // Debug the status of all endpoints
            /*foreach (var point in endPointStatus)
            {
                Debug.Log($"{point.Key.name} is {(point.Value ? "occupied" : "available")}");
            }*/
        }
    }

    Transform GetAvailableEndpoint()
    {
        foreach (var point in endPointStatus)
        {
            if (!point.Value) // If the endpoint is not occupied
            {
                return point.Key;
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
        foreach (var noEnemy in enemyList)
        {
            if (noEnemy == null)
            {
                enemyList.Remove(noEnemy);
                
            }
        }
    }
}
