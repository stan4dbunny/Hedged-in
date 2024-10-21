using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazePiece : MonoBehaviour
{
    [SerializeField] private GameObject _westWall;
    [SerializeField] private GameObject _eastWall;
    [SerializeField] private GameObject _northWall;
    [SerializeField] private GameObject _southWall;

    public bool visited = false;
    public bool forkHasBeenBacktracked = false;
    public bool suitableForObstacle = false;
    public bool isOccupied = false;
    public bool isEndpoint = false;
    public List<MazePiece> nextPieces;
    public List<MazePiece> previousPieces;
    public Vector2 cellPos;

    // Getter methods to access the private walls
    public GameObject GetNorthWall() { return _northWall; }
    public GameObject GetSouthWall() { return _southWall; }
    public GameObject GetEastWall() { return _eastWall; }
    public GameObject GetWestWall() { return _westWall; }

    //helper methods
    public void VisitPiece()
    {
        visited = true;
    }

    public void AddNextPiece(MazePiece next)
    {
        nextPieces.Add(next);
    }

    public void AddPreviousPiece(MazePiece previous)
    {
        previousPieces.Add(previous);
    }

    public void ClearWestWall()
    {
        _westWall.SetActive(false);
    }

    public void ClearEastWall()
    {
        _eastWall.SetActive(false);
    }

    public void ClearNorthWall()
    {
        _northWall.SetActive(false);
    }

    public void ClearSouthWall()
    {
        _southWall.SetActive(false);
    }

    public bool CheckWestWallActive()
    {
        return _westWall.activeSelf;
    }

    public bool CheckEastWallActive()
    {
        return _eastWall.activeSelf;
    }

    public bool CheckNorthWallActive()
    {
        return _northWall.activeSelf;
    }

    public bool CheckSouthWallActive()
    {
        return _southWall.activeSelf;
    }
    public void SetWallScales(float scaleFactor)
    {
        _eastWall.transform.localScale = new Vector3(1, 1, scaleFactor);
        _northWall.transform.localScale = new Vector3(1, 1, scaleFactor);
        _southWall.transform.localScale = new Vector3(1, 1, scaleFactor);
        _westWall.transform.localScale = new Vector3(1, 1, scaleFactor);
    }

    public void SetLocalPositions(float scaleFactor)
    {
        switch(scaleFactor)
        {
            case 1:
                break;
            case 1.5f:
                _eastWall.transform.localPosition = new Vector3(1, 0, -0.5f);
                _northWall.transform.localPosition = new Vector3(-0.5f, 0, 1);
                break;
            case 2:
                _eastWall.transform.localPosition = new Vector3(1.5f, 0, -0.5f);
                _northWall.transform.localPosition = new Vector3(-0.5f, 0, 1.5f);
                break;
        }
    }
}
