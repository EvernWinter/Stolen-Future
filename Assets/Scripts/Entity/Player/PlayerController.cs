using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : Entity
{ 
   private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction touchMoveAction;
    private InputAction dashAction;
   
    private Vector2 moveInput;
    private Vector2 touchPosition;
    [SerializeField] private PlayerUI playerUI;
    
    
    
   
    [Header("Flying")]
    [SerializeField] private Sprite[] flyingSprites;
    [SerializeField] private float animationSpeed = 0.1f;
    
    [Header("Dashing")]
    [SerializeField] private bool canDash = true;
    [SerializeField] private Rigidbody2D playerRb;
    [SerializeField] private float dashingPower;
    [SerializeField] private float dashingTime = 0.2f;
    [SerializeField] private float dashingCoolDown = 1f;
    [SerializeField] private bool isDashing = false;
    private SpriteRenderer spriteRenderer;
    public bool canMove;
    private bool isAnimating; // Flag to check if animation is already running

    public float moveSpeed = 5f;

    

    private void Awake()
    {
        // Initialize the PlayerInput component
        playerInput = GetComponent<PlayerInput>();
        playerRb = GetComponent<Rigidbody2D>();
        entityType = EntityType.Player;

        // Fetch the Move and TouchMove actions from the Input Actions
        moveAction = playerInput.actions["Move"];       // Ensure "Move" action is named correctly in the Input Actions asset
        touchMoveAction = playerInput.actions["TouchMove"]; // Same for "TouchMove"
        dashAction = playerInput.actions["Dash"];

        // Bind the Move action for WASD input
        moveAction.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        moveAction.canceled += ctx => moveInput = Vector2.zero;

        // Bind the TouchMove action for mobile touch input
        touchMoveAction.performed += ctx => touchPosition = ctx.ReadValue<Vector2>();
        
        // Bind the Dash action
        dashAction.performed += ctx => {
            Debug.Log("Dash action performed");
            Debug.Log("Can Dash: " + canDash);
            if (canDash) // Check if dashing is allowed before starting the dash
            {
                StartCoroutine(Dash());
            }
        };
        
    }
   
    void Start()
    {
        playerUI.SetHealth(MaxHealth);
        canMove = false;
        spriteRenderer = GetComponent<SpriteRenderer>();
    
        if (!isAnimating)
        {
            StartCoroutine(AnimateFlying());
        }

        StartCoroutine(AutoShoot(BulletType.Player));
        StartCoroutine(EnableMovementAfterDelay(3f));
    }

    private void OnEnable()
    {
        // Enable the input actions
        moveAction.Enable();
        touchMoveAction.Enable();
        dashAction.Enable();
    }

    private void OnDisable()
    {
        // Disable the input actions
        moveAction.Disable();
        touchMoveAction.Disable();
        dashAction.Disable();
    }

    private void Update()
    {
        StartCoroutine(AutoShoot(BulletType.Player));
        
        
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
    
    
    private IEnumerator Dash()
    {
        Debug.Log("Attempting to Dash...");
        if (canDash) 
        {
            Debug.Log("Dashing...");
            canDash = false;
            isDashing = true; // Set the dashing flag

            float originalGravity = playerRb.gravityScale;
            playerRb.gravityScale = 0;

            // Use the move input to determine the dash direction
            Vector2 dashDirection = new Vector2(moveInput.x, moveInput.y).normalized * dashingPower;

            // Set the dash velocity in the direction of input
            playerRb.velocity = dashDirection;

            yield return new WaitForSeconds(dashingTime);

            // Stop the player after dashing
            playerRb.velocity = Vector2.zero; // Stop the dash

            playerRb.gravityScale = originalGravity;
            yield return new WaitForSeconds(dashingCoolDown);

            canDash = true; 
            isDashing = false; // Reset the dashing flag
            Debug.Log("Dash complete, cooldown finished.");
        }
        else
        {
            Debug.Log("Cannot Dash, currently dashing or cooldown.");
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
    
    private IEnumerator EnableMovementAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // Wait for the specified delay
        canMove = true; // Allow player movement
        canDash = true;
        // Inform the GameManager that movement is now allowed
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.EnableMovement(); // Allow movement from the GameManager side
        }
    }

    public override void TakeDamage(int damage)
    {
        if (isDashing)
        {
            Debug.Log("Player is invincible during dash; no damage taken.");
            return; // Prevent damage if dashing
        }
        base.TakeDamage(damage);
        playerUI.Damage(Health);
    }
}