using UnityEngine;
using UnityEngine.InputSystem;

public class ShapeUnitController : MonoBehaviour {
    ShapeController parentShape;

    private float touchTime = 0f;
    private Vector2 touchPos = Vector3.zero;

    const float doubleClickThreshold = 0.3f;

    void Start() {
    }

    void Update() {
    }

    private void OnMouseDown() {
        touchTime = Time.time;
        touchPos = TouchPosition();

        parentShape.OnUnitTouch(this);
    }

    private Vector2 TouchPosition() {
        Vector3 pos;
        if (Touchscreen.current != null && (Touchscreen.current.press.wasPressedThisFrame || Touchscreen.current.press.wasReleasedThisFrame)) {
            pos = Touchscreen.current.primaryTouch.position.ReadValue();
        } else {
            pos = Mouse.current.position.ReadValue();
        }

        return new Vector2(pos.x, pos.z);
    }

    private void OnMouseUp() {
        float touchDistance = Vector2.Distance(TouchPosition(), touchPos);
        float touchLifespan = Time.time - touchTime;

        parentShape.OnUnitRelease(Util.CreateTouch(touchLifespan, touchDistance));
    }

    public void SetParent(ShapeController parentShape) {
        this.parentShape = parentShape;
    }
}
