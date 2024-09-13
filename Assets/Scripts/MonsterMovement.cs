using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class MonsterMovement : MonoBehaviour
{
    public GameObject player;
    public float speed;
    public float distanceBetween;
    private float distance;
    // Update is called once per frame
    void Update()
    {
        // Calculate the distance between the enemy and the player in 3D space
        distance = Vector3.Distance(transform.position, player.transform.position);

        // Calculate the direction from the enemy to the player
        Vector3 direction = player.transform.position - transform.position;

        // Calculate the angle to rotate the enemy towards the player on the Y-axis
        float angle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;

        if (distance < distanceBetween)
        {
            // Move the enemy towards the player
            transform.position = Vector3.MoveTowards(transform.position, player.transform.position, speed * Time.deltaTime);

            // Rotate the enemy to face the player
            transform.rotation = Quaternion.Euler(0, -angle, 0);
        }
    }
}

