using System;
using UnityEngine;

[Serializable]
public class DGTile : ISerializationCallbackReceiver
{
    private const int TILE_SIZE = 3;

    public bool[,] Values = new bool[TILE_SIZE, TILE_SIZE];

    public bool[,] IsContraint = new bool[TILE_SIZE, TILE_SIZE];

    [SerializeField, HideInInspector]
    private bool[] serialisedTile = new bool[TILE_SIZE * TILE_SIZE];

    public DGTile()
    {
        Values[1,1] = true;
        IsContraint[1, 1] = true;
    }

    public bool MatchesConstraints(DGTile constraints)
    {
        for (int x = 0; x < TILE_SIZE; x++)
        {
            for (int y = 0; y < TILE_SIZE; y++)
            {
                if (constraints.IsContraint[x, y])
                {
                    if (Values[x, y] != constraints.Values[x, y])
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    public DGTile RotateTile(int noRotations = 1)
    {
        noRotations %= 4;

        DGTile currentRotation = this;

        for (int i = 0; i < noRotations; i++)
        {
            DGTile nextRotation = new DGTile();

            for (int x = 0; x < TILE_SIZE; x++)
            {
                for (int y = 0; y < TILE_SIZE; y++)
                {
                    nextRotation.Values[y, TILE_SIZE - 1 - x] = currentRotation.Values[x, y];
                }
            }
        }

        return currentRotation;
    }

    public void OnBeforeSerialize()
    {
        if (serialisedTile == null || serialisedTile.Length != TILE_SIZE * TILE_SIZE)
        {
            serialisedTile = new bool[TILE_SIZE * TILE_SIZE];
        }

        if (Values == null)
        {
            Values = new bool[TILE_SIZE, TILE_SIZE];
        }

        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                serialisedTile[y * 3 + x] = Values[x, y];
            }
        }
    }

    public void OnAfterDeserialize()
    {
        if (serialisedTile == null || serialisedTile.Length != TILE_SIZE * TILE_SIZE)
        {
            return;
        }

        if (Values == null)
        {
            Values = new bool[TILE_SIZE, TILE_SIZE];
        }

        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                Values[x, y] = serialisedTile[y * 3 + x];
            }
        }
    }
}