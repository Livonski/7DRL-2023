using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class Grid : MonoBehaviour
{
    private GameObject[,] tiles;
    [SerializeField] private Vector2Int gridSize;
    [SerializeField] private Vector2Int offset;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject dummy;
    [SerializeField] private GameObject testObject;

    private Queue generatorQueue = new Queue();

    private worldGenerator worldGenerator;
    private Node[,] grid;
    private Vector3 worldBottomLeft;

    private bool unwalkablesUpdated = false;
    private void Start()
    {
        worldBottomLeft = transform.position - Vector3.right * gridSize.x / 2 - Vector3.forward * gridSize.y / 2;
        worldGenerator = GetComponent<worldGenerator>();

        tiles = worldGenerator.GenerateWorld(gridSize, worldBottomLeft, offset);
        CreateGrid();
        worldGenerator.GenerateCorridors();

        AddObject(worldGenerator.defineStartAndExit(), playerPrefab);
        GameObject player = GameObject.Find("Player(Clone)");
        Camera.main.transform.SetParent(player.transform);
        Camera.main.transform.position = player.transform.position + new Vector3(0, 15, -5);

    }
    private void Update()
    {
        if (!unwalkablesUpdated)
            StartCoroutine(updateUnwalkables());
    }

    private void CreateGrid()
    {
        grid = new Node[gridSize.x, gridSize.y];
        for (int y = 0; y <gridSize.y; y++)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                Vector3 worldPoint = worldBottomLeft + transform.position + new Vector3(x, 0, y);
                bool walkable = true;
                if (tiles[x, y] != null && tiles[x,y].tag == "Unwalkable")
                {
                    walkable = false;
                }

                grid[x, y] = new Node(walkable, worldPoint, new Vector2(x, y));
            }
        }
        //return _grid;
    }

    private IEnumerator updateUnwalkables()
    {
        for (int y = 0; y < gridSize.y; y++)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                bool walkable = true;
                if (tiles[x, y] != null && tiles[x, y].tag == "Unwalkable")
                {
                    walkable = false;
                }
                grid[x, y].setWalkable(walkable);
            }
        }
        yield return new WaitForSeconds(5f);
        unwalkablesUpdated = true;
        yield return null;
    }

    private void AddObject(Vector2Int position, GameObject newGridObject)
    {
        GameObject newObject = Instantiate(newGridObject);

        if (newObject.GetComponent<Movable>() != null)
            newObject.GetComponent<Movable>().SetStartPosition(new Vector3(position.x, 0, position.y));

        newObject.transform.position = tiles[position.x, position.y].transform.position;
        newObject.transform.parent = tiles[position.x, position.y].transform;
    }

    public GameObject GetTile(Vector3Int position)
    {
        return tiles[position.x, position.z];
    }

    public GameObject GetTile(Vector3 position)
    {
        if (position.x < 0 | position.z < 0) 
            return null;
        if (position.x >= gridSize.x | position.z >= gridSize.y)
            return null;
        return tiles[(int)position.x, (int)position.z];
    }

    public GameObject TileFromWorldPoint(Vector3 position)
    {
        float percentX = Mathf.Clamp01((position.x + gridSize.x / 2) / gridSize.x);
        float percentY = Mathf.Clamp01((position.z + gridSize.y / 2) / gridSize.y);

        int x = Mathf.RoundToInt((gridSize.x - 1) * percentX);
        int y = Mathf.RoundToInt((gridSize.y - 1) * percentY);
        return tiles[x, y];
    }

    public Vector3 TileIndexFromWorldPoint(Vector3 position)
    {
        int x = (int)(position.x - worldBottomLeft.x);
        int y = (int)(position.z - worldBottomLeft.y);

        return new Vector3(x, 0, y);
    }

    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        int x = (int)(worldPosition.x - worldBottomLeft.x);
        int y = (int)(worldPosition.z - worldBottomLeft.z);
        //Debug.Log(worldPosition.x + ":" + worldPosition.z);
        //Debug.Log(x + ":" + y);
        if (grid[x, y] != null)
            return grid[x, y];
        return null;
    }

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;
                if (Mathf.Abs(x) == 1 && Mathf.Abs(y) == 1)
                    continue;

                int checkX = node.GetGridPosition('x') + x;
                int checkY = node.GetGridPosition('y') + y;

                if (checkX >= 0 && checkX < gridSize.x && checkY >= 0 && checkY < gridSize.y)
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }

    private void OnDrawGizmos()
    {
        if (grid != null)
        {
            foreach (Node n in grid)
            {
                Gizmos.color = (n.isWalkable()) ? Color.white : Color.red;
                Gizmos.DrawCube(n.GetWorldPosion(), Vector3.one * (1-0.05f));
            }
        }
    }
}
