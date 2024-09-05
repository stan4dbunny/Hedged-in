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
    public List<MazePiece> connectedPieces;

    //helper methods
    public void VisitPiece()
    {
        visited = true;
    }

    public void AddConnectedPiece(MazePiece neighbour)
    {
        connectedPieces.Add(neighbour);
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
