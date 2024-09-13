using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellTexturing : MonoBehaviour
{
    public int layers;
    public Material material;
    GameObject[] shells;

    void Start()
    {
        
        shells = new GameObject[layers];
        for (int i = 0; i < layers; i++)
        {
            shells[i] = new GameObject("Shell " + i);
            shells[i].transform.SetParent(this.transform, false);
            shells[i].layer = LayerMask.NameToLayer("OnlyVR");
            shells[i].AddComponent<MeshFilter>();
            shells[i].AddComponent<MeshRenderer>();
            shells[i].GetComponent<MeshFilter>().sharedMesh = GetComponent<MeshFilter>().sharedMesh;
            shells[i].GetComponent<MeshRenderer>().sharedMaterial = material;
            MaterialPropertyBlock matProp = new MaterialPropertyBlock();
            matProp.SetFloat("_CurrLayerIndex", i);
            shells[i].GetComponent<MeshRenderer>().SetPropertyBlock(matProp);

        }
    }
}