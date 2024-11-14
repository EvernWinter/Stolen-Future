using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public PlayerController playerController;
    private bool isPaused = false;

    private bool movementAllowed = false; // Controlled by EnableMovementAfterDelay
    private bool wasMovementAllowedBeforePause; // Track movement state before pause
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private Button ReviveButton;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] public int score;
    [SerializeField] private TMP_Text[] scoreText;
    [SerializeField] private float countdownTime = 300f;

    
    void Start()
    {
        StartCoroutine(StartCountdown()); // Start the countdown coroutine
    }
    
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

        foreach (var text in scoreText)
        {
            text.text = "Score: " + score;
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
    
    public void MainMenuScene()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void ShowLosePanel()
    {
        PauseGame();
        losePanel.SetActive(true);   
        ReviveButton.interactable = true;
    }
    
    public void RewardedRecived()
    {
        UnPauseGame();
        losePanel.SetActive(false);   
        ReviveButton.interactable = false;
    }
    
    private IEnumerator StartCountdown()
    {
        float remainingTime = countdownTime;

        while (remainingTime > 0)
        {
            if (!isPaused)
            {
                remainingTime -= Time.deltaTime;
                UpdateCountdownDisplay(remainingTime);
            }
            yield return null;
        }

        ShowWinPanel();
    }

// Update Countdown Display
    private void UpdateCountdownDisplay(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        countdownText.text = $"{minutes:00}:{seconds:00}";
    }

// Show Win Panel
    private void ShowWinPanel()
    {
        countdownTime = 0;
        PauseGame();
        winPanel.SetActive(true);
    }
}
