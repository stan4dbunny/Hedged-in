using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveDoor : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 offset;
    private Plane plane;
    
    
    void Start()
    {
        // Create a plane to calculate movement on
        plane = new Plane(Vector3.up, transform.position); 

    }
    

    void Update()
    {
        // Check if the door is being dragged
        if (isDragging)
        {
            // Cast a ray from the camera to the mouse position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float distance;

            // Check if the ray intersects the plane
            if (plane.Raycast(ray, out distance))
            {
                // Calculate the new position for the door
                Vector3 point = ray.GetPoint(distance) + offset;
                float clampedX = Mathf.Clamp(point.x, 0, 9);
                float clampedZ = Mathf.Clamp(point.z, 0, 9);
                transform.position = new Vector3(clampedX, transform.position.y, clampedZ);
            }
            
            // Release the door when the mouse button is released
            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
            }
        }
    }

    void OnMouseDown()
    {
        // When clicking the door, set it to dragging mode
        isDragging = true;

        // Cast a ray from the camera to the mouse position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float distance;

        // Check if the ray intersects the plane
        if (plane.Raycast(ray, out distance))
        {
            // Calculate the offset between the door position and the click point
            offset = transform.position - ray.GetPoint(distance);
        }
    }

   
}
