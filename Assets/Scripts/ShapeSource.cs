public class ShapeSource {
    public int index;
    public int x;
    public int z;
    public int rotation;
    public bool is_mirrored;

    public ShapeSource(int index, int x, int z, int rotation, bool is_mirrored) {
        this.index = index;
        this.x = x;
        this.z = z;
        this.rotation = rotation;
        this.is_mirrored = is_mirrored;
    }
}