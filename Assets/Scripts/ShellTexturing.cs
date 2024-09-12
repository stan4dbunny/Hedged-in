using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellTexturing : MonoBehaviour
{
    Texture2D texture;
    public int resolution;
    public int layers;
    public float height;
    public float thickness;
    public float minimumHairLength;

    //public Texture2D blackAndWhiteTexture;

    public Material material;

    Renderer renderer;

    GameObject[] shells;

    void Start()
    {
        shells = new GameObject[layers];
        for (int i = 0; i < layers; i++)
        {
            shells[i] = new GameObject("Shell" + 1);
            shells[i].transform.SetParent(this.transform, false);
            shells[i].AddComponent<MeshFilter>();
            shells[i].AddComponent<MeshRenderer>();
            shells[i].GetComponent<MeshFilter>().mesh = GetComponent<MeshFilter>().mesh;
            shells[i].GetComponent<MeshRenderer>().material = material;


            shells[i].GetComponent<MeshRenderer>().material.SetInt("_Resolution", resolution);
            shells[i].GetComponent<MeshRenderer>().material.SetFloat("_Thickness", thickness);
            shells[i].GetComponent<MeshRenderer>().material.SetInt("_Layers", layers);
            shells[i].GetComponent<MeshRenderer>().material.SetFloat("_Height", height);
        }
        //give the regular texture to the shader/GPU
        renderer = GetComponent<Renderer>();
        // Access the mainTexture of the material
        Texture mainTexture = renderer.material.mainTexture;
        material.SetTexture("_MainTex", mainTexture);
        
    }

    private void Update()
    {
        int prevHeight = 0;
        for (int i = 0; i < layers; i++)
        {
            shells[i].GetComponent<MeshRenderer>().material.SetInt("_Resolution", resolution);
            shells[i].GetComponent<MeshRenderer>().material.SetFloat("_Thickness", thickness);
            shells[i].GetComponent<MeshRenderer>().material.SetInt("_Layers", layers);
            shells[i].GetComponent<MeshRenderer>().material.SetFloat("_Height", height);
            shells[i].GetComponent<MeshRenderer>().material.SetFloat("_PrevHeight", prevHeight / (float)layers);
            shells[i].GetComponent<MeshRenderer>().material.SetFloat("_CurrHeight", i);
            
            prevHeight = i;
        }
    }
}