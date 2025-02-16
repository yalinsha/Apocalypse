using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//以Xml形式存储的信息的类原型
public class BuildingInfo
{
    public string buildingName;
    public int type;
    public string restriction;//可建筑于其上的地形种类
    public int extraRestrictionId;
    public string neighborBonus;
    public int sizeX;
    public int sizeY;
    public string massProduction;
    public string singleProduction;
    public int livabilityBoost;
    public int maxCount;
    public int maxLevel;
    public string upgradeCost;
    public string upgradeDuration;
    public float stationBonus;
    public bool needStation;
    public string description;
    public string effect;
    public string upgradeRestriction;
    public int group;
    public string nameChinese;
}
public class BuildingInfoCollection
{
    public List<BuildingInfo> buildingInfos;
}
/// <summary>
/// 经过预处理的建筑信息
/// </summary>
public class BuildingInfoPro
{
    public BuildingInfo buildingInfo;
    public int restrictionMask;
    public Dictionary<string, float> neighborBonusDict;
    public List<Dictionary<string, float>> massProductionList;
    public List<Dictionary<string, float>> singleProductionList;
    public List<Dictionary<string, float>> upgradeCostList;
    public List<float> upgradeDurationList;
    public List<Dictionary<string, int>> upgradeRestrictionList;
    public BuildingInfoPro(BuildingInfo info)
    {
        buildingInfo = info;
        string[] parts = buildingInfo.restriction.Split(';');
        restrictionMask = 0;
        //右数第0-5位分别代表草地、土地、焦土、沙漠、水体、裸岩上是否可建筑
        foreach (string part in parts)
        {
            restrictionMask |= 1 << (int.Parse(part) - 1);
        }
        neighborBonusDict = new Dictionary<string, float>();
        parts = buildingInfo.neighborBonus.Split(",");
        if (parts[0] != "")
            neighborBonusDict[parts[0]] = float.Parse(parts[1]);
        massProductionList = new List<Dictionary<string, float>>();
        parts = buildingInfo.massProduction.Split(";");
        foreach (string part in parts)
        {
            Dictionary<string, float> dic = new Dictionary<string, float>();
            string[] pairs = part.Split(",");
            foreach (string pair in pairs)
            {
                string[] element = pair.Split("|");
                if (element[0] != "")
                    dic[element[0]] = float.Parse(element[1]);
            }
            massProductionList.Add(dic);
        }
        singleProductionList = new List<Dictionary<string, float>>();
        parts = buildingInfo.singleProduction.Split(";");
        foreach (string part in parts)
        {
            Dictionary<string, float> dic = new Dictionary<string, float>();
            string[] pairs = part.Split(",");
            foreach (string pair in pairs)
            {
                string[] element = pair.Split("|");
                if (element[0] != "")
                    dic[element[0]] = float.Parse(element[1]);
            }
            singleProductionList.Add(dic);
        }
        upgradeCostList = new List<Dictionary<string, float>>();
        parts = buildingInfo.upgradeCost.Split(";");
        foreach (string part in parts)
        {
            Dictionary<string, float> dic = new Dictionary<string, float>();
            string[] pairs = part.Split(",");
            foreach (string pair in pairs)
            {
                string[] element = pair.Split("|");
                if (element[0] != "")
                    dic[element[0]] = float.Parse(element[1]);
            }
            upgradeCostList.Add(dic);
        }
        upgradeDurationList = new List<float>();
        parts = buildingInfo.upgradeDuration.Split(";");
        foreach (string part in parts)
        {
            upgradeDurationList.Add(float.Parse(part));
        }
        upgradeRestrictionList = new List<Dictionary<string, int>>();
        parts = buildingInfo.upgradeRestriction.Split(";");
        foreach (string part in parts)
        {
            Dictionary<string, int> dic = new Dictionary<string, int>();
            string[] pairs = part.Split(",");
            foreach (string pair in pairs)
            {
                string[] element = pair.Split("|");
                if (element[0] != "")
                    dic[element[0]] = int.Parse(element[1]);
            }
            upgradeRestrictionList.Add(dic);
        }
    }
}
/// <summary>
/// 存储从Xml读取的建筑类型信息
/// </summary>
public class BuildingInfoManager : MonoBehaviour
{
    public static BuildingInfoManager Instance
    {
        get; private set;
    }
    private void Awake()
    {
        Instance = this;
    }
    public Dictionary<string, BuildingInfoPro> buildingInfoDict = new();
    // Start is called before the first frame update
    void Start()
    {
        BuildingInfoCollection collection = XmlDataManager.Instance.Load<BuildingInfoCollection>("building");
        foreach (BuildingInfo info in collection.buildingInfos)
        {
            BuildingInfoPro t = new(info);
            buildingInfoDict[info.buildingName] = t;
        }
    }
}
