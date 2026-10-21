public class Util {
    public static TouchKind CreateTouch(float timespan, float distance) {
        if (distance > 2f) return TouchKind.Regular;
        if (timespan < 0.1f) return TouchKind.Short;
        if (timespan < 1f) return TouchKind.Long;
        return TouchKind.Regular;
    }
}
