using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TipPanel : BasePanel<TipPanel>
{
    public TMP_Text title;
    public TMP_Text description;
    public TMP_Text option;
    public Button button;
    private void Start()
    {
        button.onClick.AddListener(Hide);
        EventManager.Instance.onPlotEvent += ShowTip;
        Hide();
    }
    public override void Hide()
    {
        base.Hide();
        GameEventManager.Instance.timeOn = 1;
        Time.timeScale = GameEventManager.Instance.timeOn * OperationManager.Instance.timeRate;
    }
    public override void Show()
    {
        base.Show();
        GameEventManager.Instance.timeOn = 0;
        Time.timeScale = GameEventManager.Instance.timeOn * OperationManager.Instance.timeRate;
    }
    public void ShowTip(PlotEventInfo info)
    {
        title.text = info.title;
        description.text = info.description;
        option.text = info.option;
        if (GameEventManager.Instance.isGameOver)
        {
            button.onClick.AddListener(() =>
            {
                GameEventManager.Instance.EndGame();
            });
        }
        Show();
    }
}
