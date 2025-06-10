using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// Event是建筑升级、拆除等游戏操作对应的事件，是编程意义上的事件。
/// </summary>
public class EventManager : MonoBehaviour
{
    public static EventManager Instance
    {
        get; private set;
    }
    private void Awake()
    {
        Instance = this;
    }
    public UnityAction<BaseBuilding> onStartConstruct, onStartUpgrade, onFinishUpgrade, onDemolish;
    /// <summary>
    /// 发生了建筑升级（含落成）、拆除、人员调度、启用停用、宜居度改变等事件后，所有建筑重新计算生产倍率，全局重新计算宜居度、各资源生产率。
    /// </summary>
    public UnityAction onStatusChanged;
    public UnityAction onVisibilityUpdated, onBuildabilityUpdated;
    public UnityAction<PlotEventInfo> onPlotEvent;
    public UnityAction<RegularEventInfo> onRegularEvent;
    public UnityAction onSolarStormStart, onGameOver, onSolarStormEnd;
    public UnityAction onLivabilityChanged;
    public UnityAction<string> onResourceExhausted;
}
