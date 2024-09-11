using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RunnerCollision : MonoBehaviour
{
    // This count determines the Runner's lives. The Runner starts with 5 lives.
    // Every time it encounters a monster it loses a life.
    // When it encounters a collectable it gains a life. 
    private int count = 5;

    // This is the sound that plays when the player picks up a collectable 
    public AudioClip collectableClip;
    private AudioSource audioSource;

    private void Start()
    {
        // Initialize the AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // This function is called when the Runner collides with another collider
    private void OnCollisionEnter(Collision collision)
    {
        // Check if the object collides with is tagged as "Monster"
        if (collision.gameObject.CompareTag("Monster"))
        {
            if (count >= 1)
            {
                count = count - 1;
                Debug.Log(count);
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
            // Call the GameWon function
            GameWon();
        }

        // Check if the object collides with is tagged as "Collectable"
        if (collision.gameObject.CompareTag("Collectable"))
        {
            count = count + 1;
            Debug.Log(count);

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
        // MARITINA here you can display a Game Over UI
        Debug.Log("Game Over!");
        SceneManager.LoadScene("GameOver");
    }

    private void GameWon()
    {
        // MARITINA here you can display a Congratulations UI
        Debug.Log("Congratulations, Game Won!");
    }
}
