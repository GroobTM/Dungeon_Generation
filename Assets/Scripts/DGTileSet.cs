using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DGTileSet", menuName = "Scriptable Objects/DGTileSet")]
public class DGTileSet : ScriptableObject
{
    public List<DGTile> Tiles = new List<DGTile>() { new DGTile() };
}
