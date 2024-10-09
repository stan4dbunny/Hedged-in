using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetLocalPosition : MonoBehaviour
{
    void Update()
    {
        transform.localPosition = new Vector3(0, 0, 0);
        transform.localRotation = Quaternion.identity;
    }
}
