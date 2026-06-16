
using System;

[Flags]
public enum DGCardinalDirection
{
    NONE = 0,
    NORTH = 1 << 0,
    EAST = 1 << 1,
    SOUTH = 1 << 2,
    WEST = 1 << 3,
}