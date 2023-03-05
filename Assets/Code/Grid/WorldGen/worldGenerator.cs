//using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class worldGenerator : MonoBehaviour
{
    [SerializeField] private Vector2Int roomSize;
    [SerializeField] private Vector2Int amountOfRooms;
    [SerializeField] private Vector2Int amountOfCorridors;
    [SerializeField] private GameObject[] tiles;

    private Vector3 worldBottomLeft;
    private Vector2Int worldSize;
    private Vector2Int worldOffset;

    private GameObject[,] world;

    private List<Room> rooms;
    private List<Vector3Int> doors;
    private List<Vector3> corridors;

    private Queue<IEnumerator> coroutineQueue;

    public GameObject[,] GenerateWorld(Vector2Int gridSize, Vector3 gridBottomLeft, Vector2Int offset)
    {
        worldBottomLeft = gridBottomLeft;
        worldSize = gridSize;
        worldOffset = offset;
        world = new GameObject[worldSize.x, worldSize.y];

        GenerateRooms();
        return world;
    }

    private void GenerateRooms()
    {
        int placedRooms = 0;
        int roomsToPlace = Random.Range(amountOfRooms.x, amountOfRooms.y);
        rooms = new List<Room>();
        doors = new List<Vector3Int>();
        corridors = new List<Vector3>();
        while (placedRooms < roomsToPlace)
        {

            int roomWidth  = Random.Range(roomSize.x, roomSize.y);
            int roomHeight = Random.Range(roomSize.x, roomSize.y);
            int doorsToPlace = Random.Range(amountOfCorridors.x, amountOfCorridors.y);
            //Debug.Log("Room contains " + doorsToPlace);
            int doorsPlaced = 0;

            Vector2Int leftBottomCorner = new Vector2Int(Random.Range(worldOffset.x, worldSize.x - roomWidth - worldOffset.x), Random.Range(worldOffset.y, worldSize.y - roomHeight - worldOffset.y));

            int tileID = 0;

            if (couldPlaceRoom(roomWidth, roomHeight, leftBottomCorner))
            {
                rooms.Add(new Room());
                for (int y = leftBottomCorner.y; y <= leftBottomCorner.y + roomHeight; y++)
                {
                    for (int x = leftBottomCorner.x; x <= leftBottomCorner.x + roomWidth; x++)
                    {
                        Vector3 tilePosition = new Vector3(x, 0, y);
                        if (x == leftBottomCorner.x | y == leftBottomCorner.y | x == leftBottomCorner.x + roomWidth | y == leftBottomCorner.y + roomHeight)
                        {
                            tileID = 0;
                            if (x != leftBottomCorner.x && y != leftBottomCorner.y | x!= leftBottomCorner.x + roomWidth && y != leftBottomCorner.y + roomHeight)
                            {
                                float doorChance = 25;
                                if(rooms.Last<Room>().doorCount() > 0)
                                {
                                    doorChance = Vector3.Distance(doors.Last<Vector3Int>(), new Vector3(x,0,y));
                                }
                                if(doorsToPlace != doorsPlaced && Random.Range(0,100) < doorChance)
                                {
                                    tileID = 2;
                                    doors.Add(new Vector3Int(x, 0, y));
                                    rooms.Last<Room>().addDoor(new Vector3Int(x, 0, y));
                                    doorsPlaced++;
                                }
                            }
                        }
                        else
                        {
                            tileID = 1;
                        }
                        addTile(tilePosition, tiles[tileID]);
                    }
                }
                placedRooms++;
            }
        }
    }

    public void GenerateCorridors()
    {
        List<Vector3Int> doorsWithCorridors = new List<Vector3Int>();
        
        for (int i = 0; i < rooms.Count; i++)
        {
            for (int j = 0; j < rooms[i].doorCount(); j++)
            {
                if (doorsWithCorridors.Contains(rooms[i].GetDoor(j)))
                    continue;

                Debug.Log("doors without corridors " + doors.Count + ", doors with corridors " + doorsWithCorridors.Count);

                bool foundDoor = false;
                int counter = 0;

                while (!foundDoor && counter < 100)
                {
                    counter++;
                    int RandomRoom = Random.Range(0, rooms.Count() - 1);
                    if (RandomRoom == i)
                        continue;

                    int RandomDoor = Random.Range(0, rooms[RandomRoom].doorCount());
                    if (!doorsWithCorridors.Contains(rooms[RandomRoom].GetDoor(RandomDoor)))
                    {
                        PathRequestManager.RequestPath(TileToWorldPos(rooms[i].GetDoor(j)), TileToWorldPos(rooms[RandomRoom].GetDoor(RandomDoor)), OnPathFound);
                        foundDoor = true;
                        doorsWithCorridors.Add(rooms[i].GetDoor(j));
                        doorsWithCorridors.Add(rooms[RandomRoom].GetDoor(RandomDoor));
                        doors.Remove(rooms[i].GetDoor(j));
                        doors.Remove(rooms[RandomRoom].GetDoor(RandomDoor));
                    }
                    else
                    {
                        foundDoor = false;
                    }

                    if (doors.Count <= 1 && !foundDoor)
                    {
                        PathRequestManager.RequestPath(TileToWorldPos(rooms[i].GetDoor(j)), TileToWorldPos(rooms[RandomRoom].GetDoor(RandomDoor)), OnPathFound);
                        foundDoor = true;
                        doorsWithCorridors.Add(rooms[i].GetDoor(j));
                        doorsWithCorridors.Add(rooms[RandomRoom].GetDoor(RandomDoor));
                        doors.Remove(rooms[i].GetDoor(j));
                        doors.Remove(rooms[RandomRoom].GetDoor(RandomDoor));
                    }
                }
            }
        }
    }



    private IEnumerator PlaceWalls()
    {
        for(int x = 0; x < worldSize.x; x++)
        {
            for (int y = 0; y < worldSize.y; y++)
            {
                bool shouldPlaceWall = false;
                Vector3Int tilePosition = new Vector3Int(x, 0, y);
                List<GameObject> neighbours = GetNeighbours(tilePosition);
                //Debug.Log(neighbours.Count);
                for (int k = 0; k < neighbours.Count; k++)
                {
                    if (neighbours[k].CompareTag("Walkable")  && neighbours[k] != null)
                        shouldPlaceWall = true;
                    Debug.Log(neighbours[k].tag);
                }
                //Debug.Log(shouldPlaceWall); 
                //if (world[x, y] == null)
                //    shouldPlaceWall = true;

                if (shouldPlaceWall)
                {
                    //Debug.Log("placing wall at " + tilePosition);
                    addTile(tilePosition, tiles[0]);
                }
                    
            }
        }
        yield return null;   
    }

    private void OnPathFound(Vector3[] newPath, bool pathSucces)
    {
        if (pathSucces)
        {
            Debug.Log("path found");
            StartCoroutine(addPath(newPath));
            //coroutineQueue.Enqueue(addPath(newPath));
        }
        else
        {
            Debug.Log("path not found");
        }
    }

    private IEnumerator addPath(Vector3[] path)
    {
        for (int i = 0; i < path.Length; i++)
            corridors.Add(path[i]);

        yield return StartCoroutine(fillPath(corridors, tiles[1]));
    }

    public List<GameObject> GetNeighbours(Vector3Int position)
    {
        List<GameObject> neighbours = new List<GameObject>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                int checkX = position.x + x;
                int checkY = position.z + y;

                if (checkX >= 0 && checkX < worldSize.x && checkY >= 0 && checkY < worldSize.y && world[checkX, checkY] != null)
                {
                    neighbours.Add(world[checkX, checkY]);
                }
            }
        }
        return neighbours;
    }
    private bool couldPlaceRoom(int roomWidth, int roomHeight, Vector2Int leftBottomCorner)
    {
        for (int y = leftBottomCorner.y - 1; y <= leftBottomCorner.y + roomHeight + 1; y++)
        {
            for (int x = leftBottomCorner.x - 1; x <= leftBottomCorner.x + roomWidth + 1; x++)
            {
                if (world[x, y] != null)
                    return false;
            }
        }

        return true;
    }

    private void addTile(Vector3 position, GameObject tileType)
    {
        if (world[(int)position.x, (int)position.z] == null)
        {
            GameObject newTile = Instantiate(tileType);
            newTile.transform.position = worldBottomLeft + transform.position + position;
            newTile.name = "Tile " + position.x + " " + position.z;
            newTile.transform.parent = transform;

            world[(int)position.x, (int)position.z] = newTile;
        }
    }

    private IEnumerator fillPath(List<Vector3> path, GameObject tileType)
    {
        for (int i = 0; i < path.Count; i++)
        {
            Vector3 position = WorldPosToTile(path[i]);
            addTile(position, tileType);
        }
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(PlaceWalls());
    }


    private Vector3 TileToWorldPos(Vector3Int position)
    {
        Vector3 worldPosition = worldBottomLeft + transform.position + position;

        return worldPosition;
    }

    private Vector3 WorldPosToTile(Vector3 position)
    {
        int x = (int)(position.x - worldBottomLeft.x);
        int y = (int)(position.z - worldBottomLeft.z);

        return new Vector3(x, 0, y);
    }
}

public class Room
{
    private List<Vector3Int> doors;

    public Room()
    {
        doors = new List<Vector3Int>();
    }

    public Vector3Int GetDoor(int ID)
    {
        if (ID < doors.Count)
            return doors[ID];
        else
            throw new System.Exception("Door ID out of range " + ID);
    }

    public void addDoor(Vector3Int door)
    {
        doors.Add(door);
    }
    public bool Contains(Vector3Int door)
    {
        return doors.Contains(door);
    }

    public bool Equals(List<Vector3Int> doorList)
    {
        return doorList.Equals(doors);
    }

    public int doorCount()
    {
        return doors.Count;
    }
}
