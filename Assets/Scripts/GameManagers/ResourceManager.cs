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
        {"mine", 1000},
        {"food", 20000},
        {"water", 50000},
        {"oil", 0},
        //{"chip", 0},
        //{"ti", 0},
        //{"carbon", 0},
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
        //{"chip", 0},
        //{"ti", 0},
        //{"carbon", 0},
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
            Debug.Log("onStartUpgrade in ResourceManager");
            if (!(freeUpgradeDict.ContainsKey(building.buildingName) && freeUpgradeDict[building.buildingName]) || building.level == 0)//如果建造也可以免费就不加最后一个条件
            {
                ChangeResources(building.buildingInfoPro.upgradeCostList[building.level],-1);
            }
            freeUpgradeDict[building.buildingName] = false;
        };
        EventManager.Instance.onSolarStormStart += () =>
        {
            ChangeResource("mine", -SolarStormManager.Instance.currentInfo.mineDemand);
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
            productionRate["water"] += -PopulationManager.waterConsumption * PopulationManager.Instance.CurrentPopulation * BuffManager.Instance.waterConsumptionMultiplier;
            productionRate["food"] += -PopulationManager.foodConsumption * PopulationManager.Instance.CurrentPopulation * BuffManager.Instance.foodConsumptionMultiplier;
        };
    }
    public void ChangeResources(Dictionary<string,float> dict, float multiplier = 1)
    {
        foreach (KeyValuePair<string, float> pair in dict)
        {
            ChangeResource(pair.Key, pair.Value * multiplier);
        }
    }
    public void ChangeResourcesByPercent(Dictionary<string, float> dict, float multiplier = 1)
    {
        foreach (KeyValuePair<string, float> pair in dict)
        {
            ChangeResource(pair.Key, pair.Value * multiplier * GetResourceAmount(pair.Key));
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
        if (GameEventManager.Instance.isGameOver) return;
        foreach (string resource in productionRate.Keys.ToList())
        {
            if(resources[resource] < 0)
            {
                resources[resource] = 0;
                EventManager.Instance.onResourceExhausted(resource);
                if (productionRate[resource] < 0)
                {
                    GameEventManager.Instance.isGameOver = true;
                    if(resource == "food")
                        GameEventManager.Instance.ShowTipByDescription("starve");
                    else if (resource =="water")
                        GameEventManager.Instance.ShowTipByDescription("thirst");
                    else
                    {
                        throw new System.Exception("Resources still being consumed. Something is wrong.");
                    }
                }
                else
                {
                    GameEventManager.Instance.ShowTipByDescription(resource + "Exhaust");
                }
            }
        }
    }
}
