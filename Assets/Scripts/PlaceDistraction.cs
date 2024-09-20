using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceDistraction : MonoBehaviour
{
    [SerializeField] public Camera topDownView;
    [SerializeField] public GameObject distraction;

    //TODO: Cooldown for using distraction/limiting it to only one placed at a time (If we want to handle more, change MonsterMovement.cs is neccessary)
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Mouse1))
        {
            Ray ray = topDownView.ScreenPointToRay(Input.mousePosition); //when we rightclick, shoot a ray from the topView-camera plane to where the user clicked

            if(Physics.Raycast(ray, out RaycastHit hit, 100)) //place distraction object
            {
                int x = Mathf.RoundToInt(hit.point.x);
                int z = Mathf.RoundToInt(hit.point.z);
                Instantiate(distraction, new Vector3(x, 0.2f, z), Quaternion.identity);
            }
        }
    }
}
