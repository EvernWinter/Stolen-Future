using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{ 
   private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction touchMoveAction;
   
    private Vector2 moveInput;
    private Vector2 touchPosition;
    
    
   
    [Header("Flying")]
    [SerializeField] private Sprite[] flyingSprites;
    [SerializeField] private float animationSpeed = 0.1f;
    private SpriteRenderer spriteRenderer;
    public bool canMove;
    private bool isAnimating; // Flag to check if animation is already running

    public float moveSpeed = 5f;

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;   // Bullet prefab to instantiate
    [SerializeField] private Transform shootingPosition; // Position from where to shoot
    [SerializeField] private float shootingCooldown = 1f; // Cooldown between shots
    private bool canShoot = true; // Flag to control shooting cooldown

    private void Awake()
    {
        // Initialize the PlayerInput component
        playerInput = GetComponent<PlayerInput>();

        // Fetch the Move and TouchMove actions from the Input Actions
        moveAction = playerInput.actions["Move"];       // Ensure "Move" action is named correctly in the Input Actions asset
        touchMoveAction = playerInput.actions["TouchMove"]; // Same for "TouchMove"

        // Bind the Move action for WASD input
        moveAction.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        moveAction.canceled += ctx => moveInput = Vector2.zero;

        // Bind the TouchMove action for mobile touch input
        touchMoveAction.performed += ctx => touchPosition = ctx.ReadValue<Vector2>();
    }
   
    void Start()
    {
        canMove = false; // Disable movement initially
        spriteRenderer = GetComponent<SpriteRenderer>();
    
        // Start the flying animation once, if not already animating
        if (!isAnimating)
        {
            StartCoroutine(AnimateFlying());
        }

        // Start auto-shooting
        StartCoroutine(AutoShoot());
    
        // Delay movement enablement
        StartCoroutine(EnableMovementAfterDelay(3f));
    }

    private void OnEnable()
    {
        // Enable the input actions
        moveAction.Enable();
        touchMoveAction.Enable();
    }

    private void OnDisable()
    {
        // Disable the input actions
        moveAction.Disable();
        touchMoveAction.Disable();
    }

    private void Update()
    {
        // Handle movement for both PC and mobile.
        if (canMove)
        {
            MovePlayer();
        }
    }

    private void MovePlayer()
    {
        
        // If on mobile (touch input)
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            // Convert touch position to world position
            Vector3 touchWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(touchPosition.x, touchPosition.y, Camera.main.nearClipPlane));
            touchWorldPos.z = 0; // Ensure it's on the same plane as the player

            // Move player towards touch position smoothly
            transform.position = Vector3.Lerp(transform.position, touchWorldPos, Time.deltaTime * moveSpeed);
        }
        else
        {
            // If using WASD (PC input)
            Vector3 movement = new Vector3(moveInput.x, moveInput.y, 0);
            transform.position += movement * (moveSpeed * 5 )* Time.deltaTime;
        }
    }
   
    private IEnumerator AnimateFlying()
    {
        isAnimating = true; // Set animation flag
        int index = 0;

        while (true) // Infinite loop for continuous animation
        {
            spriteRenderer.sprite = flyingSprites[index]; // Set current sprite
            index = (index + 1) % flyingSprites.Length; // Loop through the array
            yield return new WaitForSeconds(animationSpeed); // Wait before changing to the next sprite
        }
    }

    private void Shoot()
    {
        if (!canShoot) return;

        // Instantiate the bullet at the shooting position
        GameObject bullet = Instantiate(bulletPrefab, shootingPosition.position, shootingPosition.rotation);

        // Get the Bullet script from the instantiated bullet and set its direction
        Bullet bulletScript = bullet.GetComponent<Bullet>();
    
        // Set the bullet direction to where the player is facing
        Vector2 shootDirection = shootingPosition.up; // Assuming the bullet shoots upwards relative to the player
        bulletScript.SetDirection(shootDirection);

        StartCoroutine(ShootCooldown()); // Start the cooldown after shooting
    }

    private IEnumerator ShootCooldown()
    {
        canShoot = false; // Prevent shooting during cooldown
        yield return new WaitForSeconds(shootingCooldown); // Wait for cooldown duration
        canShoot = true; // Allow shooting again
    }

    private IEnumerator AutoShoot()
    {
        while (true) // Loop to continuously auto-shoot
        {
            Shoot();
            //Debug.Log("Shot fired!"); // Debug statement to confirm shooting
            yield return new WaitForSeconds(shootingCooldown); // Wait for the cooldown duration between shots
        }
    }
    
    private IEnumerator EnableMovementAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // Wait for the specified delay
        canMove = true; // Allow player movement
    
        // Inform the GameManager that movement is now allowed
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.EnableMovement(); // Allow movement from the GameManager side
        }
    }
    
}