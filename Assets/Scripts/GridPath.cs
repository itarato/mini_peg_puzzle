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
        start = new GridCoord(startX, endX);
        end = new GridCoord(startZ, endZ);
    }

    public bool IsPointOnPath(Vector3 point) {
        if (start.x == end.x) {
            return point.x == start.x && point.z >= Mathf.Min(start.z, end.z) && point.z <= Mathf.Max(start.z, end.z);
        } else if (start.z == end.z) {
            return point.z == start.z && point.x >= Mathf.Min(start.x, end.x) && point.x <= Mathf.Max(start.x, end.x);
        }

        throw new System.Exception("Path is not straight");
    }

    public Vector3 GetClosestPointOnPath(Vector3 point) {
        if (start.x == end.x) {
            float clampedZ = Mathf.Clamp(point.z, Mathf.Min(start.z, end.z), Mathf.Max(start.z, end.z));
            return new Vector3(start.x, point.y, clampedZ);
        } else if (start.z == end.z) {
            float clampedX = Mathf.Clamp(point.x, Mathf.Min(start.x, end.x), Mathf.Max(start.x, end.x));
            return new Vector3(clampedX, point.y, start.z);
        }

        throw new System.Exception("Path is not straight");
    }

    public override string ToString() {
        return "[" + start + ".." + end + "]";
    }
}
