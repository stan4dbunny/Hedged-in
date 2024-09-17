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
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        if(!IsOwner) return;
        
        if(netId == 0)
        {
            ulong netObjId = GetComponent<NetworkObject>().NetworkObjectId;
            netId = (int)netObjId;
        }

        if(!playerModeSet)
        {
            if(netId == 1)
            {
                vRCam = GameObject.Find("VRCameraHolder");
                playerModeSet = true;

                vRCam.transform.GetChild(0).gameObject.SetActive(true);
                playerVR.SetActive(true);
            }
            else
            {
                if(GameObject.Find("NetworkCanvas"))
                {
                    GameObject.Find("NetworkCanvas").SetActive(false);
                }

                topDownCam = GameObject.Find("TopViewCamHolder");
                playerModeSet = true;

                topDownCam.transform.GetChild(0).gameObject.SetActive(true);
                playerTopDown.SetActive(true);
            }
        }
    }
}
