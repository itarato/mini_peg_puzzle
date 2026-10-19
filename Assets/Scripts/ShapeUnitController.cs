using UnityEngine;

public class ShapeUnitController : MonoBehaviour {
    ShapeController parentShape;

    private float[] touchTimes = { 0f, 0f };

    const float doubleClickThreshold = 0.3f;

    void Start() {
    }

    void Update() {
    }

    private void OnMouseDown() {
        touchTimes[1] = touchTimes[0];
        touchTimes[0] = Time.time;

        if (touchTimes[0] - touchTimes[1] < doubleClickThreshold) {
            parentShape.OnUnitTouch(this, true);
            touchTimes[0] = 0f;
        } else {
            parentShape.OnUnitTouch(this, false);
        }
    }

    private void OnMouseUp() {
        parentShape.OnUnitRelease();
    }

    public void SetParent(ShapeController parentShape) {
        this.parentShape = parentShape;
    }
}
