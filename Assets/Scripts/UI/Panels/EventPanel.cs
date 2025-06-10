using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventPanel : BasePanel<EventPanel>
{
    public TMP_Text title;
    public TMP_Text description;
    public Image image;
    public Button option1, option2, option3;
    public TMP_Text text1, text2, text3;
    private void Start()
    {
        EventManager.Instance.onRegularEvent += ShowEvent;
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
    public void ShowEvent(RegularEventInfo info)
    {
        title.text = info.title;
        description.text = info.description;
        image.sprite = Resources.Load<Sprite>("Sprites/Events/" + info.id);
        text1.text = info.option1;
        text2.text = info.option2;
        text3.text = info.option3;
        option1.onClick.RemoveAllListeners();
        option2.onClick.RemoveAllListeners();
        option3.onClick.RemoveAllListeners();
        option1.onClick.AddListener(EffectManager.Instance.PlayClickSound);
        option2.onClick.AddListener(EffectManager.Instance.PlayClickSound);
        option3.onClick.AddListener(EffectManager.Instance.PlayClickSound);
        GameEventManager.Instance.AssignEffectToButton(info.effect1, option1);
        GameEventManager.Instance.AssignEffectToButton(info.effect2, option2);
        GameEventManager.Instance.AssignEffectToButton(info.effect3, option3);
        option1.onClick.AddListener(Hide);
        option2.onClick.AddListener(Hide);
        option3.onClick.AddListener(Hide);
        option1.interactable = GameEventManager.Instance.CanChoose(info.effect1);
        option2.interactable = GameEventManager.Instance.CanChoose(info.effect2);
        option3.interactable = GameEventManager.Instance.CanChoose(info.effect3);
        Show();
    }
}
