using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class BaseBuilding : MonoBehaviour
{
    public string buildingName;
    public int level = 0;//等级，建造中时是0级
    public Vector2Int position;
    public Vector2Int span;//长和宽
    public BuildingInfoPro buildingInfoPro;

    public bool isUpgrading = false;
    float timeSinceUpgrade;
    float currentUpgradeDuration;
    Dictionary<Vector2Int, BaseBuilding> landUseMap = MapManager.Instance.landUseMap;

    public float TimeLeft => currentUpgradeDuration - timeSinceUpgrade;

    public virtual void Initialize(string buildingName, Vector2Int position, Vector2Int span)
    {
        this.buildingName = buildingName;
        this.position = position;
        this.span = span;
        buildingInfoPro = BuildingInfoManager.Instance.GetBuildingInfo(buildingName);

        EventManager.Instance.onStartConstruct(this);
        EventManager.Instance.onStatusChanged();
        StartUpgrade();
    }
    void StartUpgrade()
    {
        EventManager.Instance.onStartUpgrade(this);
        isUpgrading = true;
        timeSinceUpgrade = 0;
        currentUpgradeDuration = buildingInfoPro.upgradeDurationList[level] * BuffManager.Instance.constructionTimeMultiplier;
    }
    void FinishUpgrade()
    {
        ++level;
        isUpgrading = false;
        EventManager.Instance.onFinishUpgrade(this);
        EventManager.Instance.onStatusChanged();
    }
    public void Demolish()
    {
        EventManager.Instance.onDemolish(this);
        Destroy(gameObject);
        EventManager.Instance.onStatusChanged();
    }
    protected virtual void Update()
    {
        if (isUpgrading)
        {
            timeSinceUpgrade += Time.deltaTime;
            if (timeSinceUpgrade >= currentUpgradeDuration)
            {
                FinishUpgrade();
            }
        }
    }
    protected bool HasNeighbor(string name)
    {
        return MapManager.Instance.IsNear(position, span, new List<string> { name }, true);
    }
    protected bool HasNeighbor(string name, float range)
    {
        return MapManager.Instance.IsNear(position, span, new List<string> { name }, true, range);
    }
    protected HashSet<BaseBuilding> GetNeighborsInRange(float range)
    {
        Vector2Int u, v;
        int t = Mathf.CeilToInt(range);
        HashSet<BaseBuilding> set = new();
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
                            if (landUseMap.ContainsKey(u) && landUseMap[u] != null)
                                set.Add(landUseMap[u]);
                        }
                    }
                }
            }
        }
        set.Remove(this);
        return set;
    }
    //决定弹出面板的升级按钮是否置灰
    public bool CanUpgrade()
    {
        if (level == buildingInfoPro.buildingInfo.maxLevel)
        {
            return false;
        }
        //foreach (KeyValuePair<string, int> pair in buildingInfoPro.upgradeRestrictionList[level])
        //{
        //    if (!MapManager.Instance.highestLevelDict.ContainsKey(pair.Key) || MapManager.Instance.highestLevelDict[pair.Key] < pair.Value)
        //    {
        //        return false;
        //    }
        //}
        if (!ResourceManager.Instance.freeUpgradeDict.ContainsKey(buildingName) || !ResourceManager.Instance.freeUpgradeDict[buildingName])
        {
            foreach (var t in buildingInfoPro.upgradeCostList[level])
            {
                if (ResourceManager.Instance.GetResourceAmount(t.Key) < t.Value)
                {
                    return false;
                }
            }
        }
        return true;
    }
}
