using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DGTileSet", menuName = "Scriptable Objects/DGTileSet")]
public class DGTileSet : ScriptableObject
{
    public List<DGTile> Tiles = new List<DGTile>() { new DGTile() };

    public List<DGTile> GetMatching(DGTile surroundingTiles)
    {
        List<DGTile> matchingTiles = new List<DGTile>();

        foreach (DGTile tile in Tiles)
        {            
            for (int i = 0; i < 4; i++)
            {
                DGTile rotation = tile.RotateTile(i);

                if (rotation.MatchesConstraints(surroundingTiles))
                {
                    matchingTiles.Add(rotation);
                }
            }
        }

        return matchingTiles;
    }
}
