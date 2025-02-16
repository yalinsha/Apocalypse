using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static Vector2Int ToVector2Int(this Vector3Int v)
    {
        return new Vector2Int(v.x, v.y);
    }
    public static Vector3Int ToVector3Int(this Vector2Int v, int z)
    {
        return new Vector3Int(v.x, v.y, z);
    }
}
