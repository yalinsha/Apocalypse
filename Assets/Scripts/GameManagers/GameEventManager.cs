using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlotEventInfo
{
    public string title;
    public string description;
    public string option;
    public float occurrenceTime;
}
public class PlotEventCollection
{
    public List<PlotEventInfo> events;
}

public class RegularEventInfo
{
    public int id;
    public string title;
    public string description;
    public string option1, option2, option3;
    public string effect1, effect2, effect3;
    public int unlock;
}
public class RegularEventCollection
{
    public List<RegularEventInfo> events;
}

//GameEvent是游戏游玩中出现的、提示玩家或需要玩家选择的事件，分为RegularEvent和PlotEvent.
public class GameEventManager : MonoBehaviour
{
    PlotEventCollection plotEvents;
    RegularEventCollection disasterEvents, socialEvents;
    float time = 0;
    int plotIndex = 0;
    float cdDisaster = 140;
    float cdSocial = 120;
    public static GameEventManager Instance
    {
        get; private set;
    }
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        plotEvents = XmlDataManager.Instance.Load<PlotEventCollection>("plot");
        disasterEvents = XmlDataManager.Instance.Load<RegularEventCollection>("disaster");
        socialEvents = XmlDataManager.Instance.Load<RegularEventCollection>("social");
    }
    void Update()
    {
        time += Time.deltaTime;
        if (plotIndex < plotEvents.events.Count && time > plotEvents.events[plotIndex].occurrenceTime)
        {
            EventManager.Instance.onPlotEvent?.Invoke(plotEvents.events[plotIndex]);
            ++plotIndex;
        }
        cdDisaster -= (1 - LivabilityManager.Instance.Livability / 40) * Time.deltaTime;
        if (cdDisaster < 0)
        {
            RegularEventInfo disasterInfo = disasterEvents.events[Random.Range(0, disasterEvents.events.Count)];
            EventManager.Instance.onRegularEvent?.Invoke(disasterInfo);
            cdDisaster = 180;
            disasterEvents.events.Remove(disasterInfo);
            if (disasterEvents.events.Count == 0)
            {
                disasterEvents = XmlDataManager.Instance.Load<RegularEventCollection>("disaster");
            }
        }
        cdSocial -= Time.deltaTime;
        if (cdSocial < 0)
        {
            RegularEventInfo socialInfo = socialEvents.events[Random.Range(0, socialEvents.events.Count)];
            EventManager.Instance.onRegularEvent?.Invoke(socialInfo);
            cdSocial = 180 + Random.Range(-30f, 30f);
            socialEvents.events.Remove(socialInfo);
            if (socialEvents.events.Count == 0)
            {
                socialEvents = XmlDataManager.Instance.Load<RegularEventCollection>("social");
            }
        }
    }
    public List<UnityAction> TranslateEffect(string effect)
    {
        List<UnityAction> list = new();
        string[] singleEffects = effect.Split(';');
        foreach (string singleEffect in singleEffects)
        {
            string[] tmp = singleEffect.Split(':');
            switch (tmp[0])
            {
                case "A":
                    list.Add(() => ChangeResource(tmp[1]));
                    break;
                case "B":
                    list.Add(() =>
                    {
                        LivabilityManager.Instance.eventLivability += int.Parse(tmp[1]);
                        EventManager.Instance.onStatusChanged();
                    });
                    break;
                case "C":
                    list.Add(() =>
                    {
                        PopulationManager.Instance.rate += float.Parse(tmp[1]);
                    });
                    break;
                case "D":
                    string[] element = tmp[1].Split('|');
                    list.Add(() =>
                    {
                        //触发罢工，先不写了
                    });
                    break;
                case "E":
                    list.Add(() =>
                    {
                        ResourceManager.Instance.freeUpgradeDict[tmp[1]] = true;
                    });
                    break;
            }
        }
        return list;
    }
    public bool CanChoose(string effect)
    {
        string[] singleEffects = effect.Split(';');
        foreach (string singleEffect in singleEffects)
        {
            string[] tmp = singleEffect.Split(':');
            if (tmp[0] == "A" && !CanChangeResource(tmp[1]))
            {
                return false;
            }
        }
        return true;
    }
    bool CanChangeResource(string changes)
    {
        string[] pairs = changes.Split(',');
        foreach (string pair in pairs)
        {
            string[] element = pair.Split('|');
            if (element[0] != "" && ResourceManager.Instance.GetResourceAmount(element[0]) + float.Parse(element[1]) < 0)
            {
                return false;
            }
        }
        return true;
    }
    void ChangeResource(string effect)
    {
        string[] pairs = effect.Split(',');
        Dictionary<string, float> dict = new();
        foreach (string pair in pairs)
        {
            string[] element = pair.Split('|');
            if (element[0] != "")
            {
                dict[element[0]] = float.Parse(element[1]);
            }
        }
        ResourceManager.Instance.ChangeResources(dict);
    }
}