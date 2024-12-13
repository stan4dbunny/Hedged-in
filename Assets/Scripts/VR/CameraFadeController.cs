using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFadeController : MonoBehaviour
{
    public GameObject cameraObj;
    public float startFadeDistCamWander = 0.5f;
    public float fadeSpeed = 0.02f;
    public float alpha = 0;
    public Material material;
    public bool fadeToBlack = false;
    public bool isInWall = false;
    public Color color;

    void Start()
    {
        material = GetComponent<Renderer>().material;
    }

    void Update()
    {
        /*Vector3 LocalCameraPos = cameraObj.transform.localPosition;
        float distCamToParent = new Vector3(LocalCameraPos.x, 0, LocalCameraPos.z).magnitude;

        if(distCamToParent > startFadeDistCamWander && !isInWall)
        {
            fadeToBlack = true;
        }
        else if(distCamToParent < startFadeDistCamWander && !isInWall)
        {
            fadeToBlack =  false;
        }*/
        
    }

    void FixedUpdate() 
    {
        if(fadeToBlack)
        {
            if (isInWall && alpha < 0.1) //if we're in a wall, completely fade to black
            {
                //color = Color.black;
                color = Color.green;
                alpha += fadeSpeed;
            }
            /*else if(!isInWall && alpha < 0.3f) //keep it somewhat seethrough if we're not in a wall
            {
               color = Color.red;
               alpha += fadeSpeed;
            }*/
            
        }
        else if(!fadeToBlack && alpha >= fadeSpeed)
        {
            alpha -= fadeSpeed;
        }
        material.SetColor("_Color", color);
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