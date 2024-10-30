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
    [SerializeField] protected Transform shootingPosition; // Position from where to shoot
    [SerializeField] protected float shootingCooldown = 1f; // Cooldown between shots
    protected bool canShoot = true; // Flag to control shooting cooldown
    
    [Header("Property")]
    [SerializeField] protected int health = 100;
    [SerializeField] protected int maxHealth = 100;
    [SerializeField] protected EntityType entityType;

    protected virtual void Awake()
    {
        health = maxHealth;
    }

    protected virtual void Shoot(BulletType type)
    {
        if (!canShoot || bulletPrefab == null || shootingPosition == null) return;

        GameObject bullet = Instantiate(bulletPrefab, shootingPosition.position, shootingPosition.rotation);
        bullet.GetComponent<Bullet>().bulletType = type;
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.SetDirection(shootingPosition.up);
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

    public virtual void TakeDamage(int damage)
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
        Destroy(gameObject); // Remove this entity from the scene
    }
}
