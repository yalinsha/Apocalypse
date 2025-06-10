using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static float distance = 0;
    public static bool tipSingleInfoShown = false;
    public static bool tipStationableShown = false;
    public static bool tipAutomaticShown = false;
    public static bool tipPermanentShown = false;
    public static Vector2Int ToVector2Int(this Vector3Int v)
    {
        return new Vector2Int(v.x, v.y);
    }
    public static Vector3Int ToVector3Int(this Vector2Int v, int z)
    {
        return new Vector3Int(v.x, v.y, z);
    }
    public static string StringForResourceDisplay(float amount)
    {
        int num = Mathf.FloorToInt(amount);
        return num.ToString();
    }
    public static string StringForRateDisplay(float rate)
    {
        string ret = rate.ToString("0.00") + "/s";
        if (rate > 0) ret = "+" + ret;
        return ret;
    }
}
