using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum EnemyType
{
    Shoot,
    Tank,
    Turret
}
public class Enemy : Entity
{
    [SerializeField] public Transform target; // Endpoint to move towards
    [SerializeField] public EnemyType enemyType;
    [SerializeField] private EnemyManager enemyManager;
    [SerializeField] private bool stopMoving;
    [SerializeField] private GameObject playerObj;
    [SerializeField] private Transform player; // Reference to the player's transform
    [SerializeField] private bool foundPlayer;
    [SerializeField] public float point;
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private SpriteRenderer spriteRenderer; // Reference to SpriteRenderer component
    
    [Header("Tank")]
    [SerializeField] private bool isCharging;
    [SerializeField] private float chargeSpeed;
    
    [Header("Turret")]
    [SerializeField] private float turretRotationSpeed = 90f; // degrees per second
    [SerializeField] private float turretFireInterval = 2f;   // Time between each firing cycle
    [SerializeField] private int turretBurstCount = 3;        // Number of shots in each burst
    [SerializeField] private float burstCooldown = 0.3f;      // Time between each shot in a burst
    [SerializeField] private float turretMoveSpeed = 1f;
    private Coroutine firingRoutine;

    protected override void Awake()
    {
        base.Awake();
        entityType = EntityType.Enemy;
        shootingCooldown = Random.Range(0.7f, 1.2f);
       
        spriteRenderer = GetComponent<SpriteRenderer>();
        switch (enemyType)
        {
            case EnemyType.Shoot:
                spriteRenderer.sprite = sprites[0];
                break;
            case EnemyType.Tank:
                spriteRenderer.sprite = sprites[1];
                break;
            case EnemyType.Turret:
                spriteRenderer.sprite = sprites[2];
                break;
        }
    }

    void Start()
    {
        playerObj = GameObject.FindGameObjectWithTag("Player");
        health += 100f + playerObj.GetComponent<PlayerController>().currentLevel * 10f;
        AssignSprite();
    }

    public void SetTarget(Transform targetEndpoint, EnemyManager manager)
    {
        target = targetEndpoint;
        enemyManager = manager;
    }

    void Update()
    {
        if (enemyType == EnemyType.Turret)
        {
            MoveDownward();
        }
        
        if (foundPlayer && enemyType == EnemyType.Shoot)
        {
            StartCoroutine(AutoShoot(BulletType.Enemy));
        }
        else if (foundPlayer && enemyType == EnemyType.Turret && firingRoutine == null)
        {
            firingRoutine = StartCoroutine(BurstFireRoutine());
        }
        RotateToPlayer();
        if (!stopMoving && enemyType != EnemyType.Turret)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.position, 5f * Time.deltaTime);

            if (Vector3.Distance(transform.position, target.position) < 0.1f)
            {
                stopMoving = true;

                // Start Tank behavior if the enemy is of type Tank
                if (enemyType == EnemyType.Tank && !isCharging)
                {
                    StartCoroutine(TankChargeCoroutine());
                }
            }
        }
    }

    private void RotateToPlayer()
    {
        if (playerObj == null || isCharging)
        {
            //Debug.LogWarning("Target is null. Cannot rotate enemy towards player.");
            //Stop autoshoot here
            return;
        }
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
        
        if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f) // Adjust threshold if needed
        {
            foundPlayer = true;
            //Debug.Log("Rotation complete. Player found.");
        }
    }

    private IEnumerator TankChargeCoroutine()
    {
        isCharging = true; // Ensure only one charge cycle occurs at a time

        // Wait for a random duration between 2.5 and 3 seconds before starting charge
        yield return new WaitForSeconds(Random.Range(2.5f, 3f));

        // Start charging towards the player
        if (playerObj != null)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;

            float chargeStartTime = Time.time; // Record the time when charging starts

            // Charge towards the player
            while (Vector3.Distance(transform.position, player.position) > 0.1f)
            {
                transform.position += directionToPlayer * chargeSpeed * Time.deltaTime;

                // Check if charging has been going on for more than 4 seconds
                if (Time.time - chargeStartTime > 3f)
                {
                    point = 0; // Set the point to 0 if charging exceeds 4 seconds
                    Die(); // Call Die() to handle enemy death
                    yield break; // Exit the coroutine as the enemy is dying
                }

                yield return null;
            }
        }

        isCharging = false; // Reset charging state
    }
    
    private void AssignSprite()
    {
        switch (enemyType)
        {
            case EnemyType.Shoot:
                spriteRenderer.sprite = sprites[0];
                break;
            case EnemyType.Tank:
                spriteRenderer.sprite = sprites[1];
                break;
            case EnemyType.Turret:
                spriteRenderer.sprite = sprites[2];
                break;
        }
    }

    
    
    private IEnumerator BurstFireRoutine()
    {
        Debug.Log("Starting burst fire routine"); // Debug to check if routine starts

        while (enemyType == EnemyType.Turret)
        {
            for (int i = 0; i < turretBurstCount; i++)
            {
                foreach (var position in shootingPosition)
                {
                    GameObject bullet = Instantiate(bulletPrefab, position.position, position.rotation);
                    Bullet bulletScript = bullet.GetComponent<Bullet>();
                
                    bulletScript.bulletType = BulletType.Enemy;
                    bulletScript.damage = damage;
                
                    if (bulletScript != null)
                    {
                        bulletScript.SetDirection(position.up);
                    }
                }
            
                // Wait briefly between shots within the burst
                yield return new WaitForSeconds(burstCooldown);
            }
        
            // Wait for the interval between each burst
            yield return new WaitForSeconds(turretFireInterval);
        }

        firingRoutine = null; // Reset after firing stops
    }

    private void OnDisable() {
        if (firingRoutine != null && enemyType == EnemyType.Turret) 
        {
            StopCoroutine(firingRoutine);
            firingRoutine = null;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            enemyManager.ReleaseEndpoint(target, gameObject);
            other.GetComponent<PlayerController>().TakeDamage(health/2);
            health -= health;
            point = 0;
        }
    }
    
    protected override void Die()
    {
        if (target != null && enemyType != EnemyType.Turret)
        {
            enemyManager.ReleaseEndpoint(target, gameObject); // Release the endpoint when dying
        }
        else if (enemyType == EnemyType.Turret)
        {
            enemyManager.ReleaseTurretStation(target, gameObject);
        }
        Debug.Log("Enemy died and endpoint released"); // Debug message for enemy death
        if (!isCharging)
        {
            if (point > 0)
            {
                playerObj.GetComponent<PlayerController>().Leveling(point/30);
            }
            
        }
        base.Die();
    }
    
    private void OnBecameInvisible()
    {
        if (enemyType == EnemyType.Turret && transform.position.y > playerObj.transform.position.y )
        { 
           point = 0;
           Die();
        }
    }
    
    private void MoveDownward()
    {
        transform.position += Vector3.down * turretMoveSpeed * Time.deltaTime;
    }
    
}
