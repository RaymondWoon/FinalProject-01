using Unity.VisualScripting;
using UnityEngine;

public class Room
{
    // Lower left corner
    //public int X, Z;
    // Width & height of room
    //public int Width, Length;

    // Width of room
    //public int CenterX => (int)(X + Width * 0.5);
    // Length of room
    //public int CenterZ => (int)(Z + Length * 0.5);

    public int StartX { get; set; }
    public int StartY { get; set; }

    public int Width { get; set; }

    public int Length { get; set; }

    public string Tag { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public Room()
    {
        StartX = 0;
        StartY = 0;
        Width = 0;
        Length = 0;
        Tag = null;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <param name="width"></param>
    /// <param name="length"></param>
    public Room(int x, int y, int width, int length, string tag = null)
    {
        StartX = x;
        StartY = y;
        Width = width;
        Length = length;
        Tag = tag;
    }

    public bool Intersects(Room other)
    {
        // Check horizontal edges then vertical edges
        return !(StartX + Width < other.StartX || other.StartX + other.Width < StartX
            || StartY + Length < other.StartY ||  other.StartY + other.Length < StartY);
    }

    /// <summary>
    /// Print the properties of the room
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        string template = "X: {0}, Z: {1}, Width: {2}, Length: {3}";
        string roomData = string.Format(template, StartX, StartY, Width, Length);

        return roomData;
    }
}
