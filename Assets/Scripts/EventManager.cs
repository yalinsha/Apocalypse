using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
    public UnityAction<Dictionary<string, float>,float> onResourceChanged;
}
