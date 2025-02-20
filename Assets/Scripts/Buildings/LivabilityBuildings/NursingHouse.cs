using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NursingHouse : AutomaticProductionBuilding, ILivability
{
    public int Livability => level;
}
