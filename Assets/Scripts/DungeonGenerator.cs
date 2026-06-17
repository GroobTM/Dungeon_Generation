using NUnit.Framework.Constraints;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour, ISerializationCallbackReceiver
{
    [Range(1, 50)]
    public int GridWidth = 10;
    [Range(1, 50)]
    public int GridHeight = 10;
    public bool[,] EnabledGrid { get; private set; } = null;

    [SerializeField, HideInInspector]
    private bool[] serialisedEnabledGrid;
    [SerializeField, HideInInspector]
    private int serialisedEnabledGridWidth;
    [SerializeField, HideInInspector]
    private int serialisedEnabledGridHeight;

    public DGDrunkardsWalkParameters DrunkardsWalkParameters = new DGDrunkardsWalkParameters();

    [field: SerializeField]
    public DGTileSet TileSet { get; private set; }
    [field: SerializeField]
    public DGRoomCell RoomCell { get; private set; }


    public void OnValidate()
    {
        bool[,] newEnabledGrid = new bool[GridWidth, GridHeight];

        if (EnabledGrid != null)
        {
            int xLimit = Mathf.Min(GridWidth, EnabledGrid.GetLength(0));
            int yLimit = Mathf.Min(GridHeight, EnabledGrid.GetLength(1));

            for (int x = 0; x < xLimit; x++)
            {
                for (int y = 0; y < yLimit; y++)
                {
                    newEnabledGrid[x, y] = EnabledGrid[x, y];
                }
            }
        }

        EnabledGrid = newEnabledGrid;
    }

    public void ResetGrid(bool enabled = false)
    {
        if (EnabledGrid == null)
        {
            OnValidate();
        }

        for (int x = 0; x < EnabledGrid.GetLength(0); x++)
        {
            for (int y = 0; y < EnabledGrid.GetLength(1); y++)
            {
                EnabledGrid[x, y] = enabled;
            }
        }
    }

    public void PerformDrunkardsWalk()
    {
        ResetGrid(false);
        
        DGDrunkardsWalk[] drunkards = new DGDrunkardsWalk[DrunkardsWalkParameters.Targets.Count];
        DGSharedCounter sharedDrunkardCellCounter = new DGSharedCounter();

        for (int i = 0; i < drunkards.Length; i++)
        {
            drunkards[i] = new DGDrunkardsWalk(
                DrunkardsWalkParameters.Targets[i].Position,
                DrunkardsWalkParameters.Targets[i].Bias,
                EnabledGrid,
                sharedDrunkardCellCounter
            );
        }

        int totalCells = EnabledGrid.GetLength(0) * EnabledGrid.GetLength(1);
        int maxCells = Mathf.RoundToInt(totalCells * DrunkardsWalkParameters.MaxGridFill);
        for (int i = 0; i < DrunkardsWalkParameters.StepCount; i++)
        {
            foreach (DGDrunkardsWalk drunkard in drunkards)
            {
                if (sharedDrunkardCellCounter.Value >= maxCells)
                {
                    return;
                }

                drunkard.Step();
            }
        }
    }

    public void PerformWaveFunctionCollapse()
    {
        List<DGTile>[,] entropyGrid = CreateEmptyEntropyGrid();
        if (entropyGrid.GetLength(0) == 0)
        {
            return;
        }

        DGTile[,] tileGrid = new DGTile[entropyGrid.GetLength(0), entropyGrid.GetLength(1)];
        
        entropyGrid = PopulateEntropyGrid(entropyGrid, tileGrid);
        (int lowestEntropyCell, Vector2Int lowestEntropyCellPosition) = FindLowestEntropyCell(entropyGrid);

        while (lowestEntropyCell >= 0)
        {
            tileGrid[lowestEntropyCellPosition.x, lowestEntropyCellPosition.y] = SelectRandomTile(entropyGrid[lowestEntropyCellPosition.x, lowestEntropyCellPosition.y]);

            entropyGrid = PopulateEntropyGrid(entropyGrid, tileGrid);
            (lowestEntropyCell, lowestEntropyCellPosition) = FindLowestEntropyCell(entropyGrid);
        }
    }

    private List<DGTile>[,] CreateEmptyEntropyGrid()
    {
        int highestX = -1;
        int highestY = -1;

        for (int x = 0; x < EnabledGrid.GetLength(0); x++)
        {
            for (int y = 0; y < EnabledGrid.GetLength(1); y++)
            {
                if (EnabledGrid[x,y])
                {
                    if (x > highestX)
                    {
                        highestX = x;
                    }
                    if (y > highestY)
                    {
                        highestY = y;
                    }
                }
            }
        }

        if (highestX == -1 || highestY == -1)
        {
            return new List<DGTile>[0, 0];
        }
        else
        {
            return new List<DGTile>[highestX + 1, highestY + 1];
        }
    }

    private List<DGTile>[,] PopulateEntropyGrid(List<DGTile>[,] entropyGrid, DGTile[,] currentTiles)
    {
        for (int x = 0; x < entropyGrid.GetLength(0); x++)
        {
            for (int y = 0; y < entropyGrid.GetLength(1); y++)
            {
                if (EnabledGrid[x, y])
                {
                    if (currentTiles[x, y] != null)
                    {
                        entropyGrid[x, y] = null;
                    }
                    else
                    {
                        entropyGrid[x, y] = GetTileEntropy(currentTiles, x, y);
                    }
                }
                else
                {
                    entropyGrid[x, y] = null;
                }
            }
        }

        return entropyGrid;
    }

    private List<DGTile> GetTileEntropy(DGTile[,] currentTiles, int xPos, int yPos)
    {
        DGTile tileConstraints = new DGTile();
        
        int currentTilesWidth = currentTiles.GetLength(0);
        int currentTilesHeight = currentTiles.GetLength(1);

        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                if (!(x == 1  && y == 1))
                {
                    int adjacentX = xPos + (x - 1);
                    int adjacentY = yPos + (y - 1);

                    if (adjacentX >= 0 && adjacentX < currentTilesWidth && adjacentY >= 0 && adjacentY < currentTilesHeight)
                    {
                        if (EnabledGrid[adjacentX, adjacentY])
                        {
                            DGTile adjacentTile = currentTiles[adjacentX, adjacentY];

                            if (adjacentTile != null) 
                            {
                                tileConstraints.Values[x, y] = adjacentTile.Values[2 - x, 2 - y];
                                tileConstraints.IsContraint[x, y] = true;
                            }
                        }
                        else
                        {
                            tileConstraints.Values[x, y] = false;
                            tileConstraints.IsContraint[x, y] = false;
                        }
                    }
                    else
                    {
                        tileConstraints.Values[x, y] = false;
                        tileConstraints.IsContraint[x, y] = false;
                    }
                }
            }
        }

        return TileSet.GetMatching(tileConstraints);
    }

    private (int, Vector2Int) FindLowestEntropyCell(List<DGTile>[,] entropyGrid)
    {
        Vector2Int lowestPosition = new Vector2Int(-1, -1);
        int lowestEntropy = int.MaxValue;

        for (int x = 0; x < entropyGrid.GetLength(0); x++)
        {
            for (int y = 0; y < entropyGrid.GetLength(1); y++)
            {
                if (entropyGrid[x, y] != null)
                {
                    int count = entropyGrid[x, y].Count;

                    if (count == 0)
                    {
                        return (0, new Vector2Int(0, 0));
                    }

                    if (count < lowestEntropy)
                    {
                        lowestEntropy = entropyGrid[x, y].Count;
                        lowestPosition = new Vector2Int(x, y);
                    }
                }
            }
        }

        if (lowestEntropy == int.MaxValue)
        {
            return (-1, new Vector2Int(-1, -1));
        }

        return (lowestEntropy, lowestPosition);
    }

    private DGTile SelectRandomTile(List<DGTile> tiles)
    {
        int selection = Random.Range(0, tiles.Count);
        return tiles[selection];
    }

    public void OnBeforeSerialize()
    {
        if (EnabledGrid == null)
        {
            return;
        }

        serialisedEnabledGridWidth = EnabledGrid.GetLength(0);
        serialisedEnabledGridHeight = EnabledGrid.GetLength(1);
        serialisedEnabledGrid = new bool[serialisedEnabledGridWidth * serialisedEnabledGridHeight];

        for (int x = 0; x < serialisedEnabledGridWidth; x++)
        {
            for (int y = 0;y < serialisedEnabledGridHeight; y++)
            {
                serialisedEnabledGrid[y * serialisedEnabledGridWidth + x] = EnabledGrid[x, y];
            }
        }
    }

    public void OnAfterDeserialize()
    {
        if (serialisedEnabledGrid == null || serialisedEnabledGridWidth <= 0 || serialisedEnabledGridHeight <= 0)
        {
            return;
        }

        EnabledGrid = new bool[serialisedEnabledGridWidth, serialisedEnabledGridHeight];

        for (int x = 0; x < serialisedEnabledGridWidth; x++)
        {
            for (int y = 0; y < serialisedEnabledGridHeight; y++)
            {
                EnabledGrid[x, y] = serialisedEnabledGrid[y * serialisedEnabledGridWidth + x];
            }
        }
    }
}