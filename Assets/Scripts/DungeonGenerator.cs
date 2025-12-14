using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("** Map **")]
    public Image mapImage;
    public Canvas canvas;

    [HideInInspector] public Dungeon _dungeon;
    [HideInInspector] public Texture2D mapTexture;


    

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
    void Update()
    {
        
    }

    

    private void GenerateDungeon()
    {
        Debug.Log("**GenerateDungeon**");
        _dungeon = new Dungeon(_dungeonWidth, _dungeonDepth);

        //Debug.Log(_dungeon.Tiles.Length);

        //PrintDungeonTiles();

        // Add Entrance
        _dungeon.AddDungeonEntrance(_dungeonWidth, _dungeonDepth);

        //PrintDungeonRooms();

        // Add additional rooms
        _dungeon.CreateDungeonRooms(_dungeonWidth, _dungeonDepth, _minRoomSize, _maxRoomSize);

        // Step 1: Generate rooms
        //var rooms = dungeon.GenerateRooms(_noOfRooms, _minRoomSize, _maxRoomSize, _mapSize);

        //Debug.Log("Initial Rooms:");
        //dungeon.PrintRooms(rooms);

        // Step 2: Check if rooms overlap, and separate them if they do
        //dungeon.SeparateRooms(rooms);

        //Debug.Log("Separated Rooms:");
        //dungeon.PrintRooms(rooms);

        //PrintDungeonTiles();

        UpdateMap();
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
}
