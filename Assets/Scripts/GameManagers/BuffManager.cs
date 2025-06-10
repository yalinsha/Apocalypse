using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffManager : MonoBehaviour
{
    public static BuffManager Instance {  get; private set; }
    private void Awake()
    {
        Instance = this;
    }
    public float waterConsumptionMultiplier = 1;
    public float foodConsumptionMultiplier = 1;
    public Dictionary<string, float> productionBuffs = new();
    public float constructionTimeMultiplier = 1;
    public int maxStationedCount = 5;
}