using UnityEngine;
using UnityEngine.InputSystem;

public class ShapeUnitController : MonoBehaviour {
    ShapeController parentShape;

    private float touchTime = 0f;
    private Vector2 touchPos = Vector3.zero;

    void Start() {
    }

    void Update() {
    }

    private void OnMouseDown() {
        touchTime = Time.time;
        touchPos = Util.TouchPosition();

        parentShape.OnUnitTouch(this);
    }

    private void OnMouseUp() {
        float touchDistance = Vector2.Distance(Util.TouchPosition(), touchPos);
        float touchLifespan = Time.time - touchTime;

        parentShape.OnUnitRelease(Util.CreateTouch(touchLifespan, touchDistance));
    }

    public void SetParent(ShapeController parentShape) {
        this.parentShape = parentShape;
    }
}
