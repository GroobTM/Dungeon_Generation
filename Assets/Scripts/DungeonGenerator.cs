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