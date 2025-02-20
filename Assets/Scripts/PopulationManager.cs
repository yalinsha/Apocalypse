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
    public float currentPopulation = 60;
    float maxPopulation = 100;
    public float rate = 0.05f;
    int stationedPopulation = 0;
    const float increaseInterval = 30;
    float timeUntilIncrease = increaseInterval;
    public int AvailablePopulation => Mathf.FloorToInt(currentPopulation) - stationedPopulation;
    private void Start()
    {
        EventManager.Instance.onFinishUpgrade += (BaseBuilding building) =>
        {
            if(building is IStationable stationable)
            {
                stationedPopulation -= stationable.StationedCount;//先全部撤出
                int newStationedCount = Mathf.Min(stationable.MaxStationCount, AvailablePopulation);//再决定派驻人数
                stationable.StationedCount = newStationedCount;
                stationedPopulation += newStationedCount;
            }
            if(building.buildingName == "Apartment")
            {
                maxPopulation += 20;//硬编码处
            }
            if (building.buildingName == "CellRepair")
            {
                rate += 0.05f;//硬编码处
            }
        };
        EventManager.Instance.onDemolish += (BaseBuilding building) =>
        {
            if (building is IStationable stationable)
            {
                stationedPopulation -= stationable.StationedCount;
                stationable.StationedCount = 0;//全部撤出
            }
            if (building.buildingName == "Apartment")
            {
                maxPopulation -= 20;//硬编码处
            }
            if (building.buildingName == "CellRepair")
            {
                rate -= 0.05f;//硬编码处
            }
        };
    }
    void Update()
    {
        timeUntilIncrease -= Time.deltaTime;
        if (timeUntilIncrease < 0)
        {
            PopulationIncrease();
            timeUntilIncrease = increaseInterval;
        }
    }
    void PopulationIncrease()
    {
        if (LivabilityManager.Instance.livability >= 0)
            currentPopulation += rate * currentPopulation * (1 - currentPopulation / maxPopulation);
    }
    public void IncreaseStation(IStationable building)//向某一建筑增派人员
    {
        ++building.StationedCount;
        ++stationedPopulation;
    }
    public void DecreaseStation(IStationable building)
    {
        --building.StationedCount;
        --stationedPopulation;
    }
}
