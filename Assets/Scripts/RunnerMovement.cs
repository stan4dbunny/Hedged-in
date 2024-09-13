using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunnerMovement : MonoBehaviour
{
    // Start is called before the first frame update
    public float moveSpeed = 5f;

    // Update is called once per frame
    void Update()
    {
        // Get input from the user for movement
        float moveHorizontal = Input.GetAxis("Horizontal"); // A/D or Left/Right arrow keys
        float moveVertical = Input.GetAxis("Vertical"); // W/S or Up/Down arrow keys

        // Calculate the direction of movement in the XZ plane (no Y movement)
        Vector3 movement = new Vector3(moveHorizontal, 0f, moveVertical);

        // Normalize movement to ensure consistent speed in all directions
        movement = movement.normalized * moveSpeed * Time.deltaTime;

        // Apply the movement to the player's position
        transform.Translate(movement, Space.World);
    }
}
