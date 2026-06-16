using System.Collections.Generic;
using UnityEngine;

public class DGRoomCell : MonoBehaviour
{
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

    private void Start()
    {
        northWall.SetActive(false);
        eastWall.SetActive(false);
        southWall.SetActive(false);
        westWall.SetActive(false);

        northDetails.ForEach(obj  => obj.SetActive(false));
        eastDetails.ForEach(obj => obj.SetActive(false));
        southDetails.ForEach(obj => obj.SetActive(false));
        westDetails.ForEach(obj => obj.SetActive(false));

        northEastDetails.ForEach(obj => obj.SetActive(false));
        southEastDetails.ForEach(obj => obj.SetActive(false));
        southWestDetails.ForEach(obj => obj.SetActive(false));
        northWestDetails.ForEach(obj => obj.SetActive(false));
    }

    public void Configure(DGCardinalDirection cellOptions)
    {
        bool north = (cellOptions & DGCardinalDirection.NORTH) != 0;
        bool east = (cellOptions & DGCardinalDirection.EAST) != 0;
        bool south = (cellOptions & DGCardinalDirection.SOUTH) != 0;
        bool west = (cellOptions & DGCardinalDirection.WEST) != 0;


        if (north)
        {
            northWall.SetActive(true);
            northDetails.ForEach(obj => obj.SetActive(true));
        }

        if (east)
        {
            eastWall.SetActive(true);
            eastDetails.ForEach(obj => obj.SetActive(true));
        }

        if (south)
        {
            southWall.SetActive(true);
            southDetails.ForEach(obj => obj.SetActive(true));
        }

        if (west)
        {
            westWall.SetActive(true);
            westDetails.ForEach(obj => obj.SetActive(true));
        }

        if (north && east)
        {
            northEastDetails.ForEach(obj => obj.SetActive(true));
        }

        if (south && east)
        {
            southEastDetails.ForEach(obj => obj.SetActive(true));
        }

        if (south && west)
        {
            southWestDetails.ForEach(obj => obj.SetActive(true));
        }

        if (north && west)
        {
            northWestDetails.ForEach(obj => obj.SetActive(true));
        }
    }
}
