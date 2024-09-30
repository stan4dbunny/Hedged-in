using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class GlobalVolumetricParams : MonoBehaviour
{
    public GameObject cloudContainer;
    void Update()
    {
        var (bBoxmin, bBoxmax) = GetBoundBoxMinAndMax(cloudContainer.transform);
        Shader.SetGlobalVector("BoundsMin", bBoxmin);
        Shader.SetGlobalVector("BoundsMax", bBoxmax);
    }

    private (Vector3, Vector3) GetBoundBoxMinAndMax(Transform transform)
    {
        Vector3 bBoxmin = new Vector3(transform.position.x - transform.localScale.x / 2, transform.position.y - transform.localScale.y / 2, transform.position.z - transform.localScale.z / 2);
        Vector3 bBoxmax = new Vector3(transform.position.x + transform.localScale.x / 2, transform.position.y + transform.localScale.y / 2, transform.position.z + transform.localScale.z / 2);

        return(bBoxmin, bBoxmax);
    }
}
