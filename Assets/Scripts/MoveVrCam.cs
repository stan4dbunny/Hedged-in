using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveVrCam : MonoBehaviour
{
    public Transform cameraPosition;
    // Update is called once per frame
    void Update()
    {
        if(GameObject.Find("PlayerNetwork(Clone)") && GameObject.Find("PlayerNetwork(Clone)").GetComponent<SetPlayerMode>().netId == 1)
        {
            cameraPosition = GameObject.Find("PlayerNetwork(Clone)").transform.GetChild(0).GetChild(0).GetChild(0);
            transform.position = cameraPosition.position;
        }
    }
}
