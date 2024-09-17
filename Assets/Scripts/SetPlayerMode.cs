using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SetPlayerMode : NetworkBehaviour
{
    [SerializeField] public GameObject playerVR;
    [SerializeField] public GameObject playerTopDown;
    private GameObject vRCam;
    private GameObject topDownCam;
    public int netId;
    private bool playerModeSet = false;
    
    void Update()
    {

        if(netId == 0)
        {
            ulong netObjId = GetComponent<NetworkObject>().NetworkObjectId;
            netId = (int)netObjId;
        }

        if(!playerModeSet)
        {
            if(IsOwner && netId == 1)
            {
                vRCam = GameObject.Find("VRCameraHolder");
                playerModeSet = true;

                vRCam.transform.GetChild(0).gameObject.SetActive(true); //PlayerCamera
                playerVR.SetActive(true);
            }
            else if(!IsOwner && netId == 1)
            {
                playerVR.GetComponent<VRMovement>().transform.gameObject.SetActive(false);
                playerVR.transform.GetChild(0).GetChild(0).gameObject.SetActive(false); //XR Origin(XR Rig)
                playerVR.transform.GetChild(0).GetChild(1).gameObject.SetActive(false); //PlayerInputs
                playerVR.SetActive(true);   
            }
            else if(IsOwner)
            {
                if(GameObject.Find("NetworkCanvas"))
                {
                    GameObject.Find("NetworkCanvas").SetActive(false);
                }

                topDownCam = GameObject.Find("TopViewCamHolder");
                playerModeSet = true;

                topDownCam.transform.GetChild(0).gameObject.SetActive(true); //TopViewCam
                playerTopDown.SetActive(true);
            }
        }
    }
}
