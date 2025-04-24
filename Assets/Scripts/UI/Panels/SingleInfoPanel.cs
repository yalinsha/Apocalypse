using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SingleInfoPanel : BasePanel<SingleInfoPanel>
{
    public TMP_Text title, info;
    public GameObject regular, underConstruction;
    public TMP_Text worker, timeLeft;
    public GameObject productionMultiplier, consumptionMultiplier;
    public TMP_Text multiplier;
    public GameObject stationable, automatic, permanent;
    public TMP_Text stationed;
    public Button add, minus;
    public Toggle toggleFunctioning;
    public GameObject functioning, idle;
    BaseBuilding currentBuilding = null;
    public void UpdateInfo(BaseBuilding building)
    {
        if (building == null) return;
        currentBuilding = building;
        title.text = building.buildingInfoPro.buildingInfo.nameChinese;
        info.text = building.buildingInfoPro.buildingInfo.effect;

        if (building is IStationable stationableBuilding)
        {
            stationable.SetActive(true);
            automatic.SetActive(false);
            permanent.SetActive(false);
            add.onClick.RemoveAllListeners();
            add.onClick.AddListener(() =>
            {
                PopulationManager.Instance.IncreaseStation(stationableBuilding);
            });
            minus.onClick.RemoveAllListeners();
            minus.onClick.AddListener(() =>
            {
                PopulationManager.Instance.DecreaseStation(stationableBuilding);
            });
        }
        else if (building is AutomaticProductionBuilding automaticBuilding)
        {
            stationable.SetActive(false);
            automatic.SetActive(true);
            permanent.SetActive(false);
            toggleFunctioning.onValueChanged.RemoveAllListeners();
            toggleFunctioning.onValueChanged.AddListener((value) =>
            {
                automaticBuilding.IsFunctioning = value;
            });
        }
        else
        {
            stationable.SetActive(false);
            automatic.SetActive(false);
            permanent.SetActive(true);
        }

        if (building.isUpgrading)
        {
            underConstruction.SetActive(true);
            regular.SetActive(false);
            worker.text = (building.buildingInfoPro.buildingInfo.sizeX * building.buildingInfoPro.buildingInfo.sizeY).ToString();
            return;
        }

        if(building is ProductionBuilding production)//暂未考虑仅消耗的情况
        {
            productionMultiplier.SetActive(true);
        }
        else
        {
            productionMultiplier.SetActive(false);
            multiplier.text = "";
        }
        
        UpdateDynamicInfo();
    }
    public void UpdateDynamicInfo()
    {
        if(currentBuilding == null) return;
        if (currentBuilding.isUpgrading)
        {
            underConstruction.SetActive(true);
            regular.SetActive(false);
        }
        else
        {
            underConstruction.SetActive(false);
            regular.SetActive(true);
            if (currentBuilding is ProductionBuilding production)//暂未考虑仅消耗的情况
            {
                multiplier.text = production.multiplier * 100 + "%";
            }
            if(currentBuilding is IStationable stationableBuilding)
            {
                stationed.text = stationableBuilding.StationedCount.ToString();
                add.interactable = (stationableBuilding.StationedCount < PopulationManager.Instance.AvailablePopulation && stationableBuilding.StationedCount < stationableBuilding.MaxStationCount);
                minus.interactable = (stationableBuilding.StationedCount > 0);
            }
            else if(currentBuilding is AutomaticProductionBuilding automaticBuilding)
            {
                toggleFunctioning.isOn = automaticBuilding.IsFunctioning;
                functioning.SetActive(automaticBuilding.IsFunctioning);
                idle.SetActive(!automaticBuilding.IsFunctioning);
            }
        }
    }
    private void Start()
    {
        Debug.Log("SingleInfoPanel Start Called.");
        EventManager.Instance.onStatusChanged += () =>
        {
            Debug.Log("Updating ...");
            UpdateDynamicInfo();
        };//检查一下是否后加
        Hide();
    }
    private void Update()
    {
        if (currentBuilding == null) return;
        if (currentBuilding.isUpgrading)
        {
            timeLeft.text = TimeSpan.FromSeconds(currentBuilding.TimeLeft).ToString(@"mm\:ss");
        }
    }
}
