using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraColliderFade : MonoBehaviour
{
    public GameObject cameraObj;
    public float startFadeDist = 0.05f;
    private SphereCollider thisCollider;
    private Material material;
    private float colliderRadius;

    void Start()
    {
        thisCollider = GetComponent<SphereCollider>();
        material = GetComponent<Renderer>().material;
        colliderRadius = thisCollider.radius;
    }

    private void OnCollisionStay(Collision collision)
    {
        if(collision.gameObject.tag == "Wall")
        {
            Vector3 pos = cameraObj.transform.position;
            Vector3 posCol = collision.contacts[0].point;

            float distToCenter = (pos - posCol).magnitude;
            float remapRatio = colliderRadius - startFadeDist;

            float rescaleDist = Mathf.Abs(((distToCenter - startFadeDist) * 1/remapRatio) - 1); //remaps vaöues to range [0, 1], where 0 is not in collider, and 1 is fully in

            material.SetFloat("_Alpha", rescaleDist);
        }
    }
    private void OnCollisionExit(Collision collision) 
    {
        if(collision.gameObject.tag == "Wall")
        {
            material.SetFloat("_Alpha", 0);
        }
    }
}
