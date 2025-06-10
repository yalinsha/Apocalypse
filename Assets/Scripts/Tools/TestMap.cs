using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 读取编辑器中的默认地图
/// </summary>
public class TestMap : MonoBehaviour
{
    private void Start()
    {
        Tilemap tilemap = MapRenderer.Instance.tilemap;
        Vector2Int v;
        TileBase tile;
        for (int i = -MapManager.mapSize; i <= MapManager.mapSize; i++)
        {
            for (int j = -MapManager.mapSize; j <= MapManager.mapSize; j++)
            {
                v = new Vector2Int(i, j);
                tile = tilemap.GetTile(v.ToVector3Int(0));
                if (tile != null)
                {
                    if(Enum.TryParse(Regex.Replace(tile.name, @"\d$", ""), true, out MapManager.ELandscapeType landscapeType))
                    {
                        MapManager.Instance.landscapeMap[v] = landscapeType;
                    }
                }
                if (!MapManager.Instance.landscapeMap.ContainsKey(v))
                {
                    MapManager.Instance.landscapeMap[v] = MapManager.ELandscapeType.Invalid;
                }
                tile = tilemap.GetTile(v.ToVector3Int(1));
                MapManager.Instance.buildabilityMap[v] = (tile == null);
                MapManager.Instance.magneticPositions.Add(Vector2Int.zero);
                MapManager.Instance.UpdateVisibility();
            }
        }
    }
}
