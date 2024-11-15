using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EntityType
{
    Player,
    Enemy
}

public abstract class Entity : MonoBehaviour
{
    [Header("Shooting")]
    [SerializeField] protected GameObject bulletPrefab;   // Bullet prefab to instantiate
    [SerializeField] protected List<Transform> shootingPosition; // Position from where to shoot
    [SerializeField] protected float shootingCooldown = 1f; // Cooldown between shots
    public float ShootingCooldown { get { return shootingCooldown; } set { shootingCooldown = value; } }
    protected bool canShoot = true; // Flag to control shooting cooldown
    
    [Header("Property")]
    [SerializeField] protected float health = 100f;
    public float Health { get { return health; } }
    
    [SerializeField] protected float maxHealth = 100f;
    public float MaxHealth { get { return maxHealth; } set { maxHealth = value; } }
    
    [SerializeField] protected float damage = 10f;
    public float Damage { get { return damage; } set { damage = value; } }
    
    [SerializeField] protected EntityType entityType;

    protected virtual void Awake()
    {
        health = maxHealth;
        
    }

    protected virtual void Shoot(BulletType type)
    {
        if (!canShoot || bulletPrefab == null || shootingPosition == null) return;

        foreach (var postion in shootingPosition)
        {
            GameObject bullet = Instantiate(bulletPrefab, postion.position, postion.rotation);
            bullet.GetComponent<Bullet>().bulletType = type;
            bullet.GetComponent<Bullet>().damage = damage;
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.SetDirection(postion.up);
            }
        }
        
        StartCoroutine(ShootCooldown());
    }
    
    protected virtual IEnumerator ShootCooldown()
    {
        canShoot = false; // Prevent shooting during cooldown
        yield return new WaitForSeconds(shootingCooldown); // Wait for cooldown duration
        canShoot = true; // Allow shooting again
    }

    protected virtual IEnumerator AutoShoot(BulletType type)
    {
        while (true) // Loop to continuously auto-shoot
        {
            Shoot(type);
            yield return new WaitForSeconds(shootingCooldown); // Wait for the cooldown duration between shots
        }
    }

    public virtual void TakeDamage(float damage)
    {
        if (health > 0)
        {
            health -= damage;
        }
        else
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        if (this is Enemy)
        {
            FindObjectOfType<GameManager>().score += (int)GetComponent<Enemy>().point;
        }
        Destroy(gameObject); // Remove this entity from the scene
    }
    
    
}
