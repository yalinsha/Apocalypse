using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeadPanel : BasePanel<HeadPanel>
{
    public TMP_Text figureWater, figureFood, figureOre, figureOil, figureElectricity, figurePopulation;
    public Image solarImage;
    public Sprite normalSun, stormingSun;
    public TMP_Text figureTimeLeft;
    void UpdateInfo()
    {
        figureWater.text = Extensions.StringForResourceDisplay(ResourceManager.Instance.GetResourceAmount("water"));
        figureFood.text = Extensions.StringForResourceDisplay(ResourceManager.Instance.GetResourceAmount("food"));
        figureOre.text = Extensions.StringForResourceDisplay(ResourceManager.Instance.GetResourceAmount("mine"));
        figureOil.text = Extensions.StringForResourceDisplay(ResourceManager.Instance.GetResourceAmount("oil"));
        figureElectricity.text = Extensions.StringForResourceDisplay(ResourceManager.Instance.GetResourceAmount("electric"));
        figurePopulation.text = PopulationManager.Instance.AvailablePopulation + " / " + (int)PopulationManager.Instance.currentPopulation;
        solarImage.sprite = SolarStormManager.Instance.IsInStorm ? stormingSun : normalSun;
        TimeSpan time = TimeSpan.FromSeconds(SolarStormManager.Instance.TimeLeft > 0 ? SolarStormManager.Instance.TimeLeft : 0);
        figureTimeLeft.text = time.ToString(@"mm\:ss");
    }
    private void Update()
    {
        UpdateInfo();
    }
}
