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
    }

    public Dungeon(Tile[,] _tiles)
    {
        tiles = _tiles;
    }

    public void AddDungeonEntrance(int dungeonWidth, int dungeonDepth)
    {
        // Entrance is a 3 x 3 room located at the middle bottom

        // Initialize start point
        int startX = 0;
        int startZ = 0;

        // Define entrtance along longer axis
        if (dungeonWidth >= dungeonDepth)
        {
            startX = dungeonWidth / 2 - 1;
        }
        else
        {
            startZ = dungeonDepth / 2 - 1;
        }

        Room entrance = new Room(startX, startZ, 3, 3, "Entrance");

        rooms.Add(entrance);

        for (int x = startX; x < startX + 3; x++)
        {
            for (int z = startZ;  z < startZ + 3; z++)
            {
                CarveTile(x, z, Tile.TileType.Room, true);
            }
        }
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

    private void CarveTile(int x, int z, Tile.TileType tileType, bool isVisible = true)
    {
        tiles[x, z].Type = tileType;
        tiles[x, z].IsVisible = isVisible;
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
