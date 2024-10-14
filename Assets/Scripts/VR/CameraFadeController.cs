using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFadeController : MonoBehaviour
{
    public GameObject cameraObj;
    public float startFadeDistCamWander = 0.2f;
    public float fadeSpeed = 0.05f;
    private float alpha = 0;
    private Material material;
    private bool fadeToBlack = false;
    private bool isInWall = false;

    void Start()
    {
        material = GetComponent<Renderer>().material;
    }

    void Update()
    {
        Vector3 LocalCameraPos = cameraObj.transform.localPosition;
        float distCamToParent = new Vector3(LocalCameraPos.x, 0, LocalCameraPos.z).magnitude;

        if(distCamToParent > startFadeDistCamWander && !isInWall)
        {
            fadeToBlack = true;
        }
        else if(distCamToParent < startFadeDistCamWander && !isInWall)
        {
            fadeToBlack =  false;
        }
        
    }

    void FixedUpdate() 
    {
        if(fadeToBlack && alpha < 1)
        {
            alpha += fadeSpeed;
        }
        else if(!fadeToBlack && alpha > 0)
        {
            alpha -= fadeSpeed;
        }
        material.SetFloat("_Alpha", alpha);
    }

    private void OnCollisionEnter(Collision collision) 
    {
        
        if(collision.gameObject.tag == "Wall")
        {
            isInWall = true;
            fadeToBlack = true;
        }
    }

    private void OnCollisionExit(Collision collision) 
    {
        if(collision.gameObject.tag == "Wall")
        {
            isInWall = false;
            fadeToBlack = false;
        }
    }
}
