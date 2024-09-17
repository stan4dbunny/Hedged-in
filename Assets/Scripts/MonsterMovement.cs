using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class MonsterMovement : MonoBehaviour
{
    public Transform player;
    public bool playerTracking = false;
    public float speed;
    public float distanceBetween;
    private float distance;
    // Update is called once per frame
    void Update()
    {   
        if(GameObject.Find("PlayerNetwork(Clone)") && !playerTracking) //Enter here when a player spawn due to joining the network session
        {
            SetTarget();//Sets target to chase
        }

        if(playerTracking)
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

    private void SetTarget()
    {
        playerTracking = true;
        GameObject playerParent = GameObject.Find("PlayerNetwork(Clone)");
        player = playerParent.transform.GetChild(0); //Only works if the PlayerVR-component of the PlayerNetwork-prefab is the 4th component
    }
}

