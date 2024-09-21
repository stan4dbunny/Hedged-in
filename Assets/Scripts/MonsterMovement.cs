using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterMovement : MonoBehaviour
{
    public GameObject distraction; //object operator can use to distract the monster
    [SerializeField] public GameObject mazeInfo; //object that generates the maze
    [SerializeField] public GameObject player; //player object
    private GenerateMaze mazeGenerator; //used for accessing actual maze
    private NavMeshAgent navMeshAgent; //navigation mesh object https://docs.unity3d.com/ScriptReference/AI.NavMeshAgent.html
    private int fov = 90; //monster field-of-view
    private Vector3 currentDestination; //current world position monster is trying to path towards
    private bool hasDestination = false;
    private bool seesPlayer;

    private Animator animator; 


    void Start()
    {
        mazeGenerator = mazeInfo.GetComponent<GenerateMaze>();
        navMeshAgent = GetComponent<NavMeshAgent>(); //TODO: need to update the navMesh when a player clicks a wall, so that the monster understands that the maze has changed 
        animator = GetComponent<Animator>(); 
    }
    void FixedUpdate()
    {
        //Debug.Log(currentDestination);
        if(GameObject.FindWithTag("distraction")) //Currently only handles one active distraction-object. If wewant to handle more: https://docs.unity3d.com/ScriptReference/GameObject.FindGameObjectsWithTag.html
        {
            distraction = GameObject.FindWithTag("distraction");
            if(isInRange(distraction)) //prioritize if a distraction is close to the monster
            {
                pathTo(distraction);
                return;
            }
        }
        
        seesPlayer = lookForPlayer(); //if there is no distraction, look for the player

        if(seesPlayer) //if we find the player, chase it
        {
            pathTo(player);
            animator.SetBool("PlayerIsVisible", true);
            navMeshAgent.speed = 1.2f; 
            return;
        }

        wander(); //if not any of the above is true, randomly wander around the maze
    }

    private bool isInRange(GameObject target) //Check if certain gameObject in in range of the monster
    {
        if(Vector3.Distance(target.transform.position, transform.position) < 3f)
        {
            return true;
        }

        return false;
    }

    private void pathTo(GameObject target) //moves monster towards a certain position
    {
        currentDestination = target.transform.position;
        navMeshAgent.destination = currentDestination;
        hasDestination = true;

        if(Vector3.Distance(transform.position, currentDestination) < 0.3f) //if we're close to our destination, we consider it as having reached it, enables new destination to be found.
        {
            hasDestination = false;
        }
    }

    private bool lookForPlayer() //looks to see if player is in the monsters field of view
    {
        Vector3 vecBetweenMonsterAndPlayer = (player.transform.position + new Vector3(0, 0.2f, 0)) - transform.position; //vector between monster and player
        float vecAngle = Vector3.Angle(vecBetweenMonsterAndPlayer, transform.forward); //angle of vector between monster and player, and the forward vector of the object

        if(vecAngle < fov) //if in the monsters field of view
        {
            /*this sometimes seesplayer through walls???*/
            if(Physics.Raycast(transform.position, vecBetweenMonsterAndPlayer, out RaycastHit hit)) //shoot a ray towards the player
            {
                if(hit.collider.gameObject.tag == "Player") //if the ray collides with the player, return true. We have found the player
                {
                    return true; //maybe change monster speed when it sees player?
                }
            }
        }

        return false;
    }

    private void wander() //if monster is not in range of distraction, or sees the player, wander around the maze
    {
        animator.SetBool("PlayerIsVisible", false);
        navMeshAgent.speed = 0.6f;

        if (!hasDestination)
        {
            int xPos = Mathf.RoundToInt(transform.position.x);
            int zPos = Mathf.RoundToInt(transform.position.z);
            MazePiece thisPiece = mazeGenerator._maze[xPos, zPos]; //Get the mazeCell in which the monster is

            List<MazePiece> adjacentPieces = new List<MazePiece>();
            //TODO: have possibility for monster to set a wander destination further than one piece
            foreach(MazePiece piece in thisPiece.nextPieces)
            {
                if(!piece.isEndpoint){ adjacentPieces.Add(piece);}
            }
            foreach(MazePiece piece in thisPiece.previousPieces)
            {
                if(!piece.isEndpoint){ adjacentPieces.Add(piece);}
            }

            var index = Random.Range(0, adjacentPieces.Count); 
            MazePiece randomPiece = adjacentPieces[index]; //randomly pick adjacent meze cell to navigate to
            currentDestination = randomPiece.transform.position;
            hasDestination = true;
        }

        navMeshAgent.destination = currentDestination;

        if(Vector3.Distance(transform.position, currentDestination) < 0.3f) //if we're clsoe to our destination, we consider it as having reached it, enables new destination to be found.
        {
            hasDestination = false;
        }
    }

    private void OnCollisionEnter(Collision collision) 
    {
        if(collision.gameObject.tag == "distraction")
        {
            Destroy(collision.gameObject);
        }
    }
}
