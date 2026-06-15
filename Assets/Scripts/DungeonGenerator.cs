using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [Range(1, 50)]
    public int GridWidth = 10;
    [Range(1, 50)]
    public int GridHeight = 10;
    public bool[,] EnabledGrid { get; private set; } = null;

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
        if (EnabledGrid != null)
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
}