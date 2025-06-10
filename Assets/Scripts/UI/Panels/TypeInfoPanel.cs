using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TypeInfoPanel : BasePanel<TypeInfoPanel>
{
    public TMP_Text title, description, info;
    public void UpdateInfo(string buildingName)
    {
        BuildingInfoPro buildingInfoPro = BuildingInfoManager.Instance.GetBuildingInfo(buildingName);
        title.text = buildingInfoPro.buildingInfo.nameChinese;
        description.text = buildingInfoPro.buildingInfo.description;
        info.text = buildingInfoPro.buildingInfo.cost;
    }
    private void Start()
    {
        Hide();
    }
}
