using System;
using System.Collections.Generic;
using UnityEngine;

public interface IStationable
{
    public int StationedCount { get; set; }
    public int MaxStationCount {
        get 
        {
            BaseBuilding building = this as BaseBuilding;
            return building.level > 0 ? building.level + 5 : 0;
        }
    }//默认实现
}
/// <summary>
/// 广义上的生产建筑即有massProduction表的建筑，所以也包含仅消耗的建筑
/// </summary>
public abstract class ProductionBuilding : BaseBuilding
{
    public float multiplier;

    protected float GlobalMultiplier()
    {
        if (buildingInfoPro.buildingInfo.group == 1 || buildingInfoPro.buildingInfo.group == 4)
        {
            return MapManager.Instance.RobotMultiplier + LivabilityManager.Instance.livability * 0.02f;
        }
        return 0;//仅消耗的建筑不受全局加成
    }

    protected float EnvironmentMultiplier()
    {
        float environmentMultiplier = 0;
        foreach (KeyValuePair<string, float> pair in buildingInfoPro.neighborBonusDict)
        {
            if (HasNeighbor(pair.Key))
            {
                environmentMultiplier += pair.Value;
            }
        }
        if (buildingInfoPro.buildingInfo.group == 1 || buildingInfoPro.buildingInfo.group == 4)//仅消耗的建筑不受LAN加成
        {
            if (HasNeighbor("LAN"))
            {
                environmentMultiplier += 0.2f;
            }
        }
        return environmentMultiplier;
    }
    public abstract void RecalculateMultiplier();
    protected override void Update()
    {
        base.Update();
        EventManager.Instance.onResourceChanged(buildingInfoPro.massProductionList[level],multiplier);
    }
}

public class StationableProductionBuilding : ProductionBuilding, IStationable
{
    public int StationedCount { get; set; }
    public override void RecalculateMultiplier()
    {
        if (StationedCount == 0)
        {
            multiplier = 0;
        }
        else
        {
            multiplier = 1 + (StationedCount - 1) * buildingInfoPro.buildingInfo.stationBonus + EnvironmentMultiplier() + GlobalMultiplier();
        }
    }
}

public class UnstationableProductionBuilding : ProductionBuilding
{
    public override void RecalculateMultiplier()
    {
        multiplier = 1 + EnvironmentMultiplier() + GlobalMultiplier();
    }
}
/// <summary>
/// 生产三级资源的建筑，目前的游戏设计中均是可派驻的
/// </summary>
public class SingleProductionBuilding : StationableProductionBuilding
{
    readonly Dictionary<string, float> localProduction = new();
    protected override void Update()
    {
        base.Update();
        if (level == 0) return;
        foreach (KeyValuePair<string, float> pair in buildingInfoPro.singleProductionList[level - 1])
        {
            if (!localProduction.ContainsKey(pair.Key))
            {
                localProduction[pair.Key] = 0;
            }
            if (pair.Key == "chip_part")
            {
                localProduction[pair.Key] += (multiplier + MapManager.Instance.AIMultiplier) * pair.Value * Time.deltaTime;
            }
            else
            {
                localProduction[pair.Key] += multiplier * pair.Value * Time.deltaTime;
            }
            if (localProduction[pair.Key] > 1)
            {
                EventManager.Instance.onResourceChanged(new() { { pair.Key, 1 }, } , 1 );
                --localProduction[pair.Key];
            }
        }
    }
}