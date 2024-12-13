using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetUICamPos : MonoBehaviour
{
    public Transform playerCam;
    public GameObject uiCam;

    void Update()
    {
        if(uiCam.activeSelf)
        {
            transform.position = playerCam.position;
            transform.eulerAngles = playerCam.eulerAngles;
        }
    }
}
