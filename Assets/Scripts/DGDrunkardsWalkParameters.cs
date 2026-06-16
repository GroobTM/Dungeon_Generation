using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DGDrunkardsWalkParameters
{
    public int StepCount = 0;
    public float MaxGridFill = 1f;
    public List<DGDrunkardsWalkTarget> Targets = new List<DGDrunkardsWalkTarget>(){ new DGDrunkardsWalkTarget(new Vector2Int(-1, -1), 0.5f) };
}