using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float bulletSpeed = 10f; // Speed of the bullet
    public int damage = 1; // Damage the bullet deals

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Method to set bullet direction and velocity
    public void SetDirection(Vector2 direction)
    {
        rb.velocity = direction.normalized * bulletSpeed; // Ensure the direction is normalized
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Handle collision with other objects (e.g., enemies, environment)
        // Uncomment and add your collision handling logic
    }

    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}
