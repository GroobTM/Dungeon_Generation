using System;
using UnityEngine;

[Serializable]
public class DGDrunkardsWalkTarget
{
    public DGDrunkardsWalkTarget(Vector2Int position, float bias)
    {
        Position = position;
        Bias = bias;
    }

    public Vector2Int Position;
    public float Bias;
}