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
        GameObject shape1 = Instantiate(shapeUnitPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        GameObject shape2 = Instantiate(shapeUnitPrefab, new Vector3(1, 0, 0), Quaternion.identity);
        GameObject shape3 = Instantiate(shapeUnitPrefab, new Vector3(0, 0, 1), Quaternion.identity);
        GameObject shape4 = Instantiate(shapeUnitPrefab, new Vector3(0, 0, 2), Quaternion.identity);

        shapes.Add(shape1);
        shapes.Add(shape2);
        shapes.Add(shape3);
        shapes.Add(shape4);

        shape1.transform.SetParent(transform);
        shape2.transform.SetParent(transform);
        shape3.transform.SetParent(transform);
        shape4.transform.SetParent(transform);
    }

    // Update is called once per frame
    void Update() {
        HandleMouseOperations();

        if (isHovering) {
            if (Keyboard.current.spaceKey.wasPressedThisFrame) {
                transform.RotateAround(GetCurrentMouseWorldPoint(), Vector3.left, 180f);
            }

            if (Keyboard.current.leftCtrlKey.wasPressedThisFrame) {
                transform.RotateAround(GetCurrentMouseWorldPoint(), Vector3.up, 90f);
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

        if (isHit) {
            if (Mouse.current.leftButton.wasPressedThisFrame) {
                if (hit.transform == transform || hit.transform.IsChildOf(transform)) {
                    isDragging = true;

                    if (dragPlane.Raycast(ray, out float distance)) {
                        Vector3 point = ray.GetPoint(distance);
                        dragOffset = transform.position - point;
                        dragOffset.y = 0f;
                    }
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
}
