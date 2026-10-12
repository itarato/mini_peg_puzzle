using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShapeController : MonoBehaviour {
    public GameObject shapeUnitPrefab;

    private List<GameObject> shapes = new List<GameObject>();

    private const float defaultY = 0f;

    private bool isDragging;
    private Plane dragPlane = new Plane(Vector3.up, new Vector3(0f, defaultY, 0f));
    private Vector3 dragOffset;
    private bool isHovering = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
    }

    public void Init(List<GridCoord> coords) {
        foreach (GridCoord coord in coords) {
            GameObject shapeUnit = Instantiate(shapeUnitPrefab, new Vector3(coord.x, defaultY, coord.z), Quaternion.identity);
            shapes.Add(shapeUnit);
            shapeUnit.transform.SetParent(transform);
        }
    }

    // Update is called once per frame
    void Update() {
        HandleMouseOperations();

        if (isHovering) {
            if (Keyboard.current.spaceKey.wasPressedThisFrame) {
                transform.RotateAround(GetCurrentMouseWorldPoint(), Vector3.left, 180f);
                AdjustToGrid();
            }

            if (Keyboard.current.leftCtrlKey.wasPressedThisFrame) {
                transform.RotateAround(GetCurrentMouseWorldPoint(), Vector3.up, 90f);
                AdjustToGrid();
            }
        }
    }

    private Vector3 GetCurrentMouseWorldPoint() {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        if (dragPlane.Raycast(ray, out float distance)) {
            Vector3 point = ray.GetPoint(distance);
            point.y = defaultY;
            return point;
        }
        return Vector3.zero;
    }

    private void HandleMouseOperations() {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        bool isHit = Physics.Raycast(ray, out RaycastHit hit);

        if (isHit && (hit.transform == transform || hit.transform.IsChildOf(transform))) {
            if (Mouse.current.leftButton.wasPressedThisFrame) {
                isDragging = true;

                if (dragPlane.Raycast(ray, out float distance)) {
                    Vector3 point = ray.GetPoint(distance);
                    dragOffset = transform.position - point;
                    dragOffset.y = 0f;
                }
            }

            if (!isHovering) {
                SetHighlight(true);
                isHovering = true;
            }
        } else if (isHovering && !isDragging) {
            SetHighlight(false);
            isHovering = false;
        }


        if (isDragging && Mouse.current.leftButton.isPressed) {
            if (dragPlane.Raycast(ray, out float distance)) {
                Vector3 point = ray.GetPoint(distance);
                transform.position = point + dragOffset;
            }
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame) {
            isDragging = false;
            AdjustToGrid();
        }
    }

    //private void OnGUI() {
    //    GUILayout.BeginArea(new Rect(10, 10, 300, 140), GUI.skin.box);
    //    GUILayout.Label($"Position: {transform.position:F2}");
    //    GUILayout.EndArea();
    //}

    private void SetHighlight(bool highlight) {
        foreach (GameObject shape in shapes) {
            ShapeUnitController shapeUnitController = shape.GetComponent<ShapeUnitController>();
            if (shapeUnitController != null) {
                shapeUnitController.SetHighlight(highlight);
            }
        }
    }

    private void AdjustToGrid() {
        Debug.Log("DROP");
        transform.position = new Vector3(Mathf.Round(transform.position.x), defaultY, Mathf.Round(transform.position.z));
    }
}
