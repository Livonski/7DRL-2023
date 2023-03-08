using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementSequencer : MonoBehaviour
{
    private List<Movable> movables;
    private Queue<moveRequest> movementQueue;

    private bool playerMoved;

    private void Awake()
    {
        movables = new List<Movable>();
        movementQueue = new Queue<moveRequest>();   
    }

    private void Update()
    {
        if (playerMoved)
        {
            //Debug.Log("movemet sequence contains " + movementQueue.Count);
            foreach(moveRequest mR in movementQueue)
            {

                //Debug.Log("moveRequest processed");
                //Debug.Log(mR.direction);
                StartCoroutine(mR.movable.Move(mR.direction));
            }
            movementQueue.Clear();
            playerMoved = false;
        }
    }
    public void addMovable(Movable movable)
    {
        movables.Add(movable);
    }

    public void enqueueMove(Vector3 direction, Movable movable)
    {
        moveRequest moveRequest = new moveRequest(direction, movable);
        if (!movementQueue.Contains(moveRequest))
        {
            //Debug.Log("Enqueuing move");
            movementQueue.Enqueue(moveRequest);
        }      
    }

    public void startMoving()
    {
        playerMoved = true;
    }
    struct moveRequest
    {
        public Vector3 direction;
        public Movable movable;

        public moveRequest(Vector3 direction, Movable movable)
        {
            this.direction = direction;
            this.movable = movable;
        }
    }
}
