using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlaceDistraction : MonoBehaviour
{
    [SerializeField] public Camera topDownView;
    [SerializeField] public GameObject distraction;
    [SerializeField] public GameObject distractionIndicator;
    [SerializeField] public GameObject rangeIndicator;
    private bool holdingDistraction = false;
    [SerializeField] TextMeshProUGUI distractionsLeftText;
    public int distractionsAmount = 5;
    private GameObject currDistraction;
    private GameObject currRangeIndicator;
    //TODO: Cooldown for using distraction/limiting it to only one placed at a time (If we want to handle more, change MonsterMovement.cs is neccessary)
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Mouse1) && holdingDistraction)
        {
            Ray ray = topDownView.ScreenPointToRay(Input.mousePosition); //when we rightclick, shoot a ray from the topView-camera plane to where the user clicked

            if(Physics.Raycast(ray, out RaycastHit hit, 100)) //place distraction object
            {
                int x = Mathf.RoundToInt(hit.point.x);
                int z = Mathf.RoundToInt(hit.point.z);
                
                if(x >= 0 && z >= 0 && x < 10 && z < 10) //in bounds of the maze, current boundaries hardcoded
                {
                    Instantiate(distraction, new Vector3(x, 0.2f, z), Quaternion.identity);
                    Destroy(currDistraction);
                    Destroy(currRangeIndicator);
                    distractionsAmount -= 1;
                    distractionsLeftText.text = "Distractions left: " + distractionsAmount;
                    holdingDistraction = false;
                }
            }
        }

        if(Input.GetKeyDown(KeyCode.Mouse1) && !holdingDistraction && !GameObject.FindWithTag("distraction") && distractionsAmount > 0)
        {
            holdingDistraction = true;
            Ray ray = topDownView.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(ray, out RaycastHit hit, 100))
            {
                int x = Mathf.RoundToInt(hit.point.x);
                int z = Mathf.RoundToInt(hit.point.z);

                currDistraction = Instantiate(distractionIndicator, new Vector3(x, 0.2f, z), Quaternion.identity);
                currRangeIndicator = Instantiate(rangeIndicator, new Vector3(x, 0f, z), Quaternion.identity);
            }
        }

        if(currDistraction && currRangeIndicator)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos = topDownView.ScreenToWorldPoint(mousePos);
            currDistraction.transform.position = new Vector3(mousePos.x, 0.2f, mousePos.z);
            currRangeIndicator.transform.position = new Vector3(mousePos.x, 0, mousePos.z);
        }
    }
}
