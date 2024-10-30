using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Entity
{
    [SerializeField] public Transform target; // Endpoint to move towards
    [SerializeField] public Transform endTarget; // Not used in current code, consider removing if unnecessary
    private EnemyManager enemyManager;
    private bool stopMoving;
    private Transform player; // Reference to the player's transform
    

    protected override void Awake()
    {
        base.Awake();
        entityType = EntityType.Enemy;
        shootingCooldown = Random.Range(0.7f, 1.2f);
    }

    void Start()
    {
        StartCoroutine(AutoShoot(BulletType.Enemy));
        
    }

    public void SetTarget(Transform targetEndpoint, EnemyManager manager)
    {
        target = targetEndpoint;
        enemyManager = manager;
    }

    void Update()
    {
        RotateToPlayer();
        if (!stopMoving)
        {
            // Move towards the target endpoint
            transform.position = Vector3.MoveTowards(transform.position, target.position, 5f * Time.deltaTime);
            

            // If the enemy reaches the target, stop moving but do not release the endpoint yet
            if (Vector3.Distance(transform.position, target.position) < 0.1f)
            {
                stopMoving = true;
            }
        }
    }

    private void RotateToPlayer()
    {
        // Cache the player's transform if not already cached
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        // Calculate the direction to the player
        Vector3 direction = player.position - transform.position;

        // Calculate the angle in degrees, and adjust by -90 for the correct orientation
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

        // Create a rotation only around the Z-axis
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

        // Smoothly rotate towards the target rotation around the Z-axis
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 200f * Time.deltaTime); // Adjust speed as necessary
    }

    

    protected override void Die()
    {
        base.Die(); // Call base class to handle removal
        if (target != null)
        {
            enemyManager.ReleaseEndpoint(target, gameObject); // Release the endpoint when dying
        }
        Debug.Log("Enemy died and endpoint released"); // Debug message for enemy death
    }
    
}
