using System;
using System.Collections.Generic;
using UnityEngine;
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
            int tmp = building.span.x == 3 ? -1 : 0;
            for (int i = tmp; i < building.span.x + tmp; i++)//添加到土地利用表
            {
                for (int j = tmp; j < building.span.y + tmp; j++)
                {
                    v = building.position + new Vector2Int(i, j);
                    landUseMap[v] = building;
                }
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
            if (building.buildingName == "MantleSampling")//记录已有采样站的地形
            {
                samplingDict[landscapeMap[building.position]] = true;
            }
        };
        //EventManager.Instance.onDemolish += (BaseBuilding building) =>
        //{
        //    Vector2Int v;
        //    for (int i = 0; i < building.span.x; i++)
        //    {
        //        for (int j = 0; j < building.span.y; j++)
        //        {
        //            v = building.position + new Vector2Int(i, j);
        //            landUseMap[v] = null;
        //        }
        //    }//从土地利用表中移除
        //    buildings.Remove(building);//从建筑集合中移除
        //    --buildingCountDict[building.buildingName];
        //    if (building.buildingName == "MantleSampling")//重置已有采样站的地形
        //    {
        //        samplingDict[landscapeMap[building.position]] = false;
        //    }
        //    if (building.buildingName == "AirTower")
        //    {
        //        airTowerPositions.Remove(building.position);
        //        UpdateBuildability();
        //    }
        //    if (building.buildingName == "Magnetic")
        //    {
        //        magneticPositions.Remove(building.position);
        //        UpdateVisibility();
        //    }
        //};
        EventManager.Instance.onFinishUpgrade += (BaseBuilding building) =>
        {
            buildings.Add(building);//添加到建筑集合
            if (!highestLevelDict.ContainsKey(building.buildingName)|| highestLevelDict[building.buildingName] < building.level)
            {
                highestLevelDict[building.buildingName] = building.level;
            }
            if (building.buildingName == "AILab" && AITimesLeft > 0)
            {
                --AITimesLeft;
                ResourceManager.Instance.ChangeResource("chip", ResourceManager.Instance.GetResourceAmount("chip") / 2);
            }
            if(building.buildingName == "AirTower")
            {
                airTowerPositions.Add(building.position);
                UpdateBuildability();
            }
            if (building.buildingName == "Magnetic")
            {
                magneticPositions.Add(building.position);
                UpdateVisibility();
            }
        };
        EventManager.Instance.onResourceExhausted += (str) =>
        {
            Dictionary<string, float> dic;
            foreach (var building in buildings)
            {
                dic = building.buildingInfoPro.massProductionList[building.level - 1];
                if (dic.ContainsKey(str) && dic[str] < 0)
                {
                    if(building is StationableProductionBuilding stationable)
                    {
                        PopulationManager.Instance.DecreaseStationToZero(stationable);
                    }
                    else if (building is AutomaticProductionBuilding automatic)
                    {
                        automatic.IsFunctioning = false;
                    }
                }
            }
            EventManager.Instance.onStatusChanged();
        };
    }
    public const int mapSize = 30;//横纵坐标绝对值的上限
    //public Dictionary<Vector2Int, bool> visibilityMap = new();
    public bool[,] visibilityMap = new bool[2 * mapSize + 1, 2 * mapSize + 1];
    public Dictionary<Vector2Int, bool> buildabilityMap = new();
    public Dictionary<Vector2Int, BaseBuilding> landUseMap = new();
    public enum ELandscapeType
    {
        Invalid = 0,
        Grass = 1,
        Land = 2,
        Scorched = 4,
        Desert = 8,
        Water = 16,
        Rock = 32,
    }
    public Dictionary<Vector2Int, ELandscapeType> landscapeMap = new();
    public HashSet<BaseBuilding> buildings = new();
    public Dictionary<string,int> highestLevelDict = new();
    public Dictionary<string,int> buildingCountDict = new();
    public Dictionary<ELandscapeType, bool> samplingDict = new()
    {
        {ELandscapeType.Grass,false }, {ELandscapeType.Land,false}, {ELandscapeType.Scorched,false}, {ELandscapeType.Desert,false}, {ELandscapeType.Rock,false},
    };//记录各地形上是否已有地幔采样站
    List<ELandscapeType> canSampleLandscapes = new List<ELandscapeType>{ ELandscapeType.Grass, ELandscapeType.Land, ELandscapeType.Scorched, ELandscapeType.Desert, ELandscapeType.Rock};
    public HashSet<Vector2Int> magneticPositions = new();
    HashSet<Vector2Int> airTowerPositions = new();
    public int AITimesLeft { get; private set; } = 3;

    public bool CanChoose(string buildingName)
    {
        if (SolarStormManager.Instance.IsInStorm) return false;
        BuildingInfoPro info = BuildingInfoManager.Instance.GetBuildingInfo(buildingName);
        if (info.upgradeDurationList[0] > SolarStormManager.Instance.TimeLeft)
        {
            //后续如果想弹提示就在这里
            return false;
        }
        foreach (var pair in info.upgradeCostList[0])
        {
            if(ResourceManager.Instance.GetResourceAmount(pair.Key) < pair.Value)
            {
                return false;
            }
        }
        //foreach(var pair in info.upgradeRestrictionList[0])
        //{
        //    if (!highestLevelDict.ContainsKey(pair.Key) || highestLevelDict[pair.Key] < pair.Value)
        //    {
        //        return false;
        //    }
        //}
        if(PopulationManager.Instance.AvailablePopulation < info.buildingInfo.workersRequired)
        {
            return false;
        }
        if(buildingName == "MantleSampling")
        {
            bool flag = false;
            foreach(ELandscapeType type in canSampleLandscapes)
            {
                if (!samplingDict[type])
                {
                    flag = true;
                }
            }
            if (!flag)
            {
                return false;
            }
        }
        return true;
    }
    public bool CanBuild(string buildingName,Vector2Int position,bool flip)
    {
        if (SolarStormManager.Instance.IsInStorm) return false;
        BuildingInfoPro info = BuildingInfoManager.Instance.GetBuildingInfo(buildingName);
        if (info.upgradeDurationList[0] > SolarStormManager.Instance.TimeLeft)
        {
            //后续如果想弹提示就在这里
            return false;
        }
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
        int tmp = span.x == 3 ? -1 : 0;
        for (int i = tmp; i < span.x + tmp; i++)
        {
            for (int j = tmp; j < span.y + tmp; j++)
            {
                v = new Vector2Int(position.x + i, position.y + j);
                if (!visibilityMap[position.x + i + mapSize, position.y + j + mapSize])
                {
                    return false;
                }
                if (landUseMap.ContainsKey(v) && landUseMap[v] != null)
                {
                    return false;
                }
                if ((info.restrictionMask & (int)landscapeMap[v]) == 0)
                {
                    return false;
                }
            }
        }
        //额外规则
        v = position;
        if (buildingName == "WaterStation")//必须临近水源
        {
            if (!(landscapeMap.ContainsKey(v + Vector2Int.left) && landscapeMap[v + Vector2Int.left] == ELandscapeType.Water)
             && !(landscapeMap.ContainsKey(v + Vector2Int.right) && landscapeMap[v + Vector2Int.right] == ELandscapeType.Water)
             && !(landscapeMap.ContainsKey(v + Vector2Int.up) && landscapeMap[v + Vector2Int.up] == ELandscapeType.Water)
             && !(landscapeMap.ContainsKey(v + Vector2Int.down) && landscapeMap[v + Vector2Int.down] == ELandscapeType.Water))
            {
                return false;
            }
        }
        else if (buildingName == "CellRepair" && IsNear(v, span, new List<string> {"OilWell","CarbonRefine"}, false))
        {
            return false;
        }
        //else if (buildingName == "HighLab" && IsNear(v, span, new List<string> { "WaterStation", "Corn" }, false))
        //{
        //    return false;
        //}
        else if ((new List<string> { "OilWell", "CarbonRefine" }.Contains(buildingName)) && IsNear(v, span, new List<string> { "CellRepair" }, false))
        {
            return false;
        }
        //else if ((new List<string> { "WaterStation", "Corn" }.Contains(buildingName)) && IsNear(v, span, new List<string> { "HighLab" }, false))
        //{
        //    return false;
        //}
        else if (buildingName == "MantleSampling")
        {
            //不能跨地形建造，默认采样站是2*2
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
    public bool IsNear(Vector2Int position, Vector2Int span, List<string> buildingNames, bool functioningOnly, float range = 3.01f)
    {
        Vector2Int u, v;
        int t = Mathf.CeilToInt(range);
        int tmp = span.x == 3 ? -1 : 0;
        for (int i = tmp; i < span.x + tmp; i++)
        {
            for (int j = tmp; j < span.y + tmp; j++)
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
                            {
                                if (!functioningOnly)
                                {
                                    return true;
                                }
                                if (landUseMap[u] is IStationable stationable && stationable.StationedCount > 0 || landUseMap[u] is AutomaticProductionBuilding automatic && automatic.IsFunctioning)
                                {
                                    return true;
                                }
                                if(landUseMap[u] is not IStationable && landUseMap[u] is not AutomaticProductionBuilding)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
        }
        return false;
    }
    public void Build(string buildingName, Vector2Int position, bool flip)//初始化建筑对象
    {
        if (!Extensions.tipSingleInfoShown && buildingName != "StarshipCenter")
        {
            GameEventManager.Instance.ShowTipByDescription("tipSingleInfo");
            Extensions.tipSingleInfoShown = true;
        }
        BuildingInfo info = BuildingInfoManager.Instance.GetBuildingInfo(buildingName).buildingInfo;
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
        Type type = null;
        switch (info.group)
        {
            case 1://基础资源生产建筑
                type = typeof(StationableProductionBuilding);
                break;
            case 2://消耗物资的功能建筑
                switch (buildingName)
                {
                    case "NursingHouse":
                        type = typeof(NursingHouse);
                        break;
                    case "Gym":
                        type = typeof(Gym);
                        break;
                    case "RobotFactory":
                        type = typeof(StationableProductionBuilding);
                        break;
                    case "LAN":
                        type = typeof(AutomaticProductionBuilding);
                        break;
                }
                break;
            case 3://不消耗资源的建筑
                if (buildingName == "EcoGarden")
                {
                    type = typeof(EcoGarden);
                }
                else
                {
                    type = typeof(BaseBuilding);
                }
                break;
            case 4://三级资源建筑
                type = typeof(SingleProductionBuilding);
                break;
        }
        (gameObject.AddComponent(type) as BaseBuilding).Initialize(buildingName, position, span);
    }
    public void UpdateVisibility()
    {
        float range = 10.1f;//硬编码
        for (int i = -mapSize; i <= mapSize; ++i)
        {
            for (int j = -mapSize; j <= mapSize; ++j)
            {
                visibilityMap[i+mapSize,j+mapSize] = false;
            }
        }
        int t = Mathf.CeilToInt(range);
        foreach (Vector2Int v in magneticPositions)
        {
            for (int ii = -t; ii <= t; ++ii)
            {
                for (int jj = -t; jj <= t; ++jj)
                {
                    if (ii * ii + jj * jj <= range * range && InRange(v.x + ii,v.y + jj))
                    {
                        visibilityMap[v.x + ii + mapSize, v.y + jj + mapSize] = true;
                    }
                }
            }
        }
        EventManager.Instance.onVisibilityUpdated();
        bool InRange(int x, int y)
        {
            return (x >= -mapSize && x <= mapSize && y >= -mapSize && y <= mapSize);
        }
    }
    void UpdateBuildability()
    {
        float range = 12.1f;//硬编码
        for (int i = -mapSize; i <= mapSize; ++i)
        {
            for (int j = -mapSize; j <= mapSize; ++j)
            {
                buildabilityMap[new Vector2Int(i, j)] = false;
            }
        }
        for (int i = -10; i <= 10; ++i)//硬编码
        {
            for (int j = -10; j <= 10; ++j)
            {
                buildabilityMap[new Vector2Int(i, j)] = true;
            }
        }
        foreach (Vector2Int v in airTowerPositions)
        {
            int t = Mathf.CeilToInt(range);
            for (int ii = -t; ii <= t; ++ii)
            {
                for (int jj = -t; jj <= t; ++jj)
                {
                    if (ii * ii + jj * jj <= range * range)
                    {
                        buildabilityMap[v + new Vector2Int(ii, jj)] = true;
                    }
                }
            }
        }
        EventManager.Instance.onBuildabilityUpdated();
    }
}
