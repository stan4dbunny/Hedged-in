using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenManager : MonoBehaviour
{
    public GameObject welcomeScreenCanvas;
    public GameObject welcomeScreenCanvasVR;
    public Camera vrUICamera;
    public GameObject gameplayUICanvas;

    public static bool hasGameStarted = false;

    private void Start()
    {
        // If the game has already started, skip the Welcome Screen for both players
        if (hasGameStarted)
        {
            ShowGameplayUI();
        }
        else
        {
            ShowWelcomeScreen();
        }
    }

    // This function will be called when the Start Game button is clicked on the desktop player's UI
    public void StartGame()
    {
        // Set the flag that the game has started
        hasGameStarted = true;

        // Hide both Welcome Screens and show the Gameplay UI
        ShowGameplayUI();
    }

    // Show the Welcome Screen and hide the Gameplay UI
    private void ShowWelcomeScreen()
    {
        if (welcomeScreenCanvas != null)
        {
            welcomeScreenCanvas.SetActive(true); // Show desktop Welcome Screen
        }

        if (welcomeScreenCanvasVR != null)
        {
            welcomeScreenCanvasVR.SetActive(true); // Show VR Welcome Screen
        }

        if (gameplayUICanvas != null)
        {
            gameplayUICanvas.SetActive(false); // Hide Gameplay UI
        }

        if (vrUICamera != null)
        {
            vrUICamera.gameObject.SetActive(true); // Activate the VR UI Camera
        }
    }

    // Show the Gameplay UI and hide the Welcome Screen for both desktop and VR
    private void ShowGameplayUI()
    {
        if (welcomeScreenCanvas != null)
        {
            welcomeScreenCanvas.SetActive(false); // Hide desktop Welcome Screen
        }

        if (welcomeScreenCanvasVR != null)
        {
            welcomeScreenCanvasVR.SetActive(false); // Hide VR Welcome Screen
        }

        if (gameplayUICanvas != null)
        {
            gameplayUICanvas.SetActive(true); // Show Gameplay UI
        }

        if (vrUICamera != null)
        {
            vrUICamera.gameObject.SetActive(false); // Deactivate the VR UI Camera
        }
    }
}
