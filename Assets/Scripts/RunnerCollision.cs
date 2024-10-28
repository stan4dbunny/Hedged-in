using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RunnerCollision : MonoBehaviour
{
    private int count = 5;
    public AudioClip collectableClip;
    private AudioSource audioSource;
    public GameObject healthBar;
    private Slider lifeSlider;
    public GameObject gameOverCanvas;
    public GameObject gameplayCanvas;
    public GameObject winCanvas;
    public GameObject gameOverCanvasVR;
    public GameObject winCanvasVR;
    public Camera vrUICamera;
    public GameObject rightController;
    public GameObject leftController;
    public GameObject rightControllerUI;
    public GameObject leftControllerUI;

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
            lifeSlider = healthBar.GetComponent<Slider>();
            if (lifeSlider != null)
            {
                lifeSlider.maxValue = 5;  // Set max slider value to the initial life count
                lifeSlider.value = count; // Initialize slider to the player's starting lives
            }
        }

        // Hide all the canvases initially
        if (winCanvas != null) winCanvas.SetActive(false);
        if (winCanvasVR != null) winCanvasVR.SetActive(false);
        if (gameOverCanvas != null) gameOverCanvas.SetActive(false);
        if (gameOverCanvasVR != null) gameOverCanvasVR.SetActive(false);
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
        // Hide gameplay canvases
        if (gameplayCanvas != null) gameplayCanvas.SetActive(false);

        // Display the Game Over UI for both desktop and VR players
        transform.position = new Vector3(vrUICamera.transform.position.x, 0, vrUICamera.transform.position.z); //moves player to where UI camera is

        if (vrUICamera != null) vrUICamera.gameObject.SetActive(true);
        if (gameOverCanvas != null) gameOverCanvas.SetActive(true);  // Show desktop Game Over Canvas
        if (gameOverCanvasVR != null) gameOverCanvasVR.SetActive(true);  // Show VR Game Over Canvas
        EnableUIControllers();
        Debug.Log("Game Over!");
    }

    // Handle the win logic
    private void WinGame()
    {
        // Hide other UI elements 
        if (gameplayCanvas != null) gameplayCanvas.SetActive(false);
        transform.position = new Vector3(vrUICamera.transform.position.x, 0, vrUICamera.transform.position.z); //moves player to where UI camera is

        // Show the Win UI for both desktop and VR
        if (vrUICamera != null) vrUICamera.gameObject.SetActive(true);
        if (winCanvas != null) winCanvas.SetActive(true);
        if (winCanvasVR != null) winCanvasVR.SetActive(true);
        EnableUIControllers();
        Debug.Log("Game Won!");
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

    public void EnableUIControllers()
    {
        if(rightController != null && rightControllerUI != null)
        {
            rightController.SetActive(false);
            rightControllerUI.SetActive(true);
        }

        if(leftController != null && leftControllerUI != null)
        {
            leftController.SetActive(false);
            leftControllerUI.SetActive(true);
        }
    }
}
