using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
/// <summary>
/// 管理地形数据、土地利用数据和建筑分类登记表等
/// </summary>
public class MapManager : MonoBehaviour
{
    public static MapManager Instance
    {
        get; private set;
    }
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        EventManager.Instance.onStartConstruct += (BaseBuilding building) =>
        {
            Vector2Int v;
            for (int i = 0; i < building.span.x; i++)
            {
                for (int j = 0; j < building.span.y; j++)
                {
                    v = building.position + new Vector2Int(i, j);
                    landUseMap[v] = building;
                }
            }//添加到土地利用表
            buildings.Add(building);//添加到建筑集合
            if (building.buildingName == "MantleSampling")//记录已有采样站的地形
            {
                samplingDict[landscapeMap[building.position]] = true;
            }
            //记录该种建筑的数量，用来判断再建造会不会超出上限
            //这一游戏机制可能会改
            if (!buildingCountDict.ContainsKey(building.buildingName))
            {
                buildingCountDict[building.buildingName] = 1;
            }
            else
            {
                ++ buildingCountDict[building.buildingName];
            }
        };
        EventManager.Instance.onDemolish += (BaseBuilding building) =>
        {
            Vector2Int v;
            for (int i = 0; i < building.span.x; i++)
            {
                for (int j = 0; j < building.span.y; j++)
                {
                    v = building.position + new Vector2Int(i, j);
                    landUseMap[v] = null;
                }
            }//从土地利用表中移除
            buildings.Remove(building);//从建筑集合中移除
            if (building.buildingName == "MantleSampling")//重置已有采样站的地形
            {
                samplingDict[landscapeMap[building.position]] = false;
            }
        };
        EventManager.Instance.onStartUpgrade += (BaseBuilding building) =>
        {
            if (!(freeUpgradeDict.ContainsKey(building.buildingName) && freeUpgradeDict[building.buildingName]) || building.level == 0)//如果建造也可以免费就不加最后一个条件
            {
                EventManager.Instance.onResourceChanged(building.buildingInfoPro.upgradeCostList[building.level],1);
            }
            freeUpgradeDict[building.buildingName] = false;
        };
        EventManager.Instance.onFinishUpgrade += (BaseBuilding building) =>
        {
            if (!highestLevelDict.ContainsKey(building.buildingName)|| highestLevelDict[building.buildingName] < building.level)
            {
                highestLevelDict[building.buildingName] = building.level;
            }
        };
    }
    public const int mapSize = 30;//横纵坐标绝对值的上限
    public Dictionary<Vector2Int, bool> visibilityMap = new();
    public Dictionary<Vector2Int, bool> buildabilityMap = new();
    public Dictionary<Vector2Int, BaseBuilding> landUseMap = new();
    public Dictionary<Vector2Int, int> landscapeMap = new();//1,2,4,8,16,32分别代表草地、土地、焦土、沙漠、水体、裸岩
    public HashSet<BaseBuilding> buildings = new();
    public Dictionary<string,int> highestLevelDict = new();
    public Dictionary<string,int> buildingCountDict = new();
    public Dictionary<string, bool> freeUpgradeDict = new();
    public Dictionary<int, bool> samplingDict = new()
    {
        {1,false }, {2,false}, {4,false}, {8,false}, {32,false},
    };//记录各地形上是否已有地幔采样站
    public float RobotMultiplier
    {
        get; private set;
    }
    public float AIMultiplier
    {
        get; private set;
    }
    public bool CanBuild(string buildingName,Vector2Int position,bool flip)
    {
        BuildingInfoPro info = BuildingInfoManager.Instance.buildingInfoDict[buildingName];
        Vector2Int span;
        if (!flip)
        {
            span = new Vector2Int(info.buildingInfo.sizeX, info.buildingInfo.sizeY);
        }
        else
        {
            span = new Vector2Int(info.buildingInfo.sizeY, info.buildingInfo.sizeX);
        }
        Vector2Int v;
        for (int i = 0; i < span.x; i++)
        {
            for (int j = 0; j < span.y; j++)
            {
                v = new Vector2Int(position.x + i, position.y + j);
                if (!visibilityMap.ContainsKey(v) || !buildabilityMap.ContainsKey(v) || !visibilityMap[v] || !buildabilityMap[v])
                {
                    return false;
                }
                if (landUseMap.ContainsKey(v) && landUseMap[v] != null)
                {
                    return false;
                }
                if ((info.restrictionMask & landscapeMap[v]) == 0)
                {
                    return false;
                }
            }
        }
        //额外规则
        v = position;
        if (buildingName == "WaterStation")//必须临近水源
        {
            if (!(landscapeMap.ContainsKey(v + Vector2Int.left) && landscapeMap[v + Vector2Int.left] == 16)
             && !(landscapeMap.ContainsKey(v + Vector2Int.right) && landscapeMap[v + Vector2Int.right] == 16)
             && !(landscapeMap.ContainsKey(v + Vector2Int.up) && landscapeMap[v + Vector2Int.up] == 16)
             && !(landscapeMap.ContainsKey(v + Vector2Int.down) && landscapeMap[v + Vector2Int.down] == 16))
            {
                return false;
            }
        }
        else if (buildingName == "EcoGarden" && IsNear(v, span, new List<string> { "Mining", "OilWell", "NuclearPlant", "MantleSampling" }))
        {
            return false;
        }
        else if (buildingName == "HighLab" && IsNear(v, span, new List<string> { "WaterStation", "Corn" }))
        {
            return false;
        }
        else if ((new List<string> { "Mining", "OilWell", "NuclearPlant", "MantleSampling" }.Contains(buildingName)) && IsNear(v, span, new List<string> { "EcoGarden" }))
        {
            return false;
        }
        else if ((new List<string> { "WaterStation", "Corn" }.Contains(buildingName)) && IsNear(v, span, new List<string> { "HighLab" }))
        {
            return false;
        }
        else if (buildingName == "MantleSampling")
        {
            //不能跨地形建造
            if (landscapeMap[v] != landscapeMap[v + Vector2Int.right]
             || landscapeMap[v] != landscapeMap[v + Vector2Int.up]
             || landscapeMap[v] != landscapeMap[v + Vector2Int.up + Vector2Int.right])
            {
                return false;
            }
            if (samplingDict[landscapeMap[v]])
            {
                return false;
            }
        }
        return true;
    }
    /// <summary>
    /// 地块是否与建筑类别列表中的任意一种相邻，标准是欧式距离不超过range（默认为3.01f）。
    /// </summary>
    public bool IsNear(Vector2Int position, Vector2Int span, List<string> buildingNames, float range = 3.01f)
    {
        Vector2Int u, v;
        int t = Mathf.CeilToInt(range);
        for (int i = 0; i < span.x; i++)
        {
            for (int j = 0; j < span.y; j++)
            {
                v = position + new Vector2Int(i, j);
                for (int ii = -t; ii <= t; ++ii)
                {
                    for (int jj = -t; jj <= t; ++jj)
                    {
                        if (ii * ii + jj * jj <= range * range)
                        {
                            u = v + new Vector2Int(ii, jj);
                            if (landUseMap.ContainsKey(u) && landUseMap[u] != null && buildingNames.Contains(landUseMap[u].buildingName))
                                return true;
                        }
                    }
                }
            }
        }
        return false;
    }
    public void Build(string buildingName, Vector2Int position, bool flip)//初始化建筑对象
    {
        BuildingInfo info = BuildingInfoManager.Instance.buildingInfoDict[buildingName].buildingInfo;
        GameObject gameObject = new(buildingName);
        Vector2Int span;
        if (!flip)
        {
            span = new Vector2Int(info.sizeX, info.sizeY);
        }
        else
        {
            span = new Vector2Int(info.sizeY, info.sizeX);
        }
        switch (info.group)
        {
            case 1://标准生产建筑
                gameObject.AddComponent<StationableProductionBuilding>().Initialize(buildingName, position, span);
                break;
            case 2://消耗物资的功能建筑
                switch (info.buildingName)
                {
                    case "NursingHouse":
                        gameObject.AddComponent<NursingHouse>().Initialize(buildingName, position, span);
                        break;
                    case "LAN":
                        gameObject.AddComponent<LAN>().Initialize(buildingName, position, span);
                        break;
                    case "Gym":
                        gameObject.AddComponent<Gym>().Initialize(buildingName, position, span);
                        break;
                    case "RobotFactory":
                        gameObject.AddComponent<RobotFactory>().Initialize(buildingName, position, span);
                        break;
                }
                break;
            case 3:
                switch (info.buildingName)
                {
                    case "EcoGarden":
                        gameObject.AddComponent<EcoGarden>().Initialize(buildingName, position, span);
                        break;
                    case "Magnetic":
                        gameObject.AddComponent<Magnetic>().Initialize(buildingName, position, span);
                        break;
                    case "AirTower":
                        gameObject.AddComponent<AirTower>().Initialize(buildingName, position, span);
                        break;
                    case "AILab":
                        gameObject.AddComponent<AILab>().Initialize(buildingName, position, span);
                        break;
                    case "Apartment":
                        gameObject.AddComponent<Apartment>().Initialize(buildingName, position, span);
                        break;
                    case "CellRepair":
                        gameObject.AddComponent<CellRepair>().Initialize(buildingName, position, span);
                        break;
                    case "RocketBase":
                        gameObject.AddComponent<RocketBase>().Initialize(buildingName, position, span);
                        break;
                }
                break;
            case 4:
                gameObject.AddComponent<SingleProductionBuilding>().Initialize(buildingName, position, span);
                break;
        }
    }
}
