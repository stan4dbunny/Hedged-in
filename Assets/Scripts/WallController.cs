using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallController : MonoBehaviour
{
    private bool isRotating = false;
    private float rotationSpeed = 500f;
    private float rotationAngle = 90f;
    private Quaternion targetRotation;

    void Update()
    {
        if (isRotating)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
            {
                isRotating = false;
            }
        }
    }

    void OnMouseDown()
    {
        Debug.Log("Wall clicked!");
        if (!isRotating)
        {
            targetRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, rotationAngle, 0));
            isRotating = true;
        }
    }
}
