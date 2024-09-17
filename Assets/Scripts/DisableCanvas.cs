using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableCanvas : MonoBehaviour
{
    private bool objFound = false;
    public GameObject canvas; //remove this when testing is done
    // Update is called once per frame
    void Update()
    {
        if(!objFound)
        {
            if(GameObject.Find("PlayerNetwork(Clone)"))
            {
                objFound = true;
                this.gameObject.SetActive(false);
                //canvas.SetActive(false);
            }
        }
    }
}
