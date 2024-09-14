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
    private Vector3 _oldLeftHandPos = new Vector3(0, 0, 0);
    private Vector3 _oldRightHandPos = new Vector3(0, 0, 0);
    private float _moveFactor = 0;
    private bool _nonVRMode = false;
    private float _rotX = 0f;
    private float _rotY = 0f;
    private float _moveSpeed = 0.1f;
    private bool _forward = false;

    void Start()
    {
        //This is here to set the initial rotation of the player, so they don't stare into a wall at the start.
        GenerateMaze mazeGenerator = mazeInfo.GetComponent<GenerateMaze>();
        float initialPlayerRot = mazeGenerator.GetInitialRotationOfPlayerFromStartBlock();
        vRCamObj.transform.eulerAngles = new Vector3(0, initialPlayerRot, 0);
        nonVREditorTestCameraObj.transform.eulerAngles = new Vector3(0, initialPlayerRot, 0);
    }

    void Update() //only here so you can test the game in play-mode in the editor
    {
        if(Application.platform == RuntimePlatform.WindowsEditor && Input.GetKeyDown(KeyCode.Tab) && !_nonVRMode)
        {
            SwapCameras();
        }

        if(_nonVRMode)//Check for keyboard inputs here
        {
            _rotX += Input.GetAxis("Mouse Y") * -1;
            _rotY += Input.GetAxis("Mouse X");
            nonVREditorTestCamera.transform.eulerAngles = new Vector3(_rotX, _rotY, 0);
            transform.eulerAngles = new Vector3(0, _rotY, 0);

            if(Input.GetKey(KeyCode.W) == true)
            {
                _forward = true;
            }

            if(Input.GetKey(KeyCode.W) != true)
            {
                _forward = false;
            }
        }
    }

    void FixedUpdate() //we do these things in FixedUpdate so they're not Fps-dependent.
    {
        if(!_nonVRMode)
        {
            //get hand controller positions in worldspace
            Vector3 leftHandPos = leftHandController.transform.position;
            Vector3 rightHandPos = rightHandController.transform.position;

            //calculate change in hand controller positions
            Vector3 deltaLeft = leftHandPos - _oldLeftHandPos;
            Vector3 deltaRight = rightHandPos - _oldRightHandPos;

            //check the magnitude of the resulting vectors
            float magnitudeDeltaLeft = deltaLeft.magnitude;
            float magnitudeDeltaRight = deltaRight.magnitude;
            //if we move the controllers a sufficent amount, set how much to move the player
            if(magnitudeDeltaLeft > 0.0015f && magnitudeDeltaRight > 0.0015f)
            {
                _moveFactor = Mathf.Abs(magnitudeDeltaLeft + magnitudeDeltaRight) * 2;
            }
            else{
                _moveFactor = 0;
            }

            //set old positions of hand controller to be used for the next FixedUpdate()-loop
            _oldLeftHandPos = leftHandPos;
            _oldRightHandPos = rightHandPos;

            //Move the player 
            if(_moveFactor > 0.1)
            {
                MovePlayerVR();
            }
        }
        else
        {
            if(_forward)
            {
                MovePlayerNonVR();
            }
        }
        
    }

    private void MovePlayerVR()
    {
        Ray ray = vRCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); //shoot ray, both to figure out move direction and closest object(if any)
        Vector3 moveDir = ray.direction * 10; //direction to move in is the direction of the ray
        Vector3 pos = transform.position;

        Vector3 moveChange = moveDir * _moveFactor * Time.deltaTime;
        pos += moveChange;

        if(Physics.Raycast(ray, out RaycastHit hit, 20) /*&& hit.transform.CompareTag("Wall")*/)
        {
            if(hit.distance/*can tweak this to avoid phasing through walls*/ < moveChange.magnitude) //collisison detection
            {
                return;
            }
            else
            {
                if(_moveFactor < 0.5f) //sanity check, too large values for _moveFactor will be ignored.
                {
                    transform.position = new Vector3(pos.x, 0, pos.z);
                }
            }
        }
    }

    private void MovePlayerNonVR()
    {
        Ray ray = nonVREditorTestCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if(Physics.Raycast(ray, out RaycastHit hit, 20) /*&& hit.transform.CompareTag("Wall")*/)
        {
            if(hit.distance / 2 /*can tweak this to avoid phasing through walls*/< (transform.forward * _moveSpeed).magnitude)
            {
                return;
            }
            else
            {
                transform.position += transform.forward * _moveSpeed;
            }   
        }
    }

    private void SwapCameras()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        _nonVRMode = true;
        vRCamObj.SetActive(false);
        nonVREditorTestCameraObj.SetActive(true);
    }
}


