using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetLocalPosition : MonoBehaviour
{
    public GameObject target;
    public Vector3 manualPos;

    void Update()
    {
        if(target == null)
        {
            transform.localPosition = manualPos;
            transform.localRotation = Quaternion.identity;
        }
        else
        {
            switch(gameObject.tag)
            {
                case "playerModel":
                    transform.localPosition = new Vector3(target.transform.localPosition.x, -0.87f, target.transform.localPosition.z);
                    Vector3 targetRot = target.transform.eulerAngles;
                    transform.localRotation = Quaternion.Euler(0, targetRot.y, 0);
                    break;
                default:
                    transform.localPosition = target.transform.localPosition;
                    transform.localRotation = Quaternion.identity;
                    break;
            } 
        } 
    }
}
