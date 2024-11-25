using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum ShootType
{
    None,
    Shotgun,
    HomingMissile
}
public class PlayerController : Entity
{ 
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction touchMoveAction;
    private InputAction dashAction;
   
    private Vector2 moveInput;
    private Vector2 touchPosition;
    [SerializeField] private PlayerUI playerUI;
    [SerializeField] private Camera cam;
    
    [Header("Reserve Shooting Position")]
    [SerializeField] public List<Transform> reservePositions; 
   
    [Header("Flying")]
    [SerializeField] private Sprite[] flyingSprites;
    [SerializeField] private float animationSpeed = 0.1f;
    
    [Header("Protected")]
    [SerializeField] private GameObject protectedObject;
    [SerializeField] private bool canProtect;
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
    [SerializeField] public ShootType shootType;
    
    [Header("Shotgun Settings")]
    [SerializeField] private int shotgunBulletCount = 5;  // Number of bullets per shot
    [SerializeField] private float shotgunSpreadAngle = 30f; 
    
    [Header("Ads")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] public bool revive = false;
    private int dead;
    [Header("Temp Upgrade")]
    [SerializeField] private float upgradeDuration = 10f; 
    [SerializeField] private float upgradeCooldown = 5f;  
    [SerializeField] private bool canUpgrade; 
    private InputAction upgradeAction;
    
    [Header("Button")]
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button protectButton;
    
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
        upgradeAction = playerInput.actions["Upgrade"];
    
        // Bind the Move action for WASD input
        moveAction.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        moveAction.canceled += ctx => moveInput = Vector2.zero;

        // Bind the TouchMove action for mobile touch input
        touchMoveAction.performed += ctx => touchPosition = ctx.ReadValue<Vector2>();
        upgradeAction.performed += ctx => TryUpgrade();
    
        // Bind the Dash action
        dashAction.performed += ctx =>
        {
            TryProtected();
        };
    
        playerUI.SetHealth(MaxHealth);
        playerUI.SetMaxLevel(maxLevelPoint);
        

        upgradeButton.interactable = false;
        protectButton.interactable = false;
        if (upgradeButton != null)
        {
            // Add listener to the button to trigger upgrade when clicked
            upgradeButton.onClick.AddListener(() => StartCoroutine(TemporaryUpgrade()));
        }
        else
        {
            Debug.LogWarning("Upgrade button is not assigned in the inspector!");
        }

        if (protectButton != null)
        {
            // Add listener to the button to trigger protection when clicked
            protectButton.onClick.AddListener(() => StartCoroutine(Protected()));
        }
        else
        {
            Debug.LogWarning("Protect button is not assigned in the inspector!");
        }
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
        // Enable the input actions only if movement is allowed
        moveAction.Enable();
        touchMoveAction.Enable();
        dashAction.Enable();
        upgradeAction.Enable();

        // Only enable buttons and protection if movement is allowed
        upgradeButton.interactable = canMove;
        protectButton.interactable = canMove;
        canProtect = canMove;
        canUpgrade = canMove;
    }

    private void OnDisable()
    {
        // Disable the input actions
        moveAction.Disable();
        touchMoveAction.Disable();
        dashAction.Disable();
        upgradeAction.Disable();

        // Disable button interactions
        upgradeButton.interactable = false;
        protectButton.interactable = false;
        canProtect = false;
        canUpgrade = false;
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
    // Get the screen's bounds in world space
    float cameraHalfWidth = cam.orthographicSize * cam.aspect;
    float cameraHalfHeight = cam.orthographicSize;

    // Calculate the camera bounds
    float minX = cam.transform.position.x - cameraHalfWidth;
    float maxX = cam.transform.position.x + cameraHalfWidth;
    float minY = cam.transform.position.y - cameraHalfHeight;
    float maxY = cam.transform.position.y + cameraHalfHeight;

    // Handle movement for both PC and mobile.
    if (canMove)
    {
        // If touch input is detected, but only if not interacting with UI
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            // Check if touch is not on UI
            if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                // Convert touch position to world position
                Vector3 touchWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(touchPosition.x, touchPosition.y, Camera.main.nearClipPlane));
                touchWorldPos.z = 0; // Ensure it's on the same plane as the player

                // Clamp the touch position to the camera bounds
                touchWorldPos.x = Mathf.Clamp(touchWorldPos.x, minX, maxX);
                touchWorldPos.y = Mathf.Clamp(touchWorldPos.y, minY, maxY);

                // Move player towards touch position smoothly
                transform.position = Vector3.MoveTowards(transform.position, touchWorldPos, moveSpeed * Time.deltaTime);
            }
        }
        else
        {
            // If using WASD (PC input) or touch move is disabled due to UI interaction
            Vector3 movement = new Vector3(moveInput.x, moveInput.y, 0);
            Vector3 newPosition = transform.position + movement * (moveSpeed * 5) * Time.deltaTime;

            // Clamp the new position to the camera bounds
            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

            transform.position = newPosition;
        }
    }
}
    
    protected override void Shoot(BulletType type)
    {
        if (!canShoot || bulletPrefab == null || shootingPosition == null) return;

        switch (shootType)
        {
            case ShootType.None:
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
                break;
            case ShootType.Shotgun:
                
                foreach (var position in shootingPosition)
                {
                    // Calculate starting angle to center the bullets
                    float startAngle = -shotgunSpreadAngle / 2;

                    for (int i = 0; i < shotgunBulletCount; i++)
                    {
                        // Calculate the angle for each bullet
                        float angle = startAngle + (i * (shotgunSpreadAngle / (shotgunBulletCount - 1)));
                        Quaternion rotation = Quaternion.Euler(0, 0, position.eulerAngles.z + angle);

                        // Instantiate bullet with calculated spread angle
                        GameObject bullet = Instantiate(bulletPrefab, position.position, rotation);
                        bullet.GetComponent<Bullet>().bulletType = type;
                        bullet.GetComponent<Bullet>().damage = 2 * (damage / shotgunBulletCount);
                        Bullet bulletScript = bullet.GetComponent<Bullet>();
                        if (bulletScript != null)
                        {
                            bulletScript.SetDirection(rotation * Vector3.up);
                        }
                    }
                }
                break;
            case ShootType.HomingMissile:
                foreach (var postion in shootingPosition)
                {
                    GameObject bullet = Instantiate(bulletPrefab, postion.position, postion.rotation);
                    bullet.GetComponent<Bullet>().bulletType = type;
                    bullet.GetComponent<Bullet>().isHoming = true;
                    bullet.GetComponent<Bullet>().damage = damage/1.5f;
                    Bullet bulletScript = bullet.GetComponent<Bullet>();
                    if (bulletScript != null)
                    {
                        bulletScript.SetDirection(postion.up);
                    }
                }
                break;
                
        }
        
        
        StartCoroutine(ShootCooldown());
    }
    
    private IEnumerator Protected()
    {
        Debug.Log("Attempting to Protect");
        if (canProtect) 
        {
            //Debug.Log("Protecting...");
            canProtect = false;
            isProtecting = true; // Set the protect flag
            protectedObject.SetActive(true);
            yield return new WaitForSeconds(protectedTime);
            isProtecting = false;
            protectedObject.SetActive(false);
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
        canUpgrade = true;
    
        // Update UI buttons now that movement is allowed
        upgradeButton.interactable = true;
        protectButton.interactable = true;

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
            upgradeManager.ChooseUpgrade();
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
    
    public void HealMax()
    {
        health = maxHealth;
        playerUI.Heal(health);
    }

    public void UpgradeMaxHealth()
    {
        MaxHealth += 10 * (currentLevel * 1.5f);
        playerUI.SetHealth(MaxHealth);
        health += 10 * (currentLevel * 1.5f);
        playerUI.Heal(health);
    }

    protected override void Die()
    {
        dead++;
        AnalyticManager.instance.Dead(dead);
        if (!revive)
        {
            gameManager.ShowLosePanel();
        }
        else
        {
            Destroy(gameObject);
            gameManager.ShowLosePanel();
        }
    }
    
    private void TryUpgrade()
    {
        if (canUpgrade && canMove && upgradeButton.interactable) // Add check for canMove
        {
            StartCoroutine(TemporaryUpgrade());
        }
    }

    private void TryProtected()
    {
        if (canProtect && canMove && protectButton.interactable) // Add check for canMove
        {
            StartCoroutine(Protected());
        }
    }

    private IEnumerator TemporaryUpgrade()
    {
        if (canUpgrade)
        {
            canUpgrade = false; // Disable further upgrades during this period

            // Randomly choose between Shotgun or Homing Missile
            shootType = (Random.Range(0, 2) == 0) ? ShootType.Shotgun : ShootType.HomingMissile;

            // Log the upgrade type for debugging
            Debug.Log("Temporary upgrade to: " + shootType);

            // Wait for the upgrade duration to end
            yield return new WaitForSeconds(upgradeDuration);

            // Revert back to the default shoot type (None)
            shootType = ShootType.None;
            Debug.Log("Upgrade ended, reverting to None");

            // Wait for cooldown before allowing another upgrade
            yield return new WaitForSeconds(upgradeCooldown);

            canUpgrade = true; // Allow the upgrade again after cooldown
        }
    }
}