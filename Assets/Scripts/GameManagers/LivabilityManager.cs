using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILivability
{
    public int Livability { get; }
}
public class LivabilityManager : MonoBehaviour
{
    public static LivabilityManager Instance
    {
        get; private set;
    }
    private void Awake()
    {
        Instance = this;
    }

    public int eventLivability = 0;
    public int Livability
    {
        get;
        private set;
    }
    readonly HashSet<ILivability> livabilityBuildings = new();

    private void Start()
    {
        EventManager.Instance.onStartConstruct += (BaseBuilding building) =>
        {
            eventLivability += BuildingInfoManager.Instance.GetBuildingInfo(building.buildingName).buildingInfo.livabilityBoost;
        };
        EventManager.Instance.onFinishUpgrade += (BaseBuilding building) =>
        {
            if(building is ILivability livabilityBuilding)
            {
                livabilityBuildings.Add(livabilityBuilding);
            }
        };
        EventManager.Instance.onDemolish += (BaseBuilding building) =>
        {
            if (building is ILivability livabilityBuilding)
            {
                livabilityBuildings.Remove(livabilityBuilding);
            }
        };
    }
    public void RecalculateLivability()
    {
        Livability = 5;//宜居度基准值
        foreach (var building in livabilityBuildings)
        {
            Livability += building.Livability;
        }
        Livability += eventLivability;
        Livability -= Mathf.FloorToInt(PopulationManager.Instance.currentPopulation / 15);//人口减少宜居度
        Livability = Mathf.Clamp(Livability, -20, 20);
        EventManager.Instance.onLivabilityChanged();
    }
}
