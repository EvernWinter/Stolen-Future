using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public PlayerController playerController;
    private bool isPaused = false;

    private bool movementAllowed = false; // Controlled by EnableMovementAfterDelay
    private bool wasMovementAllowedBeforePause; // Track movement state before pause
    [SerializeField] private GameObject pausePanel;

    void Update()
    {
        // If the game is paused, don't update movement
        if (isPaused) return;

        // Check if the pointer (touch or mouse) is over any UI element
        if (IsPointerOverUI() && movementAllowed)
        {
            playerController.canMove = false; // Disable movement if interacting with UI
        }
        else if (movementAllowed)
        {
            playerController.canMove = true; // Enable movement otherwise
        }
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return true;

        // For touch input, loop through all touches and check
        if (Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void EnableMovement()
    {
        movementAllowed = true;
        playerController.canMove = true; // Ensure movement is enabled
    }

    public void DisableMovement()
    {
        movementAllowed = false;
        playerController.canMove = false; // Ensure movement is fully disabled
    }

    public void Pause()
    {
        if (!isPaused)
        {
            PauseGame();
            pausePanel.SetActive(true);
        }
        else if (isPaused)
        {
            UnPauseGame();
            pausePanel.SetActive(false); 
        }
    }
    
    
    public void PauseGame()
    {
        if (!isPaused)
        {
            Time.timeScale = 0;   // Freeze the game time
            isPaused = true;      // Set paused state to true
            wasMovementAllowedBeforePause = movementAllowed; // Store whether movement was allowed
            DisableMovement();    // Disable movement when paused
            
        }
    }

    public void UnPauseGame()
    {
        if (isPaused)
        {
            Time.timeScale = 1;   // Resume the game time
            isPaused = false;     // Set paused state to false
            // Restore the previous movement state
            if (wasMovementAllowedBeforePause)
            {
                EnableMovement();  // Only enable movement if it was allowed before the pause
            }
        }
    }
}
