using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public int livability;
    public HashSet<ILivability> livabilityBuildings = new();

    private void Start()
    {
        EventManager.Instance.onStartConstruct += (BaseBuilding building) =>
        {
            eventLivability += BuildingInfoManager.Instance.buildingInfoDict[building.buildingName].buildingInfo.livabilityBoost;
            Recalculate();
        };
    }
    public void Recalculate()
    {
        livability = 5;//宜居度基准值
        foreach (var building in livabilityBuildings)
        {
            livability += building.Livability;
        }
        livability += eventLivability;
        livability -= Mathf.FloorToInt(PopulationManager.Instance.currentPopulation / 15);//人口减少宜居度
        livability = Mathf.Clamp(livability, -20, 20);
        //有个全局更新？
    }
}
