using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] private int _dungeonWidth = 15;
    [SerializeField] private int _dungeonDepth = 15;
    [SerializeField] private int _minRoomSize = 3;
    [SerializeField] private int _maxRoomSize = 5;

    [Header("** Map **")]
    public Image mapImage;
    public Canvas canvas;

    [HideInInspector] public Dungeon _dungeon;
    
    
    private Texture2D mapTexture;

    private int numOfTries = 300;

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

        GenerateDungeon();
    }

    // Update is called once per frame
    //void Update()
    //{
        
    //}

    private void GenerateDungeon()
    {
        Debug.Log("**GenerateDungeon**");
        // Initialize the dungeon
        _dungeon = new Dungeon(_dungeonWidth, _dungeonDepth);

        // Add entrance
        AddDungeonEntrance();

        // Add dungeon rooms
        AddDungeonRooms();

        UpdateMap();
    }

    /// <summary>
    /// Add the entrance to the dungeons along the middle of the longer
    /// of the left or the bottom edges
    /// </summary>
    private void AddDungeonEntrance()
    {
        // Entrance is a 3 x 3 room located at the middle left/bottom of the longest edge

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

    #region TILE

    private void CarveTile(int _x, int _z, Tile.TileType _tileType, bool _isVisible = true)
    {
        _dungeon.Tiles[_x, _z].Type = _tileType;
        _dungeon.Tiles[_x, _z].IsVisible = _isVisible;
    }

    #endregion

    #region MAP

    private void CreateMap()
    {
        GameObject mapGO = new GameObject();
        mapGO.name = "MapObject";

        // add a rect Transform to replace the normal Transform
        RectTransform sRect = mapGO.AddComponent<RectTransform>();

        sRect.SetParent(canvas.gameObject.transform, false);
        // Add a sprite Renderer to the new Object
        mapImage = mapGO.AddComponent<Image>();

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
        mapImage.rectTransform.anchorMin = new Vector2(1F, 0F);
        mapImage.rectTransform.anchorMax = new Vector2(1F, 0F);
        mapImage.rectTransform.pivot = new Vector2(1F, 0F);
        mapImage.rectTransform.offsetMin = Vector2.zero;
        mapImage.rectTransform.offsetMax = sizePx * 1F;   //1 tile = 2x2 px
        mapImage.rectTransform.anchoredPosition = new Vector2(-3, +3);//small dist from corner	

        Sprite sprite = Sprite.Create(mapTexture, new Rect(0, 0, mapTexture.width, mapTexture.height), new Vector2(0.5F, 0.5F));
        mapImage.sprite = sprite;
        mapImage.enabled = true;
    }

    #endregion
}
