using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenManager : MonoBehaviour
{
    // Reference to the Welcome Screen UI Canvas
    public GameObject welcomeScreenCanvas;

    // Reference to the Gameplay UI Canvas (the UI that contains life bar and timer)
    public GameObject gameplayUICanvas;

    // Static variable to keep track of whether the Welcome Screen was already shown
    public static bool hasGameStarted = false;

    private void Start()
    {
        // If the game has already started, skip the Welcome Screen
        if (hasGameStarted)
        {
            ShowGameplayUI();
        }
        else
        {
            ShowWelcomeScreen();
        }
    }

    // This function will be called when the Start Game button is clicked
    public void StartGame()
    {
        // Set the flag that the game has started
        hasGameStarted = true;

        // Hide the Welcome Screen and show the Gameplay UI
        ShowGameplayUI();
    }

    // Show the Welcome Screen and hide the Gameplay UI
    private void ShowWelcomeScreen()
    {
        if (welcomeScreenCanvas != null)
        {
            welcomeScreenCanvas.SetActive(true); // Show Welcome Screen
        }

        if (gameplayUICanvas != null)
        {
            gameplayUICanvas.SetActive(false); // Hide Gameplay UI
        }
    }

    // Show the Gameplay UI and hide the Welcome Screen
    private void ShowGameplayUI()
    {
        if (welcomeScreenCanvas != null)
        {
            welcomeScreenCanvas.SetActive(false); // Hide Welcome Screen
        }

        if (gameplayUICanvas != null)
        {
            gameplayUICanvas.SetActive(true); // Show Gameplay UI
        }
    }
}