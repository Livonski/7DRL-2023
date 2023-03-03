using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    private GameObject[,] tiles;
    [SerializeField] private Vector2Int gridSize;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject testObject;

    private void Start()
    {
        tiles = new GameObject[gridSize.x, gridSize.y];
        InstantiateGrid(testObject);
        AddObject(new Vector2Int(0, 0), player);
    }

    private void InstantiateGrid(GameObject defaultTile)
    {
        for (int y= 0; y < gridSize.y; y++)
        {
            for(int x= 0; x < gridSize.x; x++)
            {
                GameObject newTile = Instantiate(defaultTile);
                newTile.transform.position = transform.position + new Vector3(x, 0, y);
                newTile.name = "Tile " + x + " " + y;
                newTile.transform.parent = transform;

                tiles[y, x] = newTile;
            }
        }
    }

    private void AddObject(Vector2Int position, GameObject newGridObject)
    {
        GameObject newObject = Instantiate(newGridObject);
        newObject.transform.position = tiles[position.y, position.x].transform.position;
        //newObject.transform.parent = tiles[position.y, position.x].transform;
    }

    //public void MoveObject(Vector2Int position, Vector2Int moveDirection)
    //{
    //
    //}
}
