using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VRMovement : MonoBehaviour
{
    public Camera vRCam;
    public GameObject vRCamObj;
    public Camera nonVREditorTestCamera;
    public GameObject nonVREditorTestCameraObj;
    public GameObject mazeInfo;
    public GameObject leftHandController;
    public GameObject rightHandController;
    private float _inputVal;
    private Vector3 oldLeftHandPos = new Vector3(0, 0, 0);
    private Vector3 oldRightHandPos = new Vector3(0, 0, 0);
    private float moveFactor = 0;
    private bool nonVRMode = false;
    private float rotX = 0f;
    private float rotY = 0f;
    private float moveSpeed = 0.1f;
    private bool forward = false;

    void Start()
    {
        GenerateMaze mazeGenerator = mazeInfo.GetComponent<GenerateMaze>();
        float initialPlayerRot = mazeGenerator.GetInitialRotationOfPlayerFromStartBlock();
        vRCamObj.transform.eulerAngles = new Vector3(0, initialPlayerRot, 0);
    }

    void Update()
    {
        if(Application.platform == RuntimePlatform.WindowsEditor && Input.GetKeyDown(KeyCode.Tab) && nonVRMode == false)
        {
            SwapCameras();
        }

        if(nonVRMode == true)//Check for keyboard inputs here
        {
            rotX += Input.GetAxis("Mouse Y") * -1;
            rotY += Input.GetAxis("Mouse X");
            nonVREditorTestCamera.transform.eulerAngles = new Vector3(rotX, rotY, 0);
            transform.eulerAngles = new Vector3(0, rotY, 0);

            if(Input.GetKey(KeyCode.W) == true)
            {
                forward = true;
            }

            if(Input.GetKey(KeyCode.W) != true)
            {
                forward = false;
            }
        }
    }

    void FixedUpdate()
    {
        if(!nonVRMode)
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
                MovePlayerVR();
            }
        }
        else
        {
            if(forward == true)
            {
                MovePlayerNonVR();
            }
        }
        
    }

    private void MovePlayerVR()
    {
        Ray ray = vRCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 moveDir = ray.direction * 10;
        Vector3 pos = transform.position;

        Vector3 moveChange = moveDir * moveFactor * Time.deltaTime;
        pos += moveChange;

        if(Physics.Raycast(ray, out RaycastHit hit, 5))
        {
            if(hit.distance /*can tweak this toavoid phasing through walls*/ < moveChange.magnitude)
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

    private void MovePlayerNonVR()
    {
        Ray ray = nonVREditorTestCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if(Physics.Raycast(ray, out RaycastHit hit, 5))
        {
            if(hit.distance / 2 /*can tweak this toavoid phasing through walls*/< (transform.forward * moveSpeed).magnitude)
            {
                return;
            }
            else
            {
                transform.position += transform.forward * moveSpeed;
            }   
        }
    }

    private void SwapCameras()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        nonVRMode = true;
        vRCamObj.SetActive(false);
        nonVREditorTestCameraObj.SetActive(true);
    }
}


