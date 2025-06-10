using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public struct PlotEventInfo
{
    public string title;
    public string description;
    public string option;
}
public class PlotEventDictionary
{
    public SerializableDictionary<string,PlotEventInfo> events;
}

public struct RegularEventInfo
{
    public int id;
    public string title;
    public string description;
    public string option1, option2, option3;
    public string effect1, effect2, effect3;
}
public class RegularEventCollection
{
    public List<RegularEventInfo> events;
}

//GameEvent是游戏游玩中出现的、提示玩家或需要玩家选择的事件，分为RegularEvent和PlotEvent.
public class GameEventManager : MonoBehaviour
{
    PlotEventDictionary plotEvents;
    RegularEventCollection disasterEvents, socialEvents;
    //float time = 0;
    //int plotIndex = 0;
    const float cd = 31;
    const float randSocial = 7;
    float disasterTimeLeft = cd;
    float socialTimeLeft = cd + 10;
    public bool isGameOver = false;
    List<(float,string)> tips = new List<(float,string)>
    {
        (0,"story1"),
        (0.1f,"story2"),
        (0.1f,"tipBasics1"),
        (0.1f,"tipBasics2"),
        (30,"tipEvent"),
        (30,"tipStorm"),
        (60,"tipStormComing"),
        (80,"tipAdvanced1"),
        (0.1f,"tipAdvanced2"),
    };
    public float timeOn = 1;
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
        plotEvents = XmlDataManager.Instance.Load<PlotEventDictionary>("plot");
        disasterEvents = XmlDataManager.Instance.Load<RegularEventCollection>("disaster");
        socialEvents = XmlDataManager.Instance.Load<RegularEventCollection>("social");
        EventManager.Instance.onGameOver += () =>
        {
            ShowTipByDescription("defeatedByStorm");
        };
        EventManager.Instance.onFinishUpgrade += (building) =>
        {
            if (building is IStationable)
            {
                if (!Extensions.tipStationableShown && building.buildingName != "StarshipCenter")
                {
                    ShowTipByDescription("tipStationable");
                    Extensions.tipStationableShown = true;
                }
            }
            else if (building is AutomaticProductionBuilding)
            {
                if (!Extensions.tipAutomaticShown)
                {
                    ShowTipByDescription("tipAutomatic");
                    Extensions.tipAutomaticShown = true;
                }
            }
            else if (!Extensions.tipPermanentShown)
            {
                ShowTipByDescription("tipPermanent");
                Extensions.tipPermanentShown = true;
            }
        };
        StartCoroutine(ShowTimedTips());
    }
    IEnumerator ShowTimedTips()
    {
        foreach(var t in tips)
        {
            yield return new WaitForSeconds(t.Item1);
            ShowTipByDescription(t.Item2);
        }
    }
    void Update()
    {
        if (isGameOver) return;
        if (SolarStormManager.Instance.IsInStorm) return;

        disasterTimeLeft -= (1 - LivabilityManager.Instance.Livability / 40) * Time.deltaTime;
        if (disasterTimeLeft < 0)
        {
            RegularEventInfo disasterInfo = disasterEvents.events[Random.Range(0, disasterEvents.events.Count)];
            EventManager.Instance.onRegularEvent?.Invoke(disasterInfo);
            disasterTimeLeft = cd;
            disasterEvents.events.Remove(disasterInfo);
            if (disasterEvents.events.Count == 0)
            {
                disasterEvents = XmlDataManager.Instance.Load<RegularEventCollection>("disaster");
            }
        }
        socialTimeLeft -= Time.deltaTime;
        if (socialTimeLeft < 0)
        {
            RegularEventInfo socialInfo = socialEvents.events[Random.Range(0, socialEvents.events.Count)];
            EventManager.Instance.onRegularEvent?.Invoke(socialInfo);
            socialTimeLeft = cd + Random.Range(-randSocial, randSocial);
            socialEvents.events.Remove(socialInfo);
            if (socialEvents.events.Count == 0)
            {
                socialEvents = XmlDataManager.Instance.Load<RegularEventCollection>("social");
            }
        }
    }
    public void AssignEffectToButton(string effect,Button button)
    {
        string[] singleEffects = effect.Split(';');
        foreach (string singleEffect in singleEffects)
        {
            string[] tmp = singleEffect.Split(':');
            switch (tmp[0])
            {
                case "A":
                    button.onClick.AddListener(() => ChangeResource(tmp[1]));
                    break;
                case "B":
                    button.onClick.AddListener(() => ChangeResourceByPercent(tmp[1]));
                    break;
                case "C":
                    button.onClick.AddListener(() => PopulationManager.Instance.Rate += float.Parse(tmp[1]));
                    break;
                case "D":
                    button.onClick.AddListener(() => AddProductionBuff(tmp[1]));
                    break;
                case "E":
                    button.onClick.AddListener(() => BuffManager.Instance.constructionTimeMultiplier *= 1 + float.Parse(tmp[1]));
                    break;
                case "F":
                    button.onClick.AddListener(() => BuffManager.Instance.maxStationedCount += int.Parse(tmp[1]));
                    break;
                case "G":
                    button.onClick.AddListener(() =>
                    {
                        BuffManager.Instance.foodConsumptionMultiplier *= 1 + float.Parse(tmp[1]);
                        EventManager.Instance.onStatusChanged();
                    });
                    break;
                case "H":
                    button.onClick.AddListener(() =>
                    {
                        BuffManager.Instance.waterConsumptionMultiplier *= 1 + float.Parse(tmp[1]);
                        EventManager.Instance.onStatusChanged();
                    });
                    break;
                case "I":
                    button.onClick.AddListener(() =>
                    {
                        Debug.Log(tmp[1]);
                        LivabilityManager.Instance.eventLivability += int.Parse(tmp[1]);
                        EventManager.Instance.onStatusChanged();
                    });
                    break;
                case "J":
                    button.onClick.AddListener(() =>
                    {
                        PopulationManager.Instance.maxPopulation += int.Parse(tmp[1]);
                    });
                    break;
            }
        }
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
    void ChangeResourceByPercent(string effect)
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
        ResourceManager.Instance.ChangeResourcesByPercent(dict);
    }
    public void AddProductionBuff(string effect)
    {
        string[] pairs = effect.Split(',');
        foreach (string pair in pairs)
        {
            string[] element = pair.Split('|');
            if (element[0] != "")
            {
                if (BuffManager.Instance.productionBuffs.ContainsKey(element[0]))
                {
                    BuffManager.Instance.productionBuffs[element[0]] += float.Parse(element[1]);
                }
                else
                {
                    BuffManager.Instance.productionBuffs[element[0]] = float.Parse(element[1]);
                }
            }
        }
        EventManager.Instance.onStatusChanged();
    }
    public void ShowTipByDescription(string des)
    {
        if (plotEvents.events.ContainsKey(des))
        {
            EventManager.Instance.onPlotEvent(plotEvents.events[des]);
        }
    }
    public void EndGame()
    {
        SceneManager.LoadScene("StartScene");
    }
}