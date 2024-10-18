using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.XR.CoreUtils;
using UnityEngine;
using Unity.AI.Navigation;
using Unity.VisualScripting;

public class GenerateMaze : MonoBehaviour
{
    [SerializeField] public int mazeWidth = 10;
    [SerializeField] public int mazeHeight = 10;
    private MazePiece _mazePiece;
    public float scaleFactor;
    public float centerObjInCellVal;
    [SerializeField] private List<MazePiece> _mazePieces;
    public enum MazeScale
    {
        Scale1X,
        Scale1_5X,
        Scale2X,
    }
    [SerializeField] private MazeScale mazeScale = new MazeScale();
    private MazePiece[,] _maze; 
    [SerializeField] private GameObject _collectible;
    [SerializeField] private GameObject _endPoint;
    [SerializeField] private GameObject _monster;
    private Vector2 monsterSpawnCell = new Vector2(5, 5);
    [SerializeField] public int collectibleCount = 5;

    [SerializeField] public int movableDoorsCount = 10;
    public List<MazePiece> longestPath;
    public Material moveableWallMaterial;

    public Mesh hedgeNegXMissing;
    public Mesh hedgePosXMissing;
    public Mesh hedgeBothMissing;
    public Mesh hedge;
    
    public MazePiece GetMazePieceAtPosition(int x, int z)
    {
        return _maze[x, z];
    }


    public bool IsOuterWall(int x, int z, int mazeWidth, int mazeHeight)
    {
        return x == 0 || z == 0 || x == mazeWidth - 1 || z == mazeHeight - 1;
    }

    void Awake()
    {
        switch(mazeScale)
        {
            case MazeScale.Scale1X:
                _mazePiece = _mazePieces[0];
                scaleFactor = 1.0f;
                break;
            case MazeScale.Scale1_5X:
                _mazePiece = _mazePieces[1];
                scaleFactor = 1.5f;
                centerObjInCellVal = 0.25f;
                break;
            case MazeScale.Scale2X:
                _mazePiece = _mazePieces[2];
                scaleFactor = 2.0f;
                centerObjInCellVal = 0.5f;
                break;
        }
        _mazePiece.SetWallScales(scaleFactor);
        _mazePiece.SetLocalPositions(scaleFactor);
        var currLongestPath = new List<MazePiece>();
        
        InstantiateMazePieces();
        Generate(null, _maze[0, 0]);
        ChangeModels();
        SetMazePieceAttributes();
        GetLongestPath(_maze[0, 0], currLongestPath);
        GenerateStartAndEndPoint();
        GenerateMonster();
        GenerateCollectibles();
        RemoveDuplicateWalls();
        ColorRandomWalls();

        UpdateNavMesh();
        
    }
    private void InstantiateMazePieces()
    {
        _maze = new MazePiece[mazeWidth, mazeHeight];

        for(int x = 0; x < mazeWidth; x++)
        {
            for (int z = 0; z < mazeHeight; z++)
            {
                _maze[x, z] = Instantiate(_mazePiece, new Vector3(x * scaleFactor, 0, z * scaleFactor), Quaternion.identity);
                _maze[x, z].cellPos = new Vector2(x, z);
            }
        }
    }

    private void Generate(MazePiece previous, MazePiece current)
    {
        current.VisitPiece();
        ClearWall(previous, current);

        if(previous != null)
        {
            previous.AddNextPiece(current);
        }
        
        current.AddPreviousPiece(previous);

        MazePiece next;
        do
        {
            next = GetNextUnvisited(current);

            if(next != null)
            {
                Generate(current, next);
            }
        } while (next != null);
    }

    public MazePiece[] GetAdjacents(MazePiece piece)
    {
        int x = (int)piece.cellPos.x;
        int z = (int)piece.cellPos.y;

        MazePiece[] adjacents = new MazePiece[4];
        if (x + 1 < mazeWidth)
        {
            var cellToRight = _maze[x + 1, z];
            adjacents[0] = cellToRight;
        }

        if (x - 1 >= 0)
        {
            var cellToLeft = _maze[x - 1, z];
            adjacents[1] = cellToLeft;
        }

        if (z + 1 < mazeHeight)
        {
            var cellInfront = _maze[x, z + 1];

            adjacents[2] = cellInfront;
        }

        if (z - 1 >= 0)
        {
            var cellBehind = _maze[x, z - 1];
            adjacents[3] = cellBehind;
        }
        return adjacents;
    }
    private MazePiece GetNextUnvisited(MazePiece current)
    {
        var unvisited = GetUnvisited(current);

        return unvisited.OrderBy(_maze => Random.Range(1, 10)).FirstOrDefault();
    }

    private IEnumerable<MazePiece> GetUnvisited(MazePiece current)
    {
        int x = (int)current.cellPos.x;
        int z = (int)current.cellPos.y;

        if(x + 1 < mazeWidth)
        {
            var cellToRight = _maze[x + 1, z];

            if (cellToRight.visited == false)
            {
                yield return cellToRight;
            }
        }

        if(x - 1 >= 0)
        {
            var cellToLeft = _maze[x - 1, z];

            if (cellToLeft.visited == false)
            {
                yield return cellToLeft;
            }
        }

        if(z + 1 < mazeHeight)
        {
            var cellInfront = _maze[x, z + 1];

            if (cellInfront.visited == false)
            {
                yield return cellInfront;
            }
        }

        if(z - 1 >= 0)
        {
            var cellBehind = _maze[x, z - 1];

            if (cellBehind.visited == false)
            {
                yield return cellBehind;
            }
        }
    }

    private void ClearWall(MazePiece previous, MazePiece current)
    {
        if (previous == null)
        {
            return;
        }

        Vector3 previousPosition = previous.transform.position;
        Vector3 currentPosition = current.transform.position;

        if(previousPosition.x < currentPosition.x)
        {
            previous.ClearEastWall();
            current.ClearWestWall();
            return;
        }

        if(previousPosition.x > currentPosition.x)
        {
            previous.ClearWestWall();
            current.ClearEastWall();
            return;
        }

        if(previousPosition.z < currentPosition.z)
        {
            previous.ClearNorthWall();
            current.ClearSouthWall();
            return;
        }

        if(previousPosition.z > currentPosition.z)
        {
            previous.ClearSouthWall();
            current.ClearNorthWall();
            return;
        }

    }

    private void SetMazePieceAttributes()
    {
        for(int x = 0; x < mazeWidth; x++)
        {
            for (int z = 0; z < mazeHeight; z++)
            {
                if((_maze[x,z].CheckWestWallActive() && _maze[x,z].CheckEastWallActive()) || (_maze[x,z].CheckNorthWallActive() && _maze[x,z].CheckSouthWallActive()))
                {
                    _maze[x,z].suitableForObstacle = true;
                }
            }
        }
    }

    private void RemoveDuplicateWalls()
    {
        for(int x = 0; x < mazeWidth; x++)
        {
            for (int z = 0; z < mazeHeight; z++)
            {
                MazePiece currCell = _maze[x,z];
                if(x + 1 < mazeWidth) //Cell to the right/east
                {
                    MazePiece eastCell = _maze[x + 1, z];
                    if(currCell.CheckEastWallActive() && eastCell.CheckWestWallActive())
                        {
                            eastCell.ClearWestWall();
                        }     
                } 
                if(x - 1 >= 0) //Cell to the left/west
                {
                    MazePiece westCell = _maze[x - 1, z];
                    if(currCell.CheckWestWallActive() && westCell.CheckEastWallActive())
                    {
                        westCell.ClearEastWall();
                    }
                } 
                if(z + 1 < mazeHeight) //Cell above/north
                {
                    MazePiece northCell = _maze[x, z + 1];
                    if(currCell.CheckNorthWallActive() && northCell.CheckSouthWallActive())
                    {
                        northCell.ClearSouthWall();
                    } 
                } 
                if(z - 1 >= 0) //Cell below/south
                {
                    MazePiece southCell = _maze[x, z - 1];
                    if(currCell.CheckSouthWallActive() && southCell.CheckNorthWallActive())
                    {
                        southCell.ClearNorthWall();
                    }
                } 
            }
        }
    }

    /*
    Set correct models for the hedges
    */
    private void ChangeModels()
    {
        for (int x = 0; x < mazeWidth; x++)
        {
            for (int z = 0; z < mazeHeight; z++)
            {
                MazePiece curr = _maze[x, z];
                bool eastWall = curr.CheckEastWallActive();
                bool westWall = curr.CheckWestWallActive();
                bool northWall = curr.CheckNorthWallActive();
                bool southWall = curr.CheckSouthWallActive();

                MazePiece[] adjacents = GetAdjacents(curr);

                MazePiece cellToRight = adjacents[0];
                MazePiece cellToLeft = adjacents[1];
                MazePiece cellInFront = adjacents[2];
                MazePiece cellBehind = adjacents[3];

                
                if (eastWall)
                {
                    bool xposE = false;
                    bool xnegE = false;

                    if (cellBehind != null && cellBehind.CheckEastWallActive())
                        xnegE = true;

                    if (cellInFront != null && cellInFront.CheckEastWallActive())
                        xposE = true;

                    if (xposE && !xnegE) curr.GetEastWall().GetComponentInChildren<MeshFilter>().sharedMesh = hedgePosXMissing;
                    if (xnegE && !xposE) curr.GetEastWall().GetComponentInChildren<MeshFilter>().sharedMesh = hedgeNegXMissing;
                    if (xnegE && xposE) curr.GetEastWall().GetComponentInChildren<MeshFilter>().sharedMesh = hedgeBothMissing;
                }

                if (westWall)
                {
                    bool xposW = false;
                    bool xnegW = false;

                    if (cellBehind != null && cellBehind.CheckWestWallActive())
                        xnegW = true;

                    if (cellInFront != null && cellInFront.CheckWestWallActive())
                        xposW = true;

                    if (xposW && !xnegW) curr.GetWestWall().GetComponentInChildren<MeshFilter>().sharedMesh = hedgePosXMissing;
                    if (xnegW && !xposW) curr.GetWestWall().GetComponentInChildren<MeshFilter>().sharedMesh = hedgeNegXMissing;
                    if (xnegW && xposW) curr.GetWestWall().GetComponentInChildren<MeshFilter>().sharedMesh = hedgeBothMissing;
                }

                if(northWall)
                {
                    bool xposN = false;
                    bool xnegN = false;
                    if (cellToLeft != null && cellToLeft.CheckNorthWallActive())
                        xnegN = true;

                    if (cellToRight != null && cellToRight.CheckNorthWallActive())
                        xposN = true;

                    if (xposN && !xnegN) curr.GetNorthWall().GetComponentInChildren<MeshFilter>().sharedMesh = hedgePosXMissing;
                    if (xnegN && !xposN) curr.GetNorthWall().GetComponentInChildren<MeshFilter>().sharedMesh = hedgeNegXMissing;
                    if (xnegN && xposN) curr.GetNorthWall().GetComponentInChildren<MeshFilter>().sharedMesh = hedgeBothMissing;
                }

                if (southWall)
                {
                    bool xposS = false;
                    bool xnegS = false;
                    if (cellToLeft != null && cellToLeft.CheckSouthWallActive())
                        xnegS = true;

                    if (cellToRight != null && cellToRight.CheckSouthWallActive())
                        xposS = true;

                    if (xposS && !xnegS) curr.GetSouthWall().GetComponentInChildren<MeshFilter>().sharedMesh = hedgePosXMissing;
                    if (xnegS && !xposS) curr.GetSouthWall().GetComponentInChildren<MeshFilter>().sharedMesh = hedgeNegXMissing;
                    if (xnegS && xposS) curr.GetSouthWall().GetComponentInChildren<MeshFilter>().sharedMesh = hedgeBothMissing;
                }
            }
        }
        
    }
    private void GenerateStartAndEndPoint()
    {
        Vector2 Endposition = longestPath[longestPath.Count - 1].cellPos;
        MazePiece currCell = longestPath[longestPath.Count - 1];
        longestPath[longestPath.Count - 1].isEndpoint = true;
        GameObject endObj = Instantiate(_endPoint, new Vector3(Endposition.x * scaleFactor + centerObjInCellVal, 0, Endposition.y * scaleFactor + centerObjInCellVal), Quaternion.identity);
        endObj.transform.localScale = new Vector3(0.05f, 0.1f, 0.05f);
        //north is default, 90 is east, 180 is south, 270 is west
        if(!currCell.CheckEastWallActive())
        {
            endObj.transform.localEulerAngles = new Vector3(0, 90f, 0);
        }
        else if (!currCell.CheckSouthWallActive())
        {
            endObj.transform.localEulerAngles = new Vector3(0, 180f, 0);
        }
        else if (!currCell.CheckWestWallActive())
        {
            endObj.transform.localEulerAngles = new Vector3(0, -90f, 0);
        }
    }

    private void GenerateMonster()
    {
        Instantiate(_monster, new Vector3(monsterSpawnCell.x * scaleFactor, 0, monsterSpawnCell.y * scaleFactor), Quaternion.identity);
    }

     private void GenerateCollectibles()
    {
        int currCollectibles = 0;
        do
        {
            int randX = Random.Range(0, mazeWidth - 1);
            int randZ = Random.Range(0, mazeHeight - 1);

            if(_maze[randX,randZ].isOccupied == false && (randX != 0 && randZ != 0))
            {
                _maze[randX,randZ].isOccupied = true;
                Instantiate(_collectible, new Vector3(randX * scaleFactor + centerObjInCellVal, 0.2f, randZ * scaleFactor + centerObjInCellVal), Quaternion.identity);
                currCollectibles++;
            }
        } while (currCollectibles < collectibleCount);
    }

    private Vector3 CalculateDoorRotationAndCellPosition(MazePiece mazePiece)
    {
        Vector3 rot = new Vector3(0, 0, 0);
        if(mazePiece.CheckWestWallActive() && mazePiece.CheckEastWallActive())
        {
            rot.y += 90;
        }
        return rot;
    }

    private void GetLongestPath(MazePiece mazePiece, List<MazePiece> currentLongestPathList)
    {
        currentLongestPathList.Add(mazePiece);

        //we are at an endpoint in the maze
        if(mazePiece.nextPieces.Count == 0) //dead end
        {
            if(currentLongestPathList.Count > longestPath.Count)
            {
                longestPath.Clear();
                longestPath.AddRange(currentLongestPathList);
            }
            RemovePiecesUntilFork(currentLongestPathList);
        }
        else
        {
            foreach(var next in mazePiece.nextPieces)
            {
                GetLongestPath(next, currentLongestPathList);
            }
        }

    }

    private void RemovePiecesUntilFork(List<MazePiece> currentLongestPathList)
    {
        for(int i = currentLongestPathList.Count - 1; i >= 0; i--)
        {
            if(currentLongestPathList[i].nextPieces.Count != 2 ) //if piece is a corridor, remove it
            {
                currentLongestPathList.RemoveAt(i);
            }
            else if(currentLongestPathList[i].nextPieces.Count == 2 && currentLongestPathList[i].forkHasBeenBacktracked == true) //if piece is a fork that has been backtracked before, remove it
            {
                currentLongestPathList.RemoveAt(i);
            }
            else //if piece is a fork where we have not traversed both paths, stop removing
            {
                currentLongestPathList[i].forkHasBeenBacktracked = true;
                break;
            }
        }
    }

    private List<MazePiece> GetInternalCells()
    {
        List<MazePiece> internalCells = new List<MazePiece>();

        for (int x = 1; x < mazeWidth - 1; x++) // Skip outer walls
        {
            for (int z = 1; z < mazeHeight - 1; z++)
            {
                internalCells.Add(_maze[x,z]);
            }
        }
        
        return internalCells;
    }

    private void ColorRandomWalls()
    {

        List<MazePiece> internalCells = GetInternalCells();
        List<(MazePiece, GameObject, string)> selectableWalls = new List<(MazePiece, GameObject, string)>();

        foreach (var cell in internalCells)
        {
            if (cell.CheckNorthWallActive())
                selectableWalls.Add((cell, cell.GetNorthWall(), "North"));
            if (cell.CheckSouthWallActive())
                selectableWalls.Add((cell, cell.GetSouthWall(), "South"));
            if (cell.CheckEastWallActive())
                selectableWalls.Add((cell, cell.GetEastWall(), "East"));
            if (cell.CheckWestWallActive())
                selectableWalls.Add((cell, cell.GetWestWall(), "West"));
        }

        // Shuffle the list and randomly select walls
        var randomWalls = selectableWalls.OrderBy(_ => Random.value).Take(movableDoorsCount).ToList();

        foreach (var wallData in randomWalls) //can generate walls in the same spot, causes issues
        {
            MazePiece currentCell = wallData.Item1;
            GameObject wallOperator = wallData.Item2;
            string direction = wallData.Item3;
            wallOperator.GetComponentInChildren<Renderer>().material = moveableWallMaterial;
            wallOperator.AddComponent<WallController>();
            wallOperator.GetComponent<Collider>().layerOverridePriority = 1;
            wallOperator.GetComponent<BoxCollider>().size = new Vector3(0.3f, 0.2f, 1.0f); //Stupid solutions for stupid problems
            wallOperator.transform.localScale = new Vector3(1.0f, 1.001f, 1.0f * scaleFactor); //Stupid solutions for stupid problems

            ClearNeighboringWall(currentCell, direction);
        }
       
    }
    
    private void ClearNeighboringWall(MazePiece currentCell, string direction)
    {
        int x = (int)currentCell.cellPos.x;
        int z = (int)currentCell.cellPos.y;

        switch (direction)
        {
            case "North":
                if (z + 1 < mazeHeight)
                    _maze[x, z + 1].ClearSouthWall();
                break;
            case "South":
                if (z - 1 >= 0)
                    _maze[x, z - 1].ClearNorthWall();
                break;
            case "East":
                if (x + 1 < mazeWidth)
                    _maze[x + 1, z].ClearWestWall();
                break;
            case "West":
                if (x - 1 >= 0)
                    _maze[x - 1, z].ClearEastWall();
                break;
        }
    }
    //Used for setting initial rotation or VR-player, so they dont look directly into a wall
    public float GetInitialRotationOfPlayerFromStartBlock()
    {
        MazePiece startPiece = _maze[0, 0];

        if(startPiece.CheckNorthWallActive() && startPiece.CheckSouthWallActive())
        {
            return 90;
        }

        return 0;
    }

    public void UpdateNavMesh()
    {
        GetComponent<NavMeshSurface>().BuildNavMesh();
    }
}