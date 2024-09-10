using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RunnerCollision : MonoBehaviour
{
    // This function is called when the runnet collides with another collider
    private int count = 0; 
    private void OnCollisionEnter(Collision collision)
    {
        // Check if the object collided with is tagged as "Monster"
        if (collision.gameObject.CompareTag("Monster"))

        {
            if (count < 5)
            {
                count = count + 1; 
            }
            else
            {
                // Call the GameOver function
                GameOver();
            }
            
        }

        // Check if the object collided with is tagged as "EndPoint"
        if (collision.gameObject.CompareTag("EndPoint"))
        {
            // Call the GameWon function
            GameWon();
        }
    }

  


    // Handle the game over logic
    private void GameOver()
    {
        // You can display a Game Over UI, stop the game, or reload the scene, etc.
        Debug.Log("Game Over!");
        SceneManager.LoadScene("GameOver"); ;


    }

    private void GameWon()
    {
        // You can display a Game Over UI, stop the game, or reload the scene, etc.
        Debug.Log("Congratulations Game Won!");


    }
}
