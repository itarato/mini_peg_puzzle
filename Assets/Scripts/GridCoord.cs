using System;
using UnityEngine;

public class GridCoord : IEquatable<GridCoord> {
    public int x;
    public int z;

    public GridCoord(int x, int z) {
        this.x = x;
        this.z = z;
    }

    public bool Equals(GridCoord other) {
        if (other == null) return false;
        return x == other.x && z == other.z;
    }

    public override bool Equals(object obj) {
        return Equals(obj as GridCoord);
    }

    public override int GetHashCode() {
        return HashCode.Combine(x, z);
    }
}
