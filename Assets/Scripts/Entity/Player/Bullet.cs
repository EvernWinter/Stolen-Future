using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BulletType
{
    Player,
    Enemy
}
public class Bullet : MonoBehaviour
{
    public float bulletSpeed = 10f; // Speed of the bullet
    public float damage = 15f; // Damage the bullet deals
    public BulletType bulletType;
    public bool isHoming = false;
    private Transform target;
    [SerializeField] private Sprite[] sprite;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        switch (bulletType)
        {
            case BulletType.Player:
                GetComponent<SpriteRenderer>().sprite = sprite[0];
                break;
            case BulletType.Enemy:
                GetComponent<SpriteRenderer>().sprite = sprite[1];
                break;
        }
        if (isHoming)
        {
            target = FindNearestEnemy();
        }
    }

    private void Update()
    {
        if (isHoming)
        {
            if (target != null)
            {
                // Move towards the target (homing logic)
                Vector3 direction = (target.position - transform.position).normalized;
                transform.position += direction * bulletSpeed * Time.deltaTime;

                // Rotate the missile to face the target
                transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
            }
        }
    }

    // Method to set bullet direction and velocity
    public void SetDirection(Vector2 direction)
    {
        rb.velocity = direction.normalized * bulletSpeed;
        //Debug.Log("Bullet direction set to: " + rb.velocity); // Debug to confirm velocity
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && bulletType == BulletType.Enemy)
        {
            other.GetComponent<Entity>().TakeDamage(damage);
            OnDestroy();
        }
        else if (other.CompareTag("Enemy")  && bulletType == BulletType.Player)
        {
            other.GetComponent<Entity>().TakeDamage(damage);
            OnDestroy();
        }
        
    }
    
    private Transform FindNearestEnemy()
    {
        // Find the closest enemy
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform nearestEnemy = null;
        float minDistance = float.MaxValue;

        foreach (var enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestEnemy = enemy.transform;
            }
        }

        return nearestEnemy;
    }

    private void OnBecameInvisible()
    {
        OnDestroy();
    }

    private void OnDestroy()
    {
        Destroy(gameObject);
    }
}
