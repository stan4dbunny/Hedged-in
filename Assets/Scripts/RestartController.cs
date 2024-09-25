using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RestartController : MonoBehaviour
{
    void Start()
    {
        // Unlock and make cursor visible
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // This method will be triggered when the restart button is clicked
    public void RestartButton()
    {
        // Load the scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

