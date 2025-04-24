using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingPanel : BasePanel<BuildingPanel>
{
    public RectTransform content;
    public GameObject buildingButtonPrefab;
    Dictionary<Button,BuildingButton> buildingButtonDict = new();
    int currentTabIndex = -1;
    List<List<string>> buildingNames = new()
    {
        new()
        {
            "Corn","WaterStation","Mining","OilWell","CSolarPlant","SolarPlant","FSolarPlant","NuclearPlant","Refinery","Compression","BiomassPlant"
        },
        new()
        {
            "AirTower","Apartment","CellRepair","Magnetic","MantleSampling","Gym","NursingHouse","ResearchHouse","EcoGarden","HighLab","AILab","RobotFactory","LAN"
        },
        new()
        {
            "EnvironmentCenter","NuclearManufactor","QuantumCenter","RadiatLab","RocketBase"
        }
    };
    
    void UpdateInfo()
    {
        foreach(var pair in buildingButtonDict)
        {
            pair.Key.interactable = MapManager.Instance.CanChoose(pair.Value.BuildingName);
        }
    }
    public void UpdateTab(int index)
    {
        if (currentTabIndex == index)
        {
            return;
        }
        currentTabIndex = index;
        for(int i = content.childCount - 1; i >= 0; --i)
        {
            Destroy(content.GetChild(i).gameObject);
        }
        content.DetachChildren();
        buildingButtonDict.Clear();
        foreach (string s in buildingNames[currentTabIndex])
        {
            GameObject gameObject = Instantiate(buildingButtonPrefab,content);
            BuildingButton buildingButton = gameObject.GetComponent<BuildingButton>();
            buildingButton.Initialize(s);
            buildingButtonDict.Add(gameObject.GetComponent<Button>(),buildingButton);
        }
        content.localPosition = new Vector3(0, content.localPosition.y, content.localPosition.z);
    }
    public void GoLeft()
    {
        content.Translate(new Vector3(50,0,0), Space.Self);
    }
    public void GoRight()
    {
        content.Translate(new Vector3(-50, 0, 0), Space.Self);
    }

    private void Start()
    {
        UpdateTab(0);
    }
    private void Update()
    {
        UpdateInfo();
    }
}
