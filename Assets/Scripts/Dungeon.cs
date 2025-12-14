using System;
using System.Collections.Generic;
using UnityEngine;

public class Dungeon
{

    private Tile[,] tiles;

    private List<Room> rooms;

    public Tile[,] Tiles { get { return tiles; } }

    public List<Room> Rooms { get { return rooms; } }

    private System.Random rng = new();

    private int numOfTries = 500;

    private List<Vector2Int> cardinalDirection;

    public void SetTile(int x, int z, Tile tile)
    {
        tiles[x, z] = tile;
    }

    //public Dungeon(Tile[,] _tiles, List<Room> _rooms)
    //{
    //    tiles = _tiles;
    //    rooms = _rooms;
    //}

    /// <summary>
    /// Default constructor
    /// </summary>
    public Dungeon() { }

    /// <summary>
    /// Constructor to initialize the Tile array
    /// </summary>
    /// <param name="width"></param>
    /// <param name="depth"></param>
    public Dungeon(int width, int depth)
    {
        // Initialize dungeon array
        tiles = new Tile[width, depth];

        // Initialize the dungeon rooms
        rooms = new List<Room>();

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < depth; y++)
            {
                Tile tile = new();

                SetTile(x, y, tile);
            }
        }

        // Initialize cardinalDirections
        cardinalDirection = new List<Vector2Int>();

        // North
        cardinalDirection.Add(new Vector2Int(0, 1));
        // East
        cardinalDirection.Add(new Vector2Int(1, 0));
        // South
        cardinalDirection.Add(new Vector2Int(0, -1));
        // West
        cardinalDirection.Add(new Vector2Int(-1, 0));
    }

    public Dungeon(Tile[,] _tiles)
    {
        tiles = _tiles;
    }

    public void AddDungeonEntrance(int _dungeonWidth, int _dungeonDepth)
    {
        // Entrance is a 3 x 3 room located at the middle bottom

        // Initialize start point
        int startX = 0;
        int startZ = 0;

        // Define entrtance along longer axis
        if (_dungeonWidth >= _dungeonDepth)
        {
            startX = _dungeonWidth / 2 - 1;
        }
        else
        {
            startZ = _dungeonDepth / 2 - 1;
        }

        // Create the entrance room
        Room entrance = new Room(startX, startZ, 3, 3, "Entrance");

        // Add the entrance room to the collection
        rooms.Add(entrance);

        // Update the dungeon grid
        for (int x = startX; x < startX + 3; x++)
        {
            for (int z = startZ;  z < startZ + 3; z++)
            {
                CarveTile(x, z, Tile.TileType.Room, true);
            }
        }
    }

    public void CreateDungeonRooms(int _dungeonWidth, int _dungeonDepth, int _minRoomSize, int _maxRoomSize)
    {
        for (var i = 0; i < numOfTries; i++)
        {
            // Select a random width for the room
            int width = UnityEngine.Random.Range(_minRoomSize, _maxRoomSize + 1);
            // Select a random depth for the room
            int depth = UnityEngine.Random.Range(_minRoomSize, _maxRoomSize + 1);

            // First select a random point in lower left quartile, then scale by 2
            // Ensure that the room is within the dungeon
            int startX = (int)(UnityEngine.Random.Range(1.0f, (_dungeonWidth - width) * 0.5f)) * 2 + 1;
            int startZ = (int)(UnityEngine.Random.Range(1.0f, (_dungeonDepth - depth) * 0.5f)) * 2 + 1;

            // Ensure that room fits within constraints of the dungeon
            // Otherwise an out of index error would occur
            if (startX + width > _dungeonWidth - 1 || startZ + depth > _dungeonDepth - 1)
            {
                //Debug.Log("NX: " + startX + ", NZ: " + startZ + ", width: " + width + ", " + depth);
                Debug.Log("Error: Room does not fit - dungeon too small or error in room generation");
                continue;
            }

            // Create a new room with the random generated parameters
            Room newRoom = new Room(startX, startZ, width, depth);

            // Check if the newly created room overlaps an existing room
            bool overlaps = false;

            foreach (var other in Rooms)
            {
                if (newRoom.Intersects(other))
                {
                    overlaps = true;
                    break;
                }
            }

            // If it does, do not add room and retry
            if (overlaps) continue;

            // Add new non-overlapping room
            rooms.Add(newRoom);

            Debug.Log("New Room added: " + newRoom.ToString());

            // Update the dungeon grid
            for (int x = startX; x < startX + width; x++)
            {
                for (int z = startZ; z < startZ + depth; z++)
                {
                    CarveTile(x, z, Tile.TileType.Room, true);
                }
            }
        }
    }


    private void CarveTile(int x, int z, Tile.TileType tileType, bool isVisible = true)
    {
        tiles[x, z].Type = tileType;
        tiles[x, z].IsVisible = isVisible;
    }

    /// <summary>
    /// Random room generator of varying size and position on the map
    /// </summary>
    /// <param name="count"></param>
    /// <param name="minSize"></param>
    /// <param name="maxSize"></param>
    /// <param name="mapSize"></param>
    /// <returns></returns>
    public List<Room> GenerateRooms(int count, int minSize, int maxSize, int mapSize)
    {
        var rooms = new List<Room>();

        for (int i = 0; i < count; i++)
        {
            // random width
            int w = rng.Next(minSize, maxSize);
            // random length
            int l = rng.Next(minSize, maxSize);
            // random left-bottom x coordinate
            int x = rng.Next(1, mapSize - w - 1);
            // random left-bottom z coordinate
            int z = rng.Next(1, mapSize - l - 1);

            rooms.Add(new Room(x, z, w, l));
        }

        return rooms;
    }

    

     /// <summary>
    /// Detect if rooms intersect.
    /// If there is overlap, move by the horizontal and/or vertical distance between centres
    /// </summary>
    /// <param name="rooms"></param>
    public void SeparateRooms(List<Room> rooms)
    {
        // variable to detect if rooms overlap and movement is required
        bool moved;

        do
        {
            // initialse
            moved = false;

            // Check if each room intersects with each other
            // If it does, move by the difference between centres
            foreach (var r1 in rooms)
            {
                foreach (var r2 in rooms)
                {
                    // same room selected to check
                    if (r1 == r2) continue;

                    if (r1.Intersects(r2))
                    // intersection detected
                    {
                        // calculate horizontal distance between centres
                        //int dx = r1.CenterX - r2.CenterX;
                        // calculate vertical distance between centres
                        //int dz = r1.CenterZ - r2.CenterZ;

                        // move in the X axis if required
                        //if (dx != 0) r1.X += Math.Sign(dx);
                        // move in the Z axis if required
                        //if (dz != 0) r1.Z += Math.Sign(dz);

                        // update variable
                        moved = true;
                    }
                }
            }
        } while (moved);
    }

    /// <summary>
    /// Print the properties of the rooms in the dungeon
    /// </summary>
    /// <param name="rooms"></param>
    public void PrintRooms(List<Room> rooms)
    {
        var count = 0;

        foreach (var room in rooms)
        {
            count++;

            Debug.Log(count.ToString() + " -> " + room.ToString());
        }
    }
}
