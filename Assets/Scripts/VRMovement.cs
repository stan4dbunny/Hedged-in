using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VRMovement : MonoBehaviour
{
    public InputActionProperty primaryAction;
    public Camera playerCam; 

    void Update()
    {
        float val = primaryAction.action.ReadValue<float>();
        if(val != 0f)
        {
            Debug.Log(val);
            Debug.Log(Camera.main.transform.position);
        }
    }
}


