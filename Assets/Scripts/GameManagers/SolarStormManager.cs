using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct SolarStormInfo
{
    public float stormTime;
    public float mineDemand;
    public SolarStormInfo(float time,float demand)
    {
        stormTime = time;
        mineDemand = demand;
    }
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
    public float time = 0;
    const float stormDuration = 15;//硬编码
    float StormStart
    {
        get 
        {
            return currentInfo.stormTime;
        }
    }
    public bool IsInStorm
    {
        get; private set;
    } = false;
    /// <summary>
    /// 即将来临或正在经历的风暴数据
    /// </summary>
    public SolarStormInfo currentInfo;
    SolarStormInfoCollection collection;
    int index = 1;//0
    //bool isGameOver = false;

    public float TimeLeft => StormStart - time;

    private void Start()
    {
        collection = XmlDataManager.Instance.Load<SolarStormInfoCollection>("solar");
        currentInfo = new SolarStormInfo(180,200);
    }
    public void StartSolarStorm()
    {
        IsInStorm = true;
        EventManager.Instance.onSolarStormStart();
        EventManager.Instance.onStatusChanged();
    }
    public void EndSolarStorm()
    {
        IsInStorm = false;
        EventManager.Instance.onSolarStormEnd();
        EventManager.Instance.onStatusChanged();
    }
    void CheckGameOver()
    {
        GameEventManager.Instance.isGameOver = (currentInfo.mineDemand > ResourceManager.Instance.GetResourceAmount("mine"));
        if (GameEventManager.Instance.isGameOver)
        {
            EventManager.Instance.onGameOver();
        }
    }

    void Update()
    {
        time += Time.deltaTime;
        if (GameEventManager.Instance.isGameOver) return;
        if (!IsInStorm)
        {
            if (time > StormStart)
            {
                CheckGameOver();
                if (!GameEventManager.Instance.isGameOver)
                {
                    StartSolarStorm();
                }
            }
        }
        else
        {
            ResourceManager.Instance.ChangeResource("electric", - ResourceManager.Instance.GetResourceAmount("electric") * 0.03f * Time.deltaTime);
            if(time > StormStart + stormDuration)
            {
                ++index;
                //currentInfo = collection.infos[index];
                currentInfo = new SolarStormInfo(index * 300 - 120, 200 * index * index);
                EndSolarStorm();
            }
        }
    }
}
