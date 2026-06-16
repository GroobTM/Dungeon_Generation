using System.Collections.Generic;
using UnityEngine;

public class DGDrunkardsWalk
{
    private readonly Vector2Int gridBounds;
    private readonly Vector2Int target;
    private bool[,] grid;
    private Vector2Int currentPos;
    private DGSharedCounter sharedCellCounter;
    private float targetBias;

    private Dictionary<DGCardinalDirection, float> directionWeights = new Dictionary<DGCardinalDirection, float>();

    public DGDrunkardsWalk(Vector2Int target, float targetBias, bool[,] grid, DGSharedCounter sharedCellCounter)
    {
        this.target = target;
        this.grid = grid;
        gridBounds = new Vector2Int(grid.GetLength(0), grid.GetLength(1));
        currentPos = new Vector2Int(Mathf.RoundToInt(gridBounds.x / 2f), Mathf.RoundToInt(gridBounds.y / 2f));
        this.sharedCellCounter = sharedCellCounter;
        this.targetBias = targetBias;

        CarveCurrentCell();
    }

    public void Step()
    {
        UpdateProbabilities();

        float totalWeight = 0f;
        foreach (float weight in directionWeights.Values)
        {
            totalWeight += weight;
        }

        if (totalWeight <= 0f)
        {
            return;
        }

        float selectedValue = Random.Range(0f, totalWeight);

        totalWeight = 0f;
        foreach(KeyValuePair<DGCardinalDirection, float> directionWeight in directionWeights)
        {
            totalWeight += directionWeight.Value;

            if (selectedValue <= totalWeight)
            {
                MoveDrunkard(directionWeight.Key);
                CarveCurrentCell();
                return;
            }
        }
    }

    private void UpdateProbabilities()
    {
        float northBaseProbability = currentPos.y > 0 ? 1 : 0f;
        float eastBaseProbability = currentPos.x > 0 ? 1f : 0f;
        float southBaseProbability = currentPos.y < gridBounds.y - 1 ? 1f : 0f;
        float westBaseProbability = currentPos.x < gridBounds.x - 1 ? 1f : 0f; 

        if (target.x == -1 || target.y == -1)
        {
            directionWeights[DGCardinalDirection.NORTH] = northBaseProbability;
            directionWeights[DGCardinalDirection.EAST] = eastBaseProbability;
            directionWeights[DGCardinalDirection.SOUTH] = southBaseProbability;
            directionWeights[DGCardinalDirection.WEST] = westBaseProbability;
        }
        else
        {
            Vector2Int diff = target - currentPos;

            float northBiasProbability = diff.y < 0 ? 1f : 0f;
            float eastBiasProbability = diff.x < 0 ? 1f : 0f;
            float southBiasProbability = diff.y > 0 ? 1f : 0f;
            float westBiasProbability = diff.x > 0 ? 1f : 0f;

            directionWeights[DGCardinalDirection.NORTH] = Mathf.Lerp(1f, northBiasProbability, targetBias) * northBaseProbability;
            directionWeights[DGCardinalDirection.EAST] = Mathf.Lerp(1f, eastBiasProbability, targetBias) * eastBaseProbability;
            directionWeights[DGCardinalDirection.SOUTH] = Mathf.Lerp(1f, southBiasProbability, targetBias) * southBaseProbability;
            directionWeights[DGCardinalDirection.WEST] = Mathf.Lerp(1f, westBiasProbability, targetBias) * westBaseProbability;
        }        
    }

    private void MoveDrunkard(DGCardinalDirection direction)
    {
        switch (direction)
        {
            case DGCardinalDirection.NORTH:
                currentPos.y -= 1;
                break;
            case DGCardinalDirection.EAST:
                currentPos.x -= 1;
                break;
            case DGCardinalDirection.SOUTH:
                currentPos.y += 1;
                break;
            case DGCardinalDirection.WEST:
                currentPos.x += 1;
                break;
        }
    }

    private void CarveCurrentCell()
    {
        if (!grid[currentPos.x, currentPos.y])
        {
            sharedCellCounter.Value++;
            grid[currentPos.x, currentPos.y] = true;
        }
    }
}