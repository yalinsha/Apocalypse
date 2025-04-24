using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingButton : MonoBehaviour
{
    public string BuildingName
    {
        get;
        private set;
    }
    public void Initialize(string buildingName)
    {
        this.BuildingName = buildingName;
        GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/Buildings/" + buildingName);
    }
    public void OnClick()
    {
        OperationManager.Instance.EnterConstructionMode(BuildingName);
        MapRenderer.Instance.EnterConstructionMode(BuildingName);
    }
    public void OnPointerEnter()
    {
        TypeInfoPanel.Instance.UpdateInfo(BuildingName);
        TypeInfoPanel.Instance.Show();
    }
    public void OnPointerExit()
    {
        TypeInfoPanel.Instance.Hide();
    }
}
