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
}
