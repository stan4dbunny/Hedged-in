using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Import TextMesh Pro namespace
using UnityEngine.SceneManagement; // For scene management (Game Over)

public class CountdownTimer : MonoBehaviour
{
    // Time in minutes 
    public float timeInMinutes;

    // Internal variable to store time in seconds
    private float timer;

    // Reference to the TextMesh component that displays the countdown
    public TMP_Text countdownText;

    public GameObject gameOverCanvas;
    public GameObject gameplayCanvas;

    private void Start()
    {
        // Initialize the timer in seconds (convert minutes to seconds)
        timer = timeInMinutes * 60;
    }

    private void Update()
    {
        // Decrease the timer each frame
        timer -= Time.deltaTime;

        // Clamp the timer so it doesn't go below 0
        if (timer < 0)
        {
            timer = 0;
            GameOver();
        }

        // Format and display the time as "00:MM:SS"
        int minutes = Mathf.FloorToInt(timer / 60); // Get minutes
        int seconds = Mathf.FloorToInt(timer % 60); // Get remaining seconds

        // Update the TextMesh with the formatted time
        countdownText.text = string.Format("00:{0:00}:{1:00}", minutes, seconds); // Format as 00:MM:SS
    }

    // Trigger Game Over
    private void GameOver()
    {
        gameplayCanvas.SetActive(false);
        // Display the Game Over UI
        Debug.Log("Game Over!");
        if (gameOverCanvas != null)
        {
            gameOverCanvas.SetActive(true); // Show the Game Over Canvas
        }
    }

    // This function will be called when the restart button is clicked
    public void RestartGame()
    {
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}