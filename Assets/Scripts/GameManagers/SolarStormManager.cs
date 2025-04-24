using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SolarStormInfo
{
    public float stormTime;
    public float foodDemand;
    public float waterDemand;
    public float mineDemand;
    public float oilDemand;
    public float electricDemand;
    public float carbonDemand;
    public float tiDemand;
    public float chipDemand;
}
public class SolarStormInfoCollection
{
    public List<SolarStormInfo> infos;
}
/// <summary>
/// 负责太阳风暴的逻辑。
/// </summary>
public class SolarStormManager : MonoBehaviour
{
    public static SolarStormManager Instance
    {
        get; private set;
    }
    void Awake()
    {
        Instance = this;
    }
    float time;
    float unscaledTime;
    float stormDuration = 5;//
    float stormStart;
    public bool IsInStorm
    {
        get; private set;
    } = false;

    SolarStormInfo info;
    SolarStormInfoCollection collection;
    int index = 0;
    bool isGameOver = false;

    public float TimeLeft => stormStart - time;

    private void Start()
    {
        collection = XmlDataManager.Instance.Load<SolarStormInfoCollection>("solar");
        stormStart = collection.infos[0].stormTime;
    }
    public void StartSolarStorm()
    {
        Time.timeScale = 0;
        IsInStorm = true;
        EventManager.Instance.onSolarStormStart();
    }
    void CheckGameOver()
    {
        isGameOver = (info.foodDemand > ResourceManager.Instance.GetResourceAmount("food")
        || info.waterDemand > ResourceManager.Instance.GetResourceAmount("water")
        || info.mineDemand > ResourceManager.Instance.GetResourceAmount("mine")
        || info.oilDemand > ResourceManager.Instance.GetResourceAmount("oil")
        || info.electricDemand > ResourceManager.Instance.GetResourceAmount("electric")
        || info.carbonDemand > ResourceManager.Instance.GetResourceAmount("carbon")
        || info.tiDemand > ResourceManager.Instance.GetResourceAmount("ti")
        || info.chipDemand > ResourceManager.Instance.GetResourceAmount("chip"));
        if (isGameOver)
        {
            EventManager.Instance.onGameOver();
        }
    }

    void Update()
    {
        if (isGameOver) return;
        if (!IsInStorm)
        {
            time += Time.deltaTime;

            if (time > stormStart)
            {
                info = collection.infos[index];
                StartSolarStorm();
            }
        }
        else
        {
            unscaledTime += Time.unscaledDeltaTime;
            if (unscaledTime > stormDuration)
            {
                IsInStorm = false;
                EventManager.Instance.onSurvive();
                ++index;
                stormStart = collection.infos[index].stormTime;
                unscaledTime = 0;
            }
            ResourceManager.Instance.ChangeResource("food", -info.foodDemand / stormDuration * Time.unscaledDeltaTime);
            ResourceManager.Instance.ChangeResource("water", -info.waterDemand / stormDuration * Time.unscaledDeltaTime);
            ResourceManager.Instance.ChangeResource("mine", -info.mineDemand / stormDuration * Time.unscaledDeltaTime);
            ResourceManager.Instance.ChangeResource("oil", -info.oilDemand / stormDuration * Time.unscaledDeltaTime);
            ResourceManager.Instance.ChangeResource("electric", -info.electricDemand / stormDuration * Time.unscaledDeltaTime);
            ResourceManager.Instance.ChangeResource("carbon", -info.carbonDemand / stormDuration * Time.unscaledDeltaTime);
            ResourceManager.Instance.ChangeResource("ti", -info.tiDemand / stormDuration * Time.unscaledDeltaTime);
            ResourceManager.Instance.ChangeResource("chip", -info.chipDemand / stormDuration * Time.unscaledDeltaTime);
            CheckGameOver();
        }
    }
}
