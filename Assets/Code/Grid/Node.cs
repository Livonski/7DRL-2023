using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : IHeapItem<Node>
{
    private bool _walkable;
    private Vector3 _worldPosition;
    private Vector2 _gridPosition;

    private int _gCost;
    private int _hCost;
    private int _fCost;
    int _index;

    private Node _parent;

    public int _heapIndex
    {
        get
        {
            return _index;
        }
        set
        {
            _index = value;
        }
    }

    public int CompareTo(Node nodeToCompare)
    {
        int compare = _fCost.CompareTo(nodeToCompare.GetFCost());
        if (compare == 0)
        {
            compare = _hCost.CompareTo(nodeToCompare.GetHCost());
        }
        return -compare;
    }
    public Node(bool walkable, Vector3 worldPosition, Vector2 positionInGrid)
    {
        _walkable = walkable;
        _worldPosition = worldPosition;
        _gridPosition = positionInGrid;
    }

    public int GetGridPosition(char select)
    {
        if (select == 'x')
        {
            return Mathf.FloorToInt(_gridPosition.x);
        }
        return Mathf.FloorToInt(_gridPosition.y);
    }

    public Vector3 GetWorldPosion()
    {
        return _worldPosition;
    }
    public bool isWalkable()
    {
        return _walkable;
    }
    public int GetFCost()
    {
        return _gCost + _hCost;
    }
    public int GetHCost()
    {
        return _hCost;
    }
    public int GetGCost()
    {
        return _gCost;
    }
    public void SetGCost(int newCost)
    {
        _gCost = newCost;
    }
    public void SetHCost(int newCost)
    {
        _hCost = newCost;
    }
    public void SetParent(Node newParent)
    {
        _parent = newParent;
    }
    public Node GetParent()
    {
        return _parent;
    }
} 