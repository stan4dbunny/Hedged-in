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
    [SerializeField] TextMeshProUGUI distractionsLeftText;
    private GameObject currDistractionIndicator;
    private GameObject currRangeIndicator;
    private Vector3 mousePos;
    public int distractionsAmount = 5;
    private bool holdingDistraction = false;
    private bool clickedThisFrame = false;
    private GameObject mazeGenerator;
    private GenerateMaze mazeInfo;

    void Start()
    {
        mazeGenerator = GameObject.Find("MazeGenerator");
        mazeInfo = mazeGenerator.GetComponent<GenerateMaze>();
    }
    void Update()
    {
        //Get mouse position and it's respective maze index.
        mousePos = Input.mousePosition;
        mousePos = topDownView.ScreenToWorldPoint(mousePos);
        int x = Mathf.RoundToInt(mousePos.x);
        int z = Mathf.RoundToInt(mousePos.z);

        if(Input.GetKeyDown(KeyCode.Mouse1) && holdingDistraction) //if we press right-click and holding the indicator
        {
            if(IsClickInBounds(x, z)) //if we click in bounds
            {
                holdingDistraction = false;
                clickedThisFrame = true;

                PlaceDistractionInMaze(x, z); //place monster distraction in maze
            }

            if(!IsClickInBounds(x, z) && !clickedThisFrame) //click outside of maze bounds to get rid of potential distraction placement TODO: Bind to UI button click or something
            {
                holdingDistraction = false;
                clickedThisFrame = true;

                ClearHoldingObject();
            }     
        }

        if(Input.GetKeyDown(KeyCode.Mouse1) && !holdingDistraction && !GameObject.FindWithTag("distraction") && distractionsAmount > 0 && !clickedThisFrame) //condition for potentially placing down more distractions
        {
            holdingDistraction = true;
            clickedThisFrame = true;

            SpawnIndicatorsOnMousePos();
        }

        if(currDistractionIndicator && currRangeIndicator) //if these gameobjects exist, track them to the mouseposition.
        {
            TrackIndicatorsToMousePos(x, z);
        }

        clickedThisFrame = false;
    }

    private bool IsClickInBounds(int x, int z)
    {
        if(x >= 0 && z >= 0 && x < mazeInfo.mazeWidth * mazeInfo.scaleFactor && z < mazeInfo.mazeHeight * mazeInfo.scaleFactor){return true;}

        return false;
    }

    private void PlaceDistractionInMaze(int x, int z)
    {
        Instantiate(distraction, new Vector3(x, 0.2f, z), Quaternion.identity);
        ClearHoldingObject();
        UpdateUIText();
    }

    private void UpdateUIText()
    {
        distractionsAmount -= 1;
        distractionsLeftText.text = "Distractions left: " + distractionsAmount;
    }

    private void ClearHoldingObject()
    {
        Destroy(currDistractionIndicator);
        Destroy(currRangeIndicator);
    }

    private void SpawnIndicatorsOnMousePos()
    {
        currDistractionIndicator = Instantiate(distractionIndicator, new Vector3(mousePos.x, 0.2f, mousePos.z), Quaternion.identity);
        currRangeIndicator = Instantiate(rangeIndicator, new Vector3(mousePos.x, 0f, mousePos.z), Quaternion.identity);
        currRangeIndicator.transform.localScale = new Vector3(currRangeIndicator.transform.localScale.x * mazeInfo.scaleFactor, 1, currRangeIndicator.transform.localScale.z * mazeInfo.scaleFactor);
    }

    private void TrackIndicatorsToMousePos(int x, int z)
    {
        //snaps to part of maze object will actually be placed at
        currDistractionIndicator.transform.position = new Vector3(x, 0.2f, z);
        currRangeIndicator.transform.position = new Vector3(x, 0, z);

        //Tracks mouse continuously
        /*
        currDistractionIndicator.transform.position = new Vector3(mousePos.x, 0.2f, mousePos.z);
        currRangeIndicator.transform.position = new Vector3(mousePos.x, 0, mousePos.z);
        */

        DetermineIndicatorColor(x, z);
    }

    private void DetermineIndicatorColor(int x, int z)
    {
        Material indicatorMaterial = currRangeIndicator.GetComponent<Renderer>().material;

        if(!IsClickInBounds(x, z))
        {
            indicatorMaterial.SetColor("_Color", Color.red);
            return;
        }

        indicatorMaterial.SetColor("_Color", Color.green); //new Color(1.0f, 0.6f, 1.0f, 0.6f)
    }
}
