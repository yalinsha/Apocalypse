using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HeadPanel : BasePanel<HeadPanel>
{
    public TMP_Text figureWater, figureFood, figureOre, figureOil, figureElectricity, figurePopulation;
    public Image solarImage;
    public Sprite normalSun, stormingSun;
    public TMP_Text figureTimeLeft;
    public Button launch;
    public bool detailedWater = false, detailedFood = false, detailedOre = false, detailedOil = false, detailedElectricity = false, detailedPopulation = false, detailedStorm = false;
    private void Start()
    {
        launch.onClick.AddListener(() =>
        {
            if (LaunchPanel.Instance.gameObject.activeInHierarchy)
            {
                LaunchPanel.Instance.Hide();
            }
            else
            {
                LaunchPanel.Instance.Show();
            }
        });
    }
    void UpdateInfo()
    {
        figureWater.text = detailedWater ? Extensions.StringForRateDisplay(ResourceManager.Instance.productionRate["water"]) : Extensions.StringForResourceDisplay(ResourceManager.Instance.GetResourceAmount("water"));
        figureFood.text = detailedFood ? Extensions.StringForRateDisplay(ResourceManager.Instance.productionRate["food"]) : Extensions.StringForResourceDisplay(ResourceManager.Instance.GetResourceAmount("food"));
        figureOre.text = detailedOre ? Extensions.StringForRateDisplay(ResourceManager.Instance.productionRate["mine"]) : Extensions.StringForResourceDisplay(ResourceManager.Instance.GetResourceAmount("mine"));
        figureOil.text = detailedOil ? Extensions.StringForRateDisplay(ResourceManager.Instance.productionRate["oil"]) : Extensions.StringForResourceDisplay(ResourceManager.Instance.GetResourceAmount("oil"));
        figureElectricity.text = detailedElectricity ? Extensions.StringForRateDisplay(ResourceManager.Instance.productionRate["electric"]) : Extensions.StringForResourceDisplay(ResourceManager.Instance.GetResourceAmount("electric"));
        figurePopulation.text = detailedPopulation ? (PopulationManager.Instance.Rate * 100).ToString("0.00") + "% | " + PopulationManager.Instance.maxPopulation : PopulationManager.Instance.AvailablePopulation + " / " + PopulationManager.Instance.CurrentPopulation;
        solarImage.sprite = SolarStormManager.Instance.IsInStorm ? stormingSun : normalSun;
        figureTimeLeft.text = detailedStorm ? SolarStormManager.Instance.currentInfo.mineDemand.ToString() : TimeSpan.FromSeconds(SolarStormManager.Instance.TimeLeft > 0 ? SolarStormManager.Instance.TimeLeft : 0).ToString(@"mm\:ss");
    }
    private void Update()
    {
        UpdateInfo();
    }
    public void OnEnterOrExit(string text, bool value)
    {
        switch (text)
        {
            case "water":
                detailedWater = value; break;
            case "food":
                detailedFood = value; break;
            case "electric":
                detailedElectricity = value; break;
            case "ore":
                detailedOre = value; break;
            case "oil":
                detailedOil = value; break;
            case "population":
                detailedPopulation = value; break;
            case "storm":
                detailedStorm = value; break;
        }
    }
    public void OnEnterText(string text)
    {
        OnEnterOrExit(text, true);
    }
    public void OnExitText(string text)
    {
        OnEnterOrExit(text, false);
    }
}
