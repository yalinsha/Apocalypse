using System.Collections.Generic;
using UnityEngine;

public class PopulationManager : MonoBehaviour
{
    public static PopulationManager Instance
    {
        get; private set;
    }
    void Awake()
    {
        Instance = this;
    }
    public const float waterConsumption = 2;
    public const float foodConsumption = 1;
    float currentPopulation = 60;
    public int CurrentPopulation
    {
        get
        {
            return Mathf.FloorToInt(currentPopulation);
        }
    }
    public int maxPopulation = 80;
    private float rate = 0.01f;
    public float Rate
    {
        get
        {
            return rate;
        }
        set
        {
            rate = value < 0 ? 0 : value;
        }
    }
    int stationedPopulation = 0;
    //const float increaseInterval = 30;
    //float timeUntilIncrease = increaseInterval;
    public int AvailablePopulation => CurrentPopulation - stationedPopulation;
    private void Start()
    {
        EventManager.Instance.onStartUpgrade += (BaseBuilding building) =>
        {
            Debug.Log("onStartUpgrade in PopulationManager");
            stationedPopulation += building.buildingInfoPro.buildingInfo.workersRequired;
        };
        EventManager.Instance.onFinishUpgrade += (BaseBuilding building) =>
        {
            stationedPopulation -= building.buildingInfoPro.buildingInfo.workersRequired;
            if (building is IStationable stationable)
            {
                stationedPopulation -= stationable.StationedCount;//先全部撤出
                int newStationedCount = Mathf.Min(stationable.MaxStationedCount, AvailablePopulation);//再决定派驻人数
                stationable.StationedCount = newStationedCount;
                stationedPopulation += newStationedCount;
            }
            else if(building is AutomaticProductionBuilding automatic)
            {
                automatic.IsFunctioning = true;
            }
            if(building.buildingName == "Apartment")
            {
                maxPopulation += 10;//硬编码处
            }
            if (building.buildingName == "CellRepair")
            {
                Rate += 0.01f;//硬编码处
            }
        };
        //EventManager.Instance.onDemolish += (BaseBuilding building) =>
        //{
        //    if (building is IStationable stationable)
        //    {
        //        stationedPopulation -= stationable.StationedCount;
        //        stationable.StationedCount = 0;//全部撤出
        //    }
        //    if (building.buildingName == "Apartment")
        //    {
        //        maxPopulation -= 20;//硬编码处
        //    }
        //    if (building.buildingName == "CellRepair")
        //    {
        //        rate -= 0.05f;//硬编码处
        //    }
        //};
        EventManager.Instance.onSolarStormStart += () =>
        {
            foreach(var building in MapManager.Instance.buildings)
            {
                if(building is IStationable stationable)
                {
                    DecreaseStationToZero(stationable);
                }
            }
        };
        EventManager.Instance.onSolarStormEnd += () =>
        {
            foreach (var building in MapManager.Instance.buildings)
            {
                if (building is IStationable stationable)
                {
                    RestoreStation(stationable);
                }
            }
        };
    }
    int minuteIndex = 1;
    void Update()
    {
        if(SolarStormManager.Instance.time >= minuteIndex * 60)
        {
            PopulationIncrease();
            ++minuteIndex;
        }
        ResourceManager.Instance.ChangeResource("water", -CurrentPopulation * Time.deltaTime * waterConsumption * BuffManager.Instance.waterConsumptionMultiplier);
        ResourceManager.Instance.ChangeResource("food", -CurrentPopulation * Time.deltaTime * foodConsumption * BuffManager.Instance.foodConsumptionMultiplier);
    }
    void PopulationIncrease()
    {
        if (LivabilityManager.Instance.Livability >= 0)
        {
            currentPopulation *= Rate + 1;
            if(currentPopulation > maxPopulation) currentPopulation = maxPopulation;
            EventManager.Instance.onStatusChanged();
        }
    }
    public void IncreaseStation(IStationable building)//向某一建筑增派人员
    {
        ++building.StationedCount;
        ++stationedPopulation;
        EventManager.Instance.onStatusChanged();
    }
    public void DecreaseStation(IStationable building)
    {
        --building.StationedCount;
        --stationedPopulation;
        EventManager.Instance.onStatusChanged();
    }
    public void DecreaseStationToZero(IStationable building)
    {
        building.LastStationedCount = building.StationedCount;
        stationedPopulation -= building.StationedCount;
        building.StationedCount = 0;
    }
    public void DecreaseStationToOne(IStationable building)
    {
        if(building.StationedCount > 1)
        {
            stationedPopulation -= building.StationedCount - 1;
            building.StationedCount = 1;
        }
    }
    public void RestoreStation(IStationable building)
    {
        stationedPopulation += building.LastStationedCount;
        building.StationedCount = building.LastStationedCount;
    }
}
