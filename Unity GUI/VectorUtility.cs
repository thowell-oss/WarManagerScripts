using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WarManager;

/// <summary>
/// Utility functions for conversion between the Unity Vector Library and War Manager Point Library
/// </summary>
[Notes.Author("Utility functions for conversion between the Unity Vector Library and War Manager Point Library")]
public static class VectorUtility
{
    public static Vector2Int ToVector2Int(this Point point)
    {
        var x = point.x;
        var y = point.y;

        return new Vector2Int(x, y);
    }

    public static Vector2 ToVector2(this Point point)
    {
        var x = point.x;
        var y = point.y;

        return new Vector2(x, y);
    }

    public static Vector3 ToVector3(this Point point)
    {
        var x = point.x;
        var y = point.y;

        return new Vector3(x, y, 0);
    }

    public static Vector3Int ToVector3Int(this Point point)
    {
        var x = point.x;
        var y = point.y;

        return new Vector3Int(x, y, 0);
    }

    public static Vector2 ToVector2(this Pointf point)
    {
        var x = point.x;
        var y = point.y;

        return new Vector2(x, y);
    }

    public static Vector3 ToVector3(this Pointf point)
    {
        var x = point.x;
        var y = point.y;

        return new Vector3(x, y, 0);
    }

    public static Point toPoint(this Vector2Int vec)
    {
        var x = vec.x;
        var y = vec.y;

        return new Point(x, y);
    }

    public static Pointf ToPointf(this Vector2 vec)
    {
        var x = vec.x;
        var y = vec.y;

        return new Pointf(x, y);
    }
}
