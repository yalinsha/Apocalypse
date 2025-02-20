using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EcoGarden : BaseBuilding, ILivability
{
    public int Livability {
        get
        {
            return 10;//Ó²±àÂë
        }
    }
}