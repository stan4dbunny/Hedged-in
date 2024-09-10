using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VRMovement : MonoBehaviour
{
    public Camera vRCam;
    public GameObject mazeInfo;
    public GameObject leftHandController;
    public GameObject rightHandController;
    private float _inputVal;
    private Vector3 oldLeftHandPos = new Vector3(0, 0, 0);
    private Vector3 oldRightHandPos = new Vector3(0, 0, 0);
    private float moveFactor = 0;

    void Start()
    {
        GenerateMaze mazeGenerator = mazeInfo.GetComponent<GenerateMaze>();
        float initialPlayerRot = mazeGenerator.GetInitialRotationOfPlayerFromStartBlock();
        this.transform.eulerAngles = new Vector3(0, initialPlayerRot, 0);
    }

    void FixedUpdate()
    {
        Vector3 leftHandPos = leftHandController.transform.position;
        Vector3 rightHandPos = rightHandController.transform.position;

        Vector3 deltaLeft = leftHandPos - oldLeftHandPos;
        Vector3 deltaRight = rightHandPos - oldRightHandPos;

        float magnitudeDeltaLeft = deltaLeft.magnitude;
        float magnitudeDeltaRight = deltaRight.magnitude;

        if(magnitudeDeltaLeft > 0.01f && magnitudeDeltaRight > 0.01f)
        {
            moveFactor = Mathf.Abs(magnitudeDeltaLeft + magnitudeDeltaRight);
        }
        else{
            moveFactor = 0;
        }

        oldLeftHandPos = leftHandPos;
        oldRightHandPos = rightHandPos;

        if(moveFactor > 0.1)
        {
            movePlayer();
        }
    }

    private void movePlayer()
    {
        Ray ray = vRCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 moveDir = ray.direction * 10;
        Vector3 pos = transform.position;

        Vector3 moveChange = moveDir * moveFactor * Time.deltaTime;
        pos += moveChange;

        if(Physics.Raycast(ray, out RaycastHit hit, 5))
        {
            if(hit.distance < moveChange.magnitude)
            {
                return;
            }
            else
            {
                if(moveFactor < 0.5f)
                {
                    transform.position = new Vector3(pos.x, 0, pos.z);
                }
            }
        }
    }
}


