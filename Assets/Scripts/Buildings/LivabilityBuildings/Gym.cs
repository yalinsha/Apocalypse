using System.Collections.Generic;
using UnityEngine;

public class Gym : AutomaticProductionBuilding, ILivability
{
    readonly List<float> ranges = new()
    {
        6.01f,8.01f,10.01f,12.01f,14.01f
    };//Ó²±àÂë
    public int Livability => IsFunctioning? GetNeighborsInRange(ranges[level - 1]).Count : 0;
}
