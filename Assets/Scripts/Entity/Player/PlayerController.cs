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
    
    [Header("Reserve Shooting Position")]
    [SerializeField] public List<Transform> reservePositions; 
   
    [Header("Flying")]
    [SerializeField] private Sprite[] flyingSprites;
    [SerializeField] private float animationSpeed = 0.1f;
    
    [Header("Protected")]
    [SerializeField] private bool canProtect = true;
    [SerializeField] private Rigidbody2D playerRb;
    [SerializeField] private float protectedTime = 0.2f;
    [SerializeField] private float protectedCoolDown = 1f;
    [SerializeField] private bool isProtecting = false;
    private SpriteRenderer spriteRenderer;
    public bool canMove;
    private bool isAnimating; // Flag to check if animation is already running

    public float moveSpeed = 5f;

    [Header("Level Up")] 
    [SerializeField] private float maxLevelPoint;
    [SerializeField] private float currentLevelPoint;
    [SerializeField] public int currentLevel;
    [SerializeField] private UpgradeManager upgradeManager;

    
    private void Awake()
    {
        // Initialize the PlayerInput component
        playerInput = GetComponent<PlayerInput>();
        playerRb = GetComponent<Rigidbody2D>();
        entityType = EntityType.Player;
        currentLevel = 1;
        maxLevelPoint = 10f;
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
            Debug.Log("Can Dash: " + canProtect);
            if (canProtect) // Check if dashing is allowed before starting the dash
            {
                StartCoroutine(Protected());
            }
        };
        playerUI.SetHealth(MaxHealth);
        playerUI.SetMaxLevel(maxLevelPoint);
    }
   
    void Start()
    {
        
        playerUI.LevelUp(currentLevelPoint);
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
    
    
    private IEnumerator Protected()
    {
        Debug.Log("Attempting to Protect");
        if (canProtect) 
        {
            //Debug.Log("Protecting...");
            canProtect = false;
            isProtecting = true; // Set the protect flag
            
            yield return new WaitForSeconds(protectedTime);
            isProtecting = false;
            
            yield return new WaitForSeconds(protectedCoolDown);

            canProtect = true; 
            isProtecting = false; // Reset the dashing flag
            //Debug.Log("Protected complete, cooldown finished.");
        }
        else
        {
            Debug.Log("Cannot Protected, currently protecting or cooldown.");
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
        canProtect = true;
        // Inform the GameManager that movement is now allowed
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.EnableMovement(); // Allow movement from the GameManager side
        }
    }

    public override void TakeDamage(float damage)
    {
        if (isProtecting)
        {
            //Debug.Log("Player is invincible during Protected; no damage taken.");
            return; // Prevent damage if dashing
        }
        base.TakeDamage(damage);
        playerUI.Damage(Health);
    }

    public void Leveling(float points)
    {
        currentLevelPoint += points * (1+((Time.deltaTime*1)/2));
        playerUI.LevelUp(currentLevelPoint);
        if (currentLevelPoint >= maxLevelPoint)
        {
            currentLevelPoint -= maxLevelPoint;
            currentLevel += 1;
            maxLevelPoint += maxLevelPoint/2;
            playerUI.SetMaxLevel(maxLevelPoint);
            playerUI.LevelUp(currentLevelPoint);
            //upgradeManager.ChooseUpgrade();
        }
    }

    public void UnlockShootPoint()
    {
        if (reservePositions.Count > 0)
        {
            Transform reserve = reservePositions[0];
            shootingPosition.Add(reserve);
            reservePositions.Remove(reserve);
        }
    }

    public void Heal()
    {
        if (health < maxHealth - (maxHealth/4))
        {
            health += MaxHealth / 4;
            playerUI.Heal(health);
        }
        else
        {
            health = maxHealth;
            playerUI.Heal(health);
        }
    }

    public void UpgradeMaxHealth()
    {
        MaxHealth += 10 * (currentLevel * 1.5f);
        playerUI.SetHealth(MaxHealth);
        health += 10 * (currentLevel * 1.5f);
        playerUI.Heal(health);
    }
}