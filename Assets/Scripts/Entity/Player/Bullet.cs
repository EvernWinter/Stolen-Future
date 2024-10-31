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
    public int damage = 15; // Damage the bullet deals
    public BulletType bulletType;
    

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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

    private void OnBecameInvisible()
    {
        OnDestroy();
    }

    private void OnDestroy()
    {
        Destroy(gameObject);
    }
}
