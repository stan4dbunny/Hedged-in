using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallController : MonoBehaviour
{
    private bool isRotating = false;
    private bool rotateForward = true;
    private float rotationSpeed = 500f;
    private float rotationAngle = 90f;
    private Quaternion targetRotation;
    private GameObject mazeGenerator;
    private GenerateMaze mazeInfo;

    void Start()
    {
        mazeGenerator = GameObject.Find("MazeGenerator"); //important the the object generating the maze hsa this name (For now, maybe can get it some other way)
        mazeInfo = mazeGenerator.GetComponent<GenerateMaze>();
    }

    void Update()
    {
        if (isRotating)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.01f)
            {
                isRotating = false;
                mazeInfo.UpdateNavMesh();
            }
        }
    }

    void OnMouseDown()
    {
        if (!isRotating)
        {
            float direction = rotateForward ? rotationAngle : -rotationAngle;
            targetRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, direction, 0));
            rotateForward = !rotateForward;
            isRotating = true;
        }
    }
}
