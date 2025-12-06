using Unity.VisualScripting;
using UnityEngine;

public class Room
{
    // Lower left corner
    public int X, Z;
    // Width & height of room
    public int Width, Length;

    // Width of room
    public int CenterX => (int)(X + Width * 0.5);
    // Length of room
    public int CenterZ => (int)(Z + Length * 0.5);

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <param name="width"></param>
    /// <param name="length"></param>
    public Room(int x, int z, int width, int length)
    {
        X = x;
        Z = z;
        Width = width;
        Length = length;
    }

    public bool Intersects(Room other)
    {
        // Check horizontal edges then vertical edges
        return !(X + Width < other.X || other.X + other.Width < X
            || Z + Length < other.Z ||  other.Z + other.Length < Z);
    }

    /// <summary>
    /// Print the properties of the room
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        string template = "X: {0}, Z: {1}, Width: {2}, Length: {3}";
        string roomData = string.Format(template, X, Z, Width, Length);

        return roomData;
    }
}
