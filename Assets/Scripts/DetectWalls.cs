using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectWalls : MonoBehaviour
{
    public Material interactableMaterial;

    public float delayBeforeChecking = 1.0f;

    public List<GameObject> walls = new List<GameObject>();
    public List<GameObject> moveableWalls = new List<GameObject>();
    void Start()
    {
        StartCoroutine(DetectWallsAfterDelay());

    }

    IEnumerator DetectWallsAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeChecking);
    
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        Debug.Log($"allobject: {allObjects.Length}");

        foreach (GameObject obj in allObjects)
        {
            if (obj.name == "Wall")
            {
                walls.Add(obj);
            }
        }
        
        foreach (GameObject wall in walls)
        {
            Renderer wallRenderer = wall.GetComponentInChildren<MeshRenderer>();
            Debug.Log($"renderer: {wallRenderer.sharedMaterial}");
            Debug.Log($"interactive material: {interactableMaterial}");
            
            if (wallRenderer.sharedMaterial == interactableMaterial)
            {
                Debug.Log("Same!");
                moveableWalls.Add(wall);
                wall.AddComponent<WallController>();
            }
            
            
        }
        
        Debug.Log($"Dark walls: {moveableWalls.Count}");
       
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
