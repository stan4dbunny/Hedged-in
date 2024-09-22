using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.XR.CoreUtils;
using UnityEngine;
using Unity.AI.Navigation;

public class GenerateMaze : MonoBehaviour
{
    [SerializeField] public int mazeWidth = 10;
    [SerializeField] public int mazeHeight = 10;
    [SerializeField] private MazePiece _mazePiece;
    public MazePiece[,] _maze; 
    [SerializeField] private GameObject _collectible;
    [SerializeField] private GameObject _door;
    [SerializeField] private GameObject _endPoint;
    [SerializeField] public int collectibleCount = 5;

    [SerializeField] public int movableDoorsCount = 10;
    public List<MazePiece> longestPath;
    public Material moveableWallMaterial;

    

    public bool IsOuterWall(int x, int z, int mazeWidth, int mazeHeight)
    {
        return x == 0 || z == 0 || x == mazeWidth - 1 || z == mazeHeight - 1;
    }

    void Awake()
    {
        var currLongestPath = new List<MazePiece>();
        
        InstantiateMazePieces();
        Generate(null, _maze[0, 0]);
        SetMazePieceAttributes();
        GetLongestPath(_maze[0, 0], currLongestPath);
        GenerateStartAndEndPoint();
        GenerateCollectibles();
        //GenerateObstacles();
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
                _maze[x, z] = Instantiate(_mazePiece, new Vector3(x, 0, z), Quaternion.identity);
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
            //PrintConnectedPieces(previous);
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

    private MazePiece GetNextUnvisited(MazePiece current)
    {
        var unvisited = GetUnvisited(current);

        return unvisited.OrderBy(_maze => Random.Range(1, 10)).FirstOrDefault();
    }

    private IEnumerable<MazePiece> GetUnvisited(MazePiece current)
    {
        int x = (int)current.transform.position.x;
        int z = (int)current.transform.position.z;

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

    private void GenerateStartAndEndPoint()
    {
        Vector3 Endposition = longestPath[longestPath.Count - 1].transform.position;
        longestPath[longestPath.Count - 1].isEndpoint = true;
        Instantiate(_endPoint, Endposition, Quaternion.identity);
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
                Instantiate(_collectible, new Vector3(randX, 0.2f, randZ), Quaternion.identity);
                currCollectibles++;
            }
        } while (currCollectibles < collectibleCount);
    }

    private void GenerateObstacles()
    {
        int lenPath = longestPath.Count;
        int spawnObstacleTries = 10;

        for(int i = 0; i < spawnObstacleTries; i++)
        {
            int randIndex = Random.Range(10, lenPath - 1);
            Vector3 obstaclePos = longestPath[randIndex].transform.position;
            int x = (int)obstaclePos.x;
            int z = (int)obstaclePos.z;

            if(_maze[x,z].suitableForObstacle == true && _maze[x,z].isOccupied == false)
            {
                _maze[x,z].isOccupied = true;
                obstaclePos.y += 0.5f;
                Vector3 doorRotation = CalculateDoorRotationAndCellPosition(longestPath[randIndex]);
                GameObject door = Instantiate(_door, obstaclePos, Quaternion.identity);
                door.transform.eulerAngles = doorRotation;
                break;
            }
        }
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

    private void PrintnextPieces(MazePiece mazePiece) //function for printing stuff for debug purposes
    {
        Debug.Log("Connected for : " + mazePiece.transform.position.ToString());
        foreach(var next in mazePiece.nextPieces)
        {
            Debug.Log(next.transform.position.ToString());
        }
    }

    private void PrintPathList(List<MazePiece> pathList) //function for printing stuff for debug purposes
    {
        foreach(var piece in pathList)
        {
            Debug.Log(piece.transform.position.ToString());
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
            //GameObject wallPlayer = Instantiate(wallData.Item2, wallData.Item2.transform.position, wallData.Item2.transform.rotation);
            string direction = wallData.Item3;
            wallOperator.layer = LayerMask.NameToLayer("NotVR");
            wallOperator.GetComponentInChildren<MeshRenderer>().gameObject.layer = LayerMask.NameToLayer("NotVR");
            //wallPlayer.layer = LayerMask.NameToLayer("OnlyVR");
            //wallPlayer.SetLayerRecursively(LayerMask.NameToLayer("OnlyVR"));
            wallOperator.GetComponentInChildren<Renderer>().material = moveableWallMaterial;
            wallOperator.AddComponent<WallController>();
            wallOperator.GetComponent<Collider>().layerOverridePriority = 1;
            Debug.Log(wallOperator.GetComponent<Collider>().layerOverridePriority);

            ClearNeighboringWall(currentCell, direction);
        }
       
    }

    private void ClearNeighboringWall(MazePiece currentCell, string direction)
    {
        Vector3 currentPos = currentCell.transform.position;
        int x = (int)currentPos.x;
        int z = (int)currentPos.z;

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