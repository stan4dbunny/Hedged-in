using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallController : MonoBehaviour
{
    private bool isRotating = false;
    private bool rotateForward = true;
    private float rotationSpeed = 100f;
    private float rotationAngle = 90f;
    private Quaternion targetRotation;
    private GameObject mazeGenerator;
    private GenerateMaze mazeInfo;
    private AudioSource audioSource;
    public AudioClip doorClip;

    void Start()
    {
        mazeGenerator = GameObject.Find("MazeGenerator"); //important the the object generating the maze hsa this name (For now, maybe can get it some other way)
        mazeInfo = mazeGenerator.GetComponent<GenerateMaze>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void Update()
    {
        if (isRotating)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            PlayDoorSound();
            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.01f)
            {
                isRotating = false;
                mazeInfo.UpdateNavMesh();
            }
        }
    }

    void OnMouseDown()
    {
        if (!isRotating)
        {
            float direction = rotateForward ? rotationAngle : -rotationAngle;
            targetRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, direction, 0));
            rotateForward = !rotateForward;

            UpdateMoveableWallModel();
            isRotating = true;
        }
    }

    private void PlayDoorSound()
    {
        if (doorClip != null && !audioSource.isPlaying)
        {
            audioSource.pitch = 1.8f;
            audioSource.PlayOneShot(doorClip);
        }
    }   
   
    private void UpdateChildren(GameObject getWall)
    {
        MeshFilter[] meshes = getWall.GetComponentsInChildren<MeshFilter>();
        foreach (MeshFilter mesh in meshes)
        {
            mesh.sharedMesh = mazeInfo.hedge;
        }
    }
    private void UpdateMoveableWallModel()
    {
        int x = (int)GetComponentInParent<Transform>().position.x;
        int z = (int)GetComponentInParent<Transform>().position.z;
        MazePiece piece = GetComponentInParent<MazePiece>();

        MazePiece[] adjacents = mazeInfo.GetAdjacents(piece);
        MazePiece cellToRight = adjacents[0];
        MazePiece cellToLeft = adjacents[1];
        MazePiece cellInFront = adjacents[2];
        MazePiece cellBehind = adjacents[3];

        switch (gameObject.name)
        {
            case "North Wall":
                        if (z + 1 < mazeInfo.mazeHeight)
                        {
                            UpdateChildren(piece.GetNorthWall());
                            UpdateChildren(cellToLeft.GetNorthWall());
                            UpdateChildren(cellToRight.GetNorthWall());
                        }
                break;
            case "South Wall":
                        if (z - 1 >= 0)
                        {
                            UpdateChildren(piece.GetSouthWall());
                            UpdateChildren(cellToLeft.GetSouthWall());
                            UpdateChildren(cellToRight.GetSouthWall());
                        }
                break;
            case "East Wall":
                         if (x + 1 < mazeInfo.mazeWidth) { 
                            UpdateChildren(piece.GetEastWall());
                            UpdateChildren(cellBehind.GetEastWall());
                            UpdateChildren(cellInFront.GetEastWall());
                        }
                 break;
            case "West Wall":
                        if (x - 1 >= 0)
                        {
                            UpdateChildren(piece.GetWestWall());
                            UpdateChildren(cellBehind.GetWestWall());
                            UpdateChildren(cellInFront.GetWestWall());
                        }
                break;
        }
    }
}
