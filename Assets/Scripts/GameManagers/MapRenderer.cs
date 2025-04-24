using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
/// <summary>
/// 管理地图渲染
/// </summary>
public class MapRenderer : MonoBehaviour
{
    public static MapRenderer Instance
    {
        get; private set;
    }
    private void Awake()
    {
        Instance = this;
    }
    public Tilemap tilemap;
    Tile currentTile;
    Tile mistTile;
    Tile unbuildableTile;
    Vector2Int lastPosition;
    static readonly Color translucent = new(1, 1, 1, 0.7f);
    static readonly Color translucentGreen = new(0, 1, 0, 0.7f);
    static readonly Color translucentRed = new(1, 0, 0, 0.7f);

    private void Start()
    {
        mistTile = Resources.Load<Tile>("Tiles/mist");
        unbuildableTile = Resources.Load<Tile>("Tiles/unbuildable");
        EventManager.Instance.onDemolish += (BaseBuilding building) =>
        {
            tilemap.SetTile(building.position.ToVector3Int(2), null);
        };
        EventManager.Instance.onVisibilityUpdated += () =>
        {
            for (int i = -MapManager.mapSize; i <= MapManager.mapSize; ++i)
            {
                for (int j = -MapManager.mapSize; j <= MapManager.mapSize; ++j)
                {
                    Vector2Int v = new(i, j);
                    Tile t = null;
                    if (!MapManager.Instance.visibilityMap.ContainsKey(v) || !MapManager.Instance.visibilityMap[v])
                    {
                        t = mistTile;
                    }
                    tilemap.SetTile(v.ToVector3Int(3), t);
                }
            }
        };
        EventManager.Instance.onBuildabilityUpdated += () =>
        {
            for (int i = -MapManager.mapSize; i <= MapManager.mapSize; ++i)
            {
                for (int j = -MapManager.mapSize; j <= MapManager.mapSize; ++j)
                {
                    Vector2Int v = new(i, j);
                    Tile t = null;
                    if (!MapManager.Instance.buildabilityMap.ContainsKey(v) || !MapManager.Instance.buildabilityMap[v])
                    {
                        t = unbuildableTile;
                    }
                    tilemap.SetTile(v.ToVector3Int(1), t);
                }
            }
        };
    }
    //地形瓦片在0层，表示不可建造的灰色透明瓦片在1层，建筑瓦片在2层，迷雾瓦片在3层，建造模式的悬浮瓦片在4层
    public void EnterConstructionMode(string buildingName)
    {
        currentTile = Resources.Load<Tile>("Tiles/Buildings/" + buildingName);
        Vector3Int v;
        for (int i = -MapManager.mapSize; i <= MapManager.mapSize; ++i)
        {
            for (int j = -MapManager.mapSize; j <= MapManager.mapSize; ++j)
            {
                v = new Vector3Int(i, j, 2);//
                tilemap.RemoveTileFlags(v,TileFlags.LockColor);
                tilemap.SetColor(v, translucent);
            }
        }
    }
    public void ChangePosition(Vector2Int position, bool canBuild)
    {
        tilemap.SetTile(lastPosition.ToVector3Int(4), null);
        tilemap.SetTile(position.ToVector3Int(4), currentTile);
        tilemap.RemoveTileFlags(position.ToVector3Int(4), TileFlags.LockColor);
        tilemap.SetColor(position.ToVector3Int(4), canBuild ? translucentGreen : translucentRed);
        lastPosition = position;
    }
    public void ExitConstructionMode()
    {
        tilemap.SetTile(lastPosition.ToVector3Int(4), null);
        Vector3Int v;
        for (int i = -MapManager.mapSize; i <= MapManager.mapSize; ++i)
        {
            for (int j = -MapManager.mapSize; j <= MapManager.mapSize; ++j)
            {
                v = new Vector3Int(i, j, 2);//
                tilemap.RemoveTileFlags(v, TileFlags.LockColor);
                tilemap.SetColor(v, Color.white);
            }
        }
    }
    public void Build()//填入瓦片
    {
        tilemap.SetTile(lastPosition.ToVector3Int(2), currentTile);
    }
}
