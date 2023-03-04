using System;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections.LowLevel.Unsafe;
using System.Security.Cryptography;

public class Pathfinding : MonoBehaviour
{
    PathRequestManager _requestManager;
    [SerializeField] private Grid _grid;
    private void Awake()
    {
        _requestManager = GetComponent<PathRequestManager>();
        _grid = FindObjectOfType<Grid>();
    }

    public IEnumerator FindPath(Vector3 startPosition, Vector3 targetPosition)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        stopwatch.Start();

        Vector3[] waypoints = new Vector3[0];
        bool pathSucces = false;

        Node startNode = _grid.NodeFromWorldPoint(startPosition);
        Node targetNode = _grid.NodeFromWorldPoint(targetPosition);

        //Heap<Node> openSet = new Heap<Node>(_grid.MaxSize());
        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {

            //Node currentNode = openSet.RemoveFirst();

            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].GetFCost() < currentNode.GetFCost() || openSet[i].GetFCost() == currentNode.GetFCost())
                {
                    if (openSet[i].GetHCost() < currentNode.GetHCost())
                        currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);

            closedSet.Add(currentNode);
            if (currentNode == targetNode)
            {
                stopwatch.Stop();
                //print("path found: "+stopwatch.ElapsedMilliseconds+" ms");
                pathSucces = true;
                break;
            }
            foreach (Node neighbour in _grid.GetNeighbours(currentNode))
            {

                if (!neighbour.isWalkable() || closedSet.Contains(neighbour))
                    continue;

                int newMovementCost = currentNode.GetGCost() + GetDistance(currentNode, neighbour);
                if (newMovementCost < neighbour.GetGCost() || !openSet.Contains(neighbour))
                {
                    neighbour.SetGCost(newMovementCost);
                    neighbour.SetHCost(GetDistance(neighbour, targetNode));
                    neighbour.SetParent(currentNode);

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                }
            }

        }
        yield return null;
        if (pathSucces)
        {
            waypoints = RetracePath(startNode, targetNode);
        }
        _requestManager.FinishedProcessingPath(waypoints, pathSucces);
    }

    private Vector3[] RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.GetParent();
        }
        //Vector3[] waypoints = SimplyfyPath(path);
        Vector3[] waypoints = ListToArray(path);
        Array.Reverse(waypoints);
        return waypoints;
    }

    private Vector3[] SimplyfyPath(List<Node> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 oldDirection = Vector2.zero;

        for (int i = 1; i < path.Count; i++)
        {
            Vector2 newDirection = new Vector2(path[i - 1].GetGridPosition('x') - path[i].GetGridPosition('x'), path[i - 1].GetGridPosition('y') - path[i].GetGridPosition('y'));
            if (newDirection != oldDirection)
            {
                waypoints.Add(path[i].GetWorldPosion());
            }
            oldDirection = newDirection;
        }
        return waypoints.ToArray();
    }

    private Vector3[] ListToArray(List<Node> path)
    {
        List<Vector3> waypoints = new List<Vector3>();

        for (int i = 1; i < path.Count; i++)
        {
            waypoints.Add(path[i].GetWorldPosion());
        }

        return waypoints.ToArray();
    }

    private int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.GetGridPosition('x') - nodeB.GetGridPosition('x'));
        int dstY = Mathf.Abs(nodeA.GetGridPosition('y') - nodeB.GetGridPosition('y'));
        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }

    public void StartFindPath(Vector3 startPos, Vector3 endPos)
    {
        StartCoroutine(FindPath(startPos, endPos));
    }
}

public class Heap<T> where T : IHeapItem<T>
{
    T[] _items;
    private int _currentItemCount;

    public Heap(int maxHeapSize)
    {
        _items = new T[maxHeapSize];
    }
    public void Add(T item)
    {
        item._heapIndex = _currentItemCount;
        _items[_currentItemCount] = item;
        SortUp(item);
        _currentItemCount++;
    }

    public T RemoveFirst()
    {
        T firstItem = _items[0];
        _currentItemCount--;
        _items[0] = _items[_currentItemCount];
        _items[0]._heapIndex = 0;
        SortDown(_items[0]);
        return firstItem;
    }

    public void UpdateItem(T item)
    {
        SortUp(item);
    }

    public int Count
    {
        get { return _currentItemCount; }
    }

    public bool Contains(T item)
    {
        return Equals(_items[item._heapIndex], item);
    }

    void SortDown(T item)
    {
        while (true)
        {
            int childIndexLeft = item._heapIndex * 2 + 1;
            int childIndexRight = item._heapIndex * 2 + 2;
            int swapIndex = 0;

            if (childIndexLeft < _currentItemCount)
            {
                swapIndex = childIndexLeft;
                if (childIndexRight < _currentItemCount)
                {
                    if (_items[childIndexLeft].CompareTo(_items[childIndexRight]) < 0)
                    {
                        swapIndex = childIndexRight;
                    }
                }

                if (item.CompareTo(_items[swapIndex]) < 0)
                {
                    Swap(item, _items[swapIndex]);
                }
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }
    }

    void SortUp(T item)
    {
        int parentIndex = (item._heapIndex - 1) / 2;

        while (true)
        {
            T parentItem = _items[parentIndex];
            if (item.CompareTo(parentItem) > 0)
            {
                Swap(item, parentItem);
            }
            else
            {
                break;
            }

            parentIndex = (item._heapIndex - 1) / 2;
        }
    }

    void Swap(T itemA, T itemB)
    {
        _items[itemA._heapIndex] = itemB;
        _items[itemB._heapIndex] = itemA;
        int itemAIndex = itemA._heapIndex;
        itemA._heapIndex = itemB._heapIndex;
        itemB._heapIndex = itemAIndex;

    }
}

public interface IHeapItem<T> : IComparable<T>
{
    int _heapIndex { get; set; }
}