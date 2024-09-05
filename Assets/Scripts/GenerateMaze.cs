using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GenerateMaze : MonoBehaviour
{
    [SerializeField] public int mazeWidth = 10;
    [SerializeField] public int mazeHeight = 10;
    [SerializeField] private MazePiece _mazePiece;
    private MazePiece[,] _maze; 
    [SerializeField] private GameObject _collectible;
    [SerializeField] private GameObject _door;
    [SerializeField] private GameObject _endPoint;
    [SerializeField] private GameObject _startPoint;
    public int collectibleCount = 3;
    public List<MazePiece> longestPath;
    void Start()
    {
        var currLongestPath = new List<MazePiece>();
        
        InstantiateMazePieces();
        Generate(null, _maze[0, 0]);
        SetMazePieceAttributes();
        GetLongestPath(_maze[0, 0], currLongestPath);
        GenerateStartAndEndPoint();
        GenerateCollectibles();
        GenerateObstacles();
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
            previous.AddConnectedPiece(current);
            //PrintConnectedPieces(previous);
        }

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
        Instantiate(_endPoint, Endposition, Quaternion.identity);
        Instantiate(_startPoint, new Vector3(0, 0, 0), Quaternion.identity);
    }

    private void GenerateCollectibles() //add logic so collectibles wont spawn on eachother, on the door and directly at the start.
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

        for(int i = 0; i < spawnObstacleTries; i++) //try to spawn meaningful obstacle a set number of times, can have a bool that states if generation succeeded or not?
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

    private void GetLongestPath(MazePiece mazePiece, List<MazePiece> currentLongestPathList)//need logic for when we find a longer path on the maze
    {
        currentLongestPathList.Add(mazePiece);

        //we are at an endpoint in the maze
        if(mazePiece.connectedPieces.Count == 0) //dead end
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
            foreach(var connected in mazePiece.connectedPieces)
            {
                GetLongestPath(connected, currentLongestPathList);
            }
        }

    }

    //does not work if we have a fork that leads into another fork down the line.
    private void RemovePiecesUntilFork(List<MazePiece> currentLongestPathList)
    {
        for(int i = currentLongestPathList.Count - 1; i >= 0; i--)
        {
            if(currentLongestPathList[i].connectedPieces.Count != 2 ) //if piece is a corridor, remove it
            {
                currentLongestPathList.RemoveAt(i);
            }
            else if(currentLongestPathList[i].connectedPieces.Count == 2 && currentLongestPathList[i].forkHasBeenBacktracked == true) //if piece is a fork that has been backtracked before, remove it
            {
                currentLongestPathList.RemoveAt(i);
            }
            else //if piece is a fork where we have not traversed both paths, stop removing
            {
                currentLongestPathList[i].forkHasBeenBacktracked = true; //I think this works?
                break;
            }
        }
    }

    private void PrintConnectedPieces(MazePiece mazePiece) //function for printing stuff for debug purposes
    {
        Debug.Log("Connected for : " + mazePiece.transform.position.ToString());
        foreach(var connected in mazePiece.connectedPieces)
        {
            Debug.Log(connected.transform.position.ToString());
        }
    }

    private void PrintPathList(List<MazePiece> pathList) //function for printing stuff for debug purposes
    {
        foreach(var piece in pathList)
        {
            Debug.Log(piece.transform.position.ToString());
        }
    }
}
