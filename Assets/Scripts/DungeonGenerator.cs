using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] private int _dungeonWidth = 29;
    [SerializeField] private int _dungeonDepth = 29;
    [SerializeField] private int _minRoomSize = 3;
    [SerializeField] private int _maxRoomSize = 5;

    [Header("** Map **")]
    [SerializeField] private Image _mapImage;
    [SerializeField] private Canvas _canvas;

    [Header("** Debug **")]
    [SerializeField] private bool _debugOutput = false;

    [HideInInspector] public Dungeon _dungeon;
    
    
    private Texture2D mapTexture;

    private int numOfTries = 800;

    private List<Edge> edges;

    private List<Edge> MST;

    private double chanceXtraCorridor = 0.1;

    private System.Random rng = new();


    private void Awake()
    {
        CreateMap();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Dungeon dimensions must be odd
        if (_dungeonWidth % 2 == 0)
            _dungeonWidth++;

        if (_dungeonDepth % 2 == 0)
            _dungeonDepth++;

        // Initialize room connectors
        edges = new List<Edge>();

        // Initialize the MST
        MST = new List<Edge>();

        GenerateDungeon();
    }

    // Update is called once per frame
    //void Update()
    //{

    //}

    #region DUNGEON

    private void GenerateDungeon()
    {
        if (_debugOutput)
            Debug.Log("**GenerateDungeon**");

        // Initialize the dungeon
        _dungeon = new Dungeon(_dungeonWidth, _dungeonDepth);

        // Add entrance
        AddDungeonEntrance();

        // Add dungeon rooms
        AddDungeonRooms();

        // Connect the rooms
        ConnectDungeonRooms();

        // Get the MST from the room connectors
        MST = GetMinimumSpanningTree();

        // Add dungeon corridors
        _dungeon.Corridors = AddDungeonCorridors();

        // Update the dungeon map for the corridors
        CarveCorridorTiles();

        // Print Debug Output if required
        if (_debugOutput)
            DebugOutput();

        UpdateMap();
    }

    /// <summary>
    /// Add the entrance to the dungeons along the middle of the longer
    /// of the left or the bottom edges
    /// </summary>
    private void AddDungeonEntrance()
    {
        // Entrance is a 2 x 2 room located at the middle left/bottom of the longest edge

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
        Room entrance = new Room(startX, startZ, 2, 2, "Entrance");

        // Add the entrance room to the collection
        _dungeon.Rooms.Add(entrance);

        // Update the dungeon grid
        for (int x = startX; x < startX + 2; x++)
        {
            for (int z = startZ; z < startZ + 2; z++)
            {
                CarveTile(x, z, Tile.TileType.Room, true);
            }
        }
    }

    /// <summary>
    /// Randomly add rectangular rooms of varying sizes
    /// within the range of _minRoomSize and _maxRoomSize
    /// </summary>
    private void AddDungeonRooms()
    {
        for (var i = 0; i < numOfTries; i++)
        {
            // Select a random width for the room
            int width = rng.Next(_minRoomSize, _maxRoomSize + 1);
            // Select a random depth for the room
            int depth = rng.Next(_minRoomSize, _maxRoomSize + 1);

            // Select a start point such that the room is within the bounds of the dungeon
            int startX = rng.Next(1, _dungeonWidth - width - 1);
            int startZ = rng.Next(1, _dungeonDepth - depth - 1);

            if (startX + width > _dungeonWidth || startZ + depth > _dungeonDepth)
            {
                Debug.Log("Error: " + "NX: " + startX + ", NZ: " + startZ + ", width: " + width + ", " + depth);
                continue;
            }

            // Create a new room with the random generated parameters
            Room newRoom = new Room(startX, startZ, width, depth);

            // Check if the newly created room overlaps an existing room
            bool overlaps = false;

            foreach (var other in _dungeon.Rooms)
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
            _dungeon.Rooms.Add(newRoom);

            if (_debugOutput)
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

    /// <summary>
    /// Add an edge between each room
    /// </summary>
    private void ConnectDungeonRooms()
    {
        List<Edge> nEdges = new List<Edge>();

        for (int i = 0; i < _dungeon.Rooms.Count; i++)
        {
            for (int j = i + 1; j < _dungeon.Rooms.Count; j++)
            {
                nEdges.Add(new Edge(_dungeon.Rooms[i], _dungeon.Rooms[j]));
            }
        }

        // Sorted in ascending order to be implied 'weight' for MST
        edges = nEdges.OrderBy(e => e.distance).ToList();
    }

    /// <summary>
    /// Determine the minimum spanning tree using Kruskal's algorithm
    /// </summary>
    /// <returns></returns>
    public List<Edge> GetMinimumSpanningTree()
    {
        var mst = new List<Edge>();

        var parent = _dungeon.Rooms.ToDictionary(r => r, r => r);

        // Root node
        Room Find(Room r)
        {
            if (parent[r] != r)
                parent[r] = Find(parent[r]);

            return parent[r];
        }

        // Update node
        void Union(Room a, Room b)
        {
            parent[Find(a)] = Find(b);
        }

        // edges collection is already sorted in increasing order
        foreach (var edge in edges)
        {
            if (Find(edge.A) != Find(edge.B))
            {
                mst.Add(edge);
                Union(edge.A, edge.B);
            }
        }

        return mst;
    }

    /// <summary>
    /// Starting with the MST edges, 
    /// Apply chance to non-MST edges and to form the dungeon corridors
    /// </summary>
    /// <returns></returns>
    public List<Edge> AddDungeonCorridors()
    {
        // Initiate new list by copying the MST
        var _dungeonCorridors = new List<Edge>(MST);

        /// Iterate over all edges
        foreach (var edge in edges)
        {
            // If it's not included in the MST, apply the chance to become a corridor
            if (!_dungeonCorridors.Contains(edge) && rng.NextDouble() <chanceXtraCorridor)
                _dungeonCorridors.Add(edge);
        }

        return _dungeonCorridors;
    }

    #endregion

    #region TILE

    /// <summary>
    /// Update the tiletype from the default 'Wall' to Room, Corridon
    /// </summary>
    /// <param name="_x"></param>
    /// <param name="_z"></param>
    /// <param name="_tileType"></param>
    /// <param name="_isVisible"></param>
    private void CarveTile(int _x, int _z, Tile.TileType _tileType, bool _isVisible = true)
    {
        _dungeon.Tiles[_x, _z].Type = _tileType;
        _dungeon.Tiles[_x, _z].IsVisible = _isVisible;
    }

    /// <summary>
    /// Update the dungeon map for the dungeon corridors
    /// </summary>
    private void CarveCorridorTiles()
    {
        foreach (var edge in _dungeon.Corridors)
        {
            // Room A Centre
            int cxa = edge.A.CenterX;
            int cza = edge.A.CenterZ;

            // Room B centre
            int cxb = edge.B.CenterX;
            int czb = edge.B.CenterZ;

            // Start from Room A
            int cx = cxa;
            int cz = cza;

            while (cz != czb)
            {
                if (_dungeon.Tiles[cx, cz].Type == Tile.TileType.Wall)
                    CarveTile(cx, cz, Tile.TileType.Corridor);

                cz += Math.Sign(czb - cz);
            }

            while (cx != cxb)
            {
                if (_dungeon.Tiles[cx, cz].Type == Tile.TileType.Wall)
                    CarveTile(cx, cz, Tile.TileType.Corridor);

                cx += Math.Sign(cxb - cx);
            }
        }
    }

    #endregion

    #region MAP

    private void CreateMap()
    {
        GameObject mapGO = new GameObject();
        mapGO.name = "MapObject";

        // add a rect Transform to replace the normal Transform
        RectTransform sRect = mapGO.AddComponent<RectTransform>();

        sRect.SetParent(_canvas.gameObject.transform, false);
        // Add a sprite Renderer to the new Object
        _mapImage = mapGO.AddComponent<Image>();

        mapTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        mapTexture.filterMode = FilterMode.Point;
    }

    public void UpdateMap()
    {
        int xDim = _dungeon.Tiles.GetLength(0);
        int yDim = _dungeon.Tiles.GetLength(1);

        mapTexture.Reinitialize(xDim, yDim, TextureFormat.RGBA32, false);

        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                switch (_dungeon.Tiles[x, y].Type)
                {
                    case Tile.TileType.Wall:
                        mapTexture.SetPixel(x, y, Color.black); 
                        break;

                    case Tile.TileType.Room:
                        mapTexture.SetPixel(x, y, Color.white);
                        break;

                    case Tile.TileType.Corridor:
                        mapTexture.SetPixel(x, y, Color.blue);
                        break;

                    default:
                        mapTexture.SetPixel(x, y, Color.clear);
                        break;
                }
            }
        }

        mapTexture.Apply();
        RefreshMap(new Vector2(xDim, yDim));
    }

    private void RefreshMap(Vector2 sizePx)
    {
        //keep order of this changes:		
        _mapImage.rectTransform.anchorMin = new Vector2(1F, 0F);
        _mapImage.rectTransform.anchorMax = new Vector2(1F, 0F);
        _mapImage.rectTransform.pivot = new Vector2(1F, 0F);
        _mapImage.rectTransform.offsetMin = Vector2.zero;
        _mapImage.rectTransform.offsetMax = sizePx * 1F;   //1 tile = 2x2 px
        _mapImage.rectTransform.anchoredPosition = new Vector2(-3, +3);//small dist from corner	

        Sprite sprite = Sprite.Create(mapTexture, new Rect(0, 0, mapTexture.width, mapTexture.height), new Vector2(0.5F, 0.5F));
        _mapImage.sprite = sprite;
        _mapImage.enabled = true;
    }

    #endregion

    #region DEBUG

    private void DebugOutput()
    {
        // All room connections
        PrintEdges();

        // MST
        PrintMST();

        // Dundeon corridors
        PrintDungeonCorridors();
    }

    private void PrintDungeonCorridors()
    {
        foreach (var edge in _dungeon.Corridors)
        {
            Debug.Log("Corridor: " + edge.ToString());
        }
    }

    private void PrintEdges()
    {
        foreach (var edge in edges)
        {
            Debug.Log("Edge: " + edge.ToString());
        }
    }

    private void PrintMST()
    {
        foreach (var edge in MST)
        {
            Debug.Log("MST: " + edge.ToString());
        }
    }

    #endregion
}
