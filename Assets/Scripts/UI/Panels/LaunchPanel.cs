using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LaunchPanel : BasePanel<LaunchPanel>
{
    public TMP_Text nuclear, life, shell, chip;
    public Button launch;
    private void Start()
    {
        launch.onClick.AddListener(() =>
        {
            Extensions.distance = (ResourceManager.Instance.GetResourceAmount("chip_part") * 25 + ResourceManager.Instance.GetResourceAmount("nuclear_part") * 10) * (ResourceManager.Instance.GetResourceAmount("life_part") + ResourceManager.Instance.GetResourceAmount("shell_part") * 5) / 100;
            SceneManager.LoadScene("EndScene");
        });
        Hide();
    }
    void UpdateInfo()
    {
        nuclear.text = ((int)ResourceManager.Instance.GetResourceAmount("nuclear_part")) + " / 10";
        life.text = ((int)ResourceManager.Instance.GetResourceAmount("life_part")) + " / " + PopulationManager.Instance.CurrentPopulation;
        shell.text = ((int)ResourceManager.Instance.GetResourceAmount("shell_part")) + " / 20";
        chip.text = ((int)ResourceManager.Instance.GetResourceAmount("chip_part")) + " / 4";
        launch.interactable = IsCanLaunch();
    }
    private void Update()
    {
        UpdateInfo();
    }
    bool IsCanLaunch()
    {
        return (int)ResourceManager.Instance.GetResourceAmount("nuclear_part") >= 10
            && (int)ResourceManager.Instance.GetResourceAmount("life_part") >= PopulationManager.Instance.CurrentPopulation
            && (int)ResourceManager.Instance.GetResourceAmount("shell_part") >= 20
            && (int)ResourceManager.Instance.GetResourceAmount("chip_part") >= 4;
    }
}
