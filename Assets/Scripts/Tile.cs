using System;
using UnityEngine;

public class Tile
{

    public enum TileType
    {
        Wall,
        Room,
        Corridor,
        Door
    }

    private TileType tType;
    private bool isVisible;

    /// <summary>
    /// The type of tile
    /// </summary>
    public TileType Type
    {
        get { return tType; }
        set { tType = value; }
    }

    /// <summary>
    /// Set visibility of tile.
    /// Used after dungeon generation to hide unused areas.
    /// </summary>
    public bool IsVisible
    {
        get { return isVisible; }
        set { isVisible = value; }
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    public Tile()
    {
        tType = TileType.Wall;
        isVisible = false;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="_tType"></param>
    /// <param name="_isVisible"></param>
    public Tile(TileType _tType, bool _isVisible)
    {
        tType = _tType;
        isVisible = _isVisible;
    }

    public void PrintTile(Tile tile)
    {
        Console.WriteLine("Type: " + tile.Type + ", isVisible: " + (tile.isVisible ? "1" : "0"));
    }

    public override string ToString()
    {
        return ("Type: " + tType + ", isVisible: " + (isVisible ? "1" : "0"));
    }
}
