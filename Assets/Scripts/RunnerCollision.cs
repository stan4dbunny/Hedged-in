using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RunnerCollision : MonoBehaviour
{
    // This count determines the Runner's lives. The Runner starts with 5 lives.
    // Every time it encounters a monster it loses a life.
    // When it encounters a collectable it gains a life. 
    private int count = 5;

    // This is the sound that plays when the player picks up a collectable 
    public AudioClip collectableClip;
    private AudioSource audioSource;

    // Reference to the HealthBar GameObject
    public GameObject healthBar;

    // The Slider component from the HealthBar
    private Slider lifeSlider;

    public GameObject gameOverCanvas;
    public GameObject gameplayCanvas;
    public GameObject winCanvas;

    private void Start()
    {
        // Initialize the AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Ensure the HealthBar GameObject is set
        if (healthBar != null)
        {
            // Get the Slider component from the HealthBar
            lifeSlider = healthBar.GetComponent<Slider>();
            if (lifeSlider != null)
            {
                lifeSlider.maxValue = 5;  // Set max slider value to the initial life count
                lifeSlider.value = count; // Initialize slider to the player's starting lives
            }
        }
        else
        {
            Debug.LogError("HealthBar GameObject is not assigned.");
        }
        // Ensure the Win UI Canvas is hidden at the start
        if (winCanvas != null)
        {
            winCanvas.SetActive(false);
        }
        else
        {
            Debug.LogError("Win Canvas GameObject is not assigned.");
        }
    }

    // This function is called when the Runner collides with another collider
    private void OnCollisionEnter(Collision collision)
    {
        // Check if the object collides with is tagged as "Monster"
        if (collision.gameObject.CompareTag("Monster"))
        {
            if (count > 0)
            {
                count = count - 1;
                Debug.Log(count);
                UpdateLifeSlider(); // Update the UI Slider when life is lost
            }
            else
            {
                // Call the GameOver function
                GameOver();
            }
        }

        // Check if the object collides with is tagged as "EndPoint"
        if (collision.gameObject.CompareTag("EndPoint"))
        {
            WinGame(); // Call the WinGame function

        }

        // Check if the object collides with is tagged as "Collectable"
        if (collision.gameObject.CompareTag("Collectable"))
        {
            count = count + 1;
            Debug.Log(count);
            UpdateLifeSlider(); // Update the UI Slider when life is gained

            // Play the collectable sound
            if (collectableClip != null)
            {
                audioSource.PlayOneShot(collectableClip);
            }

            // Destroys the collectable object to make it vanish
            Destroy(collision.gameObject);
        }
    }

    // Handle the game over logic
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

    // Handle the win logic
    private void WinGame()
    {
        // Show the Win UI Canvas and hide the main UI
        if (winCanvas != null)
        {
            winCanvas.SetActive(true);
        }

        // Hide other UI elements 
        if (healthBar != null)
        {
            healthBar.SetActive(false);
        }

        // Stop game logic
        //Time.timeScale = 0; // Pause the game
    }

    private void GameWon()
    {
        // MARITINA here you can display a Congratulations UI
        Debug.Log("Congratulations, Game Won!");
    }

    // Update the slider UI element to reflect the current life count
    private void UpdateLifeSlider()
    {
        if (lifeSlider != null)
        {
            lifeSlider.value = count;
        }
    }

    // This function will be called when the restart button is clicked
    public void RestartGame()
    {
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}