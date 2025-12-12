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
    public int StartZ { get; set; }

    public int Width { get; set; }

    public int Depth { get; set; }

    public string Tag { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public Room()
    {
        StartX = 0;
        StartZ = 0;
        Width = 0;
        Depth = 0;
        Tag = "NA";
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <param name="width"></param>
    /// <param name="length"></param>
    public Room(int x, int z, int width, int depth, string tag = "NA")
    {
        StartX = x;
        StartZ = z;
        Width = width;
        Depth = depth;
        Tag = tag;
    }

    public bool Intersects(Room other)
    {
        // Check horizontal edges then vertical edges
        return !(StartX + Width < other.StartX || other.StartX + other.Width < StartX
            || StartZ + Depth < other.StartZ ||  other.StartZ + other.Depth < StartZ);
    }

    /// <summary>
    /// Print the properties of the room
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        string template = "X: {0}, Z: {1}, Width: {2}, Depth: {3}, Tag: {4}";
        string roomData = string.Format(template, StartX, StartZ, Width, Depth, Tag);

        return roomData;
    }
}
