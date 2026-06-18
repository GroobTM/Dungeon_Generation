using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour, ISerializationCallbackReceiver
{
    // ----- Grid Settings -----
    [SerializeField, Range(1, 50)]
    private int gridWidth = 10;
    [SerializeField, Range(1, 50)]
    private int gridHeight = 10;
    public bool[,] EnabledGrid { get; private set; } = null;

    [SerializeField, HideInInspector]
    private bool[] serialisedEnabledGrid;
    [SerializeField, HideInInspector]
    private int serialisedEnabledGridWidth;
    [SerializeField, HideInInspector]
    private int serialisedEnabledGridHeight;

    // ----- Drunkard's Walk Settings -----
    [SerializeField]
    private List<DGDrunkardsWalkTarget> drunkardsWalkTargets = new List<DGDrunkardsWalkTarget>() { new DGDrunkardsWalkTarget(new Vector2Int(-1, -1), 0.5f) };

    [SerializeField, Range(0, 10000)]
    private int drunkardsWalkStepCount = 0;
    [SerializeField, Range(0, 1)]
    private float drunkardsWalkMaxGridFill = 1f;
    [SerializeField, Min(-1)]
    private int drunkardsWalkSeed = -1;

    // ----- WFC Settings -----
    [SerializeField]
    private DGTileSet tileSet = null;

    private DGTile[,] tileGrid = null;

    [SerializeReference, HideInInspector]
    private DGTile[] serialisedTileGrid;
    [SerializeField, HideInInspector]
    private int serialisedTileGridWidth;
    [SerializeField, HideInInspector]
    private int serialisedTileGridHeight;

    [SerializeField, Min(-1)]
    private int wfcSeed = -1;

    // ----- 3D Conversion -----
    [SerializeField]
    private DGRoomCell roomCell = null;


    public void OnValidate()
    {
        bool[,] newEnabledGrid = new bool[gridWidth, gridHeight];

        if (EnabledGrid != null)
        {
            int xLimit = Mathf.Min(gridWidth, EnabledGrid.GetLength(0));
            int yLimit = Mathf.Min(gridHeight, EnabledGrid.GetLength(1));

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

    public void OnDrawGizmos()
    {
        if (tileGrid != null)
        {
            float visualiseSize = roomCell != null ? roomCell.CellWidth : 1f;

            for (int x = 0; x < tileGrid.GetLength(0); x++)
            {
                for (int y = 0; y < tileGrid.GetLength(1); y++)
                {
                    tileGrid[x, y]?.DrawGizmo(visualiseSize, x * visualiseSize, y * visualiseSize);
                }
            }
        }
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
        
        DGDrunkardsWalk[] drunkards = new DGDrunkardsWalk[drunkardsWalkTargets.Count];
        DGSharedCounter sharedDrunkardCellCounter = new DGSharedCounter();

        for (int i = 0; i < drunkards.Length; i++)
        {
            drunkards[i] = new DGDrunkardsWalk(
                drunkardsWalkTargets[i].Position,
                drunkardsWalkTargets[i].Bias,
                EnabledGrid,
                sharedDrunkardCellCounter
            );
        }

        int totalCells = EnabledGrid.GetLength(0) * EnabledGrid.GetLength(1);
        int maxCells = Mathf.RoundToInt(totalCells * drunkardsWalkMaxGridFill);
        for (int i = 0; i < drunkardsWalkStepCount; i++)
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
        if (tileSet == null)
        {
            return;
        }

        List<DGTile>[,] entropyGrid = CreateEmptyEntropyGrid();
        if (entropyGrid.GetLength(0) == 0 || entropyGrid.GetLength(1) == 0)
        {
            return;
        }

        tileGrid = new DGTile[entropyGrid.GetLength(0), entropyGrid.GetLength(1)];
        
        entropyGrid = PopulateEntropyGrid(entropyGrid);
        (int lowestEntropyCell, Vector2Int lowestEntropyCellPosition) = FindLowestEntropyCell(entropyGrid);

        while (lowestEntropyCell >= 0)
        {
            if (lowestEntropyCell == 0)
            {
                LogMissingCell(lowestEntropyCellPosition.x, lowestEntropyCellPosition.y);
                break;
            }
            tileGrid[lowestEntropyCellPosition.x, lowestEntropyCellPosition.y] = SelectRandomTile(entropyGrid[lowestEntropyCellPosition.x, lowestEntropyCellPosition.y]);

            entropyGrid = PopulateEntropyGrid(entropyGrid);
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

    private List<DGTile>[,] PopulateEntropyGrid(List<DGTile>[,] entropyGrid)
    {
        for (int x = 0; x < entropyGrid.GetLength(0); x++)
        {
            for (int y = 0; y < entropyGrid.GetLength(1); y++)
            {
                if (EnabledGrid[x, y])
                {
                    if (tileGrid[x, y] != null)
                    {
                        entropyGrid[x, y] = null;
                    }
                    else
                    {
                        entropyGrid[x, y] = GetTileEntropy(x, y);
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

    private List<DGTile> GetTileEntropy(int xPos, int yPos)
    {
        DGTile tileConstraints = BuildTileConstraints(xPos, yPos);

        return tileSet.GetMatching(tileConstraints);
    }

    private DGTile BuildTileConstraints(int xPos, int yPos)
    {
        DGTile tileConstraints = new DGTile();

        int currentTilesWidth = tileGrid.GetLength(0);
        int currentTilesHeight = tileGrid.GetLength(1);

        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                if (!(x == 1 && y == 1))
                {
                    int adjacentX = xPos + (x - 1);
                    int adjacentY = yPos + (y - 1);

                    if (adjacentX >= 0 && adjacentX < currentTilesWidth && adjacentY >= 0 && adjacentY < currentTilesHeight)
                    {
                        if (EnabledGrid[adjacentX, adjacentY])
                        {
                            DGTile adjacentTile = tileGrid[adjacentX, adjacentY];

                            if (adjacentTile != null)
                            {
                                tileConstraints.Values[x, y] = adjacentTile.Values[2 - x, 2 - y];
                                tileConstraints.IsContraint[x, y] = true;
                            }
                        }
                        else
                        {
                            tileConstraints.Values[x, y] = false;
                            tileConstraints.IsContraint[x, y] = true;
                        }
                    }
                    else
                    {
                        tileConstraints.Values[x, y] = false;
                        tileConstraints.IsContraint[x, y] = true;
                    }
                }
            }
        }

        return tileConstraints;
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
                        return (0, new Vector2Int(x, y));
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

    private void LogMissingCell(int xPos, int yPos)
    {
        DGTile tileConstraints = BuildTileConstraints(xPos, yPos);

        string message = $"Wave Function Collapse failed at position ({xPos}, {yPos}). A tile of this shape is require:\r\n";
        message += "([W] = Wall, [F] = Floor, [C] = Centre, [?] = Any)\r\n\r\n";

        for (int y = 2; y >= 0; y--)
        {
            string row = "";

            for (int x = 0; x < 3; x++)
            {
                if (x == 1 && y == 1)
                {
                    row += "[C] ";
                }
                else if (tileConstraints.IsContraint[x, y])
                {
                    row += tileConstraints.Values[x, y] ? "[F] " : "[W] ";
                }
                else
                {
                    row += "[?] ";
                }
            }

            message += row + "\r\n";
        }

        Debug.LogError(message);
    }

    public void OnBeforeSerialize()
    {
        if (EnabledGrid != null)
        {
            serialisedEnabledGridWidth = EnabledGrid.GetLength(0);
            serialisedEnabledGridHeight = EnabledGrid.GetLength(1);
            serialisedEnabledGrid = new bool[serialisedEnabledGridWidth * serialisedEnabledGridHeight];

            for (int x = 0; x < serialisedEnabledGridWidth; x++)
            {
                for (int y = 0; y < serialisedEnabledGridHeight; y++)
                {
                    serialisedEnabledGrid[y * serialisedEnabledGridWidth + x] = EnabledGrid[x, y];
                }
            }
        }


        if (tileGrid != null)
        {
            serialisedTileGridWidth = tileGrid.GetLength(0);
            serialisedTileGridHeight = tileGrid.GetLength(1);
            serialisedTileGrid = new DGTile[serialisedTileGridWidth * serialisedTileGridHeight];

            for (int x = 0; x < serialisedTileGridWidth; x++)
            {
                for (int y = 0; y < serialisedTileGridHeight; y++)
                {
                    serialisedTileGrid[y * serialisedTileGridWidth + x] = tileGrid[x, y];
                }
            }
        }
    }

    public void OnAfterDeserialize()
    {
        if (serialisedEnabledGrid != null && serialisedEnabledGridWidth > 0 && serialisedEnabledGridHeight > 0)
        {
            EnabledGrid = new bool[serialisedEnabledGridWidth, serialisedEnabledGridHeight];

            for (int x = 0; x < serialisedEnabledGridWidth; x++)
            {
                for (int y = 0; y < serialisedEnabledGridHeight; y++)
                {
                    EnabledGrid[x, y] = serialisedEnabledGrid[y * serialisedEnabledGridWidth + x];
                }
            }
        }

        if (serialisedTileGrid != null && serialisedTileGridWidth > 0 && serialisedTileGridHeight > 0)
        {
            tileGrid = new DGTile[serialisedTileGridWidth, serialisedTileGridHeight];

            for (int x = 0; x < serialisedTileGridWidth; x++)
            {
                for (int y = 0; y < serialisedTileGridHeight; y++)
                {
                    tileGrid[x, y] = serialisedTileGrid[y * serialisedTileGridWidth + x];
                }
            }
        }
    }
}