using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance
    {
        get; private set;
    }
    void Awake()
    {
        Instance = this;
    }
    Dictionary<string, float> resources = new()
    {
        {"electric", 0},
        {"mine", 0},
        {"food", 0},
        {"water", 0},
        {"oil", 0},
        {"chip", 0},
        {"ti", 0},
        {"carbon", 0},
        {"nuclear_part", 0},
        {"life_part", 0},
        {"shell_part", 0},
        {"chip_part", 0},
    };
    public Dictionary<string, float> productionRate = new()
    {
        {"electric", 0},
        {"mine", 0},
        {"food", 0},
        {"water", 0},
        {"oil", 0},
        {"chip", 0},
        {"ti", 0},
        {"carbon", 0},
    };
    readonly HashSet<ProductionBuilding> productionBuildings = new();
    readonly HashSet<BaseBuilding> robotFactories = new();
    public Dictionary<string, bool> freeUpgradeDict = new();
    public float RobotMultiplier
    {
        get; private set;
    }
    public float AIMultiplier
    {
        get; private set;
    }
    private void Start()
    {
        Debug.Log("ResourceManager Start Called.");
        EventManager.Instance.onFinishUpgrade += (BaseBuilding building) =>
        {
            if (building is ProductionBuilding productionBuilding)
            {
                productionBuildings.Add(productionBuilding);
            }
            if (building.buildingName == "AILab")
            {
                AIMultiplier += 0.1f;
            }
            if (building.buildingName == "RobotFactory")
            {
                robotFactories.Add(building);
            }
        };
        EventManager.Instance.onDemolish += (BaseBuilding building) =>
        {
            if (building is ProductionBuilding productionBuilding)
            {
                productionBuildings.Remove(productionBuilding);
            }
            if (building.buildingName == "AILab")
            {
                AIMultiplier -= 0.1f;
            }
        };
        EventManager.Instance.onStartUpgrade += (BaseBuilding building) =>
        {
            if (!(freeUpgradeDict.ContainsKey(building.buildingName) && freeUpgradeDict[building.buildingName]) || building.level == 0)//如果建造也可以免费就不加最后一个条件
            {
                ChangeResources(building.buildingInfoPro.upgradeCostList[building.level],-1);
            }
            freeUpgradeDict[building.buildingName] = false;
        };
        EventManager.Instance.onStatusChanged += () =>
        {
            Debug.Log("Recalculating ...");
            RecalculateRobotMultiplier();
            foreach (ProductionBuilding productionBuilding in productionBuildings)
            {
                productionBuilding.RecalculateMultiplier();
            }
            foreach (string resource in productionRate.Keys.ToList())
            {
                productionRate[resource] = 0;
            }
            foreach (ProductionBuilding productionBuilding in productionBuildings)
            {
                foreach (KeyValuePair<string, float> pair in productionBuilding.buildingInfoPro.massProductionList[productionBuilding.level-1])
                {
                    productionRate[pair.Key] += pair.Value * productionBuilding.multiplier;
                }
            }
        };
    }
    public void ChangeResources(Dictionary<string,float> dict, float multiplier = 1)
    {
        foreach (KeyValuePair<string, float> pair in dict)
        {
            ChangeResource(pair.Key, pair.Value * multiplier);
        }
    }
    public void ChangeResource(string type, float amount)
    {
        if (resources.ContainsKey(type))
        {
            resources[type] += amount;
        }
    }
    public float GetResourceAmount(string type)
    {
        return resources.ContainsKey(type) ? resources[type] : 0;
    }
    void RecalculateRobotMultiplier()
    {
        RobotMultiplier = 0;
        foreach(BaseBuilding building in robotFactories)
        {
            if ((building as IStationable).StationedCount > 0)
            {
                RobotMultiplier += 0.03f * building.level;
            }
        }
    }
    private void Update()
    {
        foreach (string resource in productionRate.Keys.ToList())
        {
            if(resources[resource] < 0)
            {
                resources[resource] = 0;
                EventManager.Instance.onResourceExhausted(resource);
            }
        }
    }
}
