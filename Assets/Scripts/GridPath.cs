using UnityEngine;

public class GridPath
{
    public GridCoord start;
    public GridCoord end;

    public GridPath(GridCoord start, GridCoord end) {
        this.start = start;
        this.end = end;
    }

    public GridPath(int startX, int endX, int startZ, int endZ) {
        this.start = new GridCoord(startX, endX);
        this.end = new GridCoord(startZ, endZ);
    }
}
