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
    [Range(0f, 1f)]
    public float Bias;
}