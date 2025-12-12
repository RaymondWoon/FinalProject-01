using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class MapCoordinate
{
    public int x, z;

    public MapCoordinate(int _x, int _z)
    {
        x = _x;
        z = _z;
    }
}

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] private int _dungeonWidth = 11;
    [SerializeField] private int _dungeonDepth = 11;
    [SerializeField] private int _minRoomSize = 4;
    [SerializeField] private int _maxRoomSize = 6;
    //[SerializeField] private int _noOfRooms = 2;


    [HideInInspector] public Dungeon _dungeon;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Dungeon dimensions must be odd
        if (_dungeonWidth % 2 == 0)
            _dungeonWidth++;

        if (_dungeonDepth % 2 == 0)
            _dungeonDepth++;

        GenerateDungeon();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void GenerateDungeon()
    {
        Debug.Log("GenerateDungeon");
        _dungeon = new Dungeon(_dungeonWidth, _dungeonDepth);

        //Debug.Log(_dungeon.Tiles.Length);

        //PrintDungeonTiles();

        // Add Entrance
        _dungeon.AddDungeonEntrance(_dungeonWidth, _dungeonDepth);

        //PrintDungeonRooms();

        // Step 1: Generate rooms
        //var rooms = dungeon.GenerateRooms(_noOfRooms, _minRoomSize, _maxRoomSize, _mapSize);

        //Debug.Log("Initial Rooms:");
        //dungeon.PrintRooms(rooms);

        // Step 2: Check if rooms overlap, and separate them if they do
        //dungeon.SeparateRooms(rooms);

        //Debug.Log("Separated Rooms:");
        //dungeon.PrintRooms(rooms);

        //PrintDungeonTiles();
    }

    private void PrintDungeonTiles()
    {
        for (int z = 0; z < _dungeonDepth; z++)
        {
            for (int x = 0; x < _dungeonWidth; x++)
            {
                Debug.Log("X: " + x + ", Z: " + z + ", " + _dungeon.Tiles[x, z].ToString());
            }
        }
    }

    private void PrintDungeonRooms()
    {
        for (int i = 0; i < _dungeon.Rooms.Count; i++)
        {
            Debug.Log(_dungeon.Rooms[i].ToString());
        }
    }
}
