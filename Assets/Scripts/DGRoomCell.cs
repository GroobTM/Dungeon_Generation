using System.Collections.Generic;
using UnityEngine;

public class DGRoomCell : MonoBehaviour
{
    [field: SerializeField, Min(0)]
    public float CellWidth { get; private set; } = 1;

    [SerializeField]
    private GameObject northWall;
    [SerializeField]
    private GameObject eastWall;
    [SerializeField]
    private GameObject southWall;
    [SerializeField]
    private GameObject westWall;

    [SerializeField]
    private List<GameObject> northDetails = new List<GameObject>();
    [SerializeField]
    private List<GameObject> eastDetails = new List<GameObject>();
    [SerializeField]
    private List<GameObject> southDetails = new List<GameObject>();
    [SerializeField]
    private List<GameObject> westDetails = new List<GameObject>();

    [SerializeField]
    private List<GameObject> northEastDetails = new List<GameObject>();
    [SerializeField]
    private List<GameObject> southEastDetails = new List<GameObject>();
    [SerializeField]
    private List<GameObject> southWestDetails = new List<GameObject>();
    [SerializeField]
    private List<GameObject> northWestDetails = new List<GameObject>();

    public void Configure(DGTile tile)
    {
        northWall.SetActive(!tile.Values[2, 1]);
        eastWall.SetActive(!tile.Values[1, 0]);
        southWall.SetActive(!tile.Values[0, 1]);
        westWall.SetActive(!tile.Values[1, 2]);

        northDetails.ForEach(obj => obj.SetActive(!tile.Values[2, 1]));
        eastDetails.ForEach(obj => obj.SetActive(!tile.Values[1, 0]));
        southDetails.ForEach(obj => obj.SetActive(!tile.Values[0, 1]));
        westDetails.ForEach(obj => obj.SetActive(!tile.Values[1, 2]));

        northEastDetails.ForEach(obj => obj.SetActive(!tile.Values[2, 0]));
        southEastDetails.ForEach(obj => obj.SetActive(!tile.Values[0, 0]));
        southWestDetails.ForEach(obj => obj.SetActive(!tile.Values[0, 2]));
        northWestDetails.ForEach(obj => obj.SetActive(!tile.Values[2, 2]));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(new Vector3(CellWidth / 2f, 0f, CellWidth / 2f), new Vector3(CellWidth, 0f, CellWidth));
    }
}
