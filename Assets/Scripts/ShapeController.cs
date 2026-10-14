using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShapeController : MonoBehaviour {
    public static event Action<ShapeController> OnShapePositionChanged;

    public GameObject shapeUnitPrefab;
    public GameObject connectorPrefab;

    public Material defaultMaterial;
    public Material highlightMaterial;
    public Material errorMaterial;

    private bool hasProblem = false;
    private bool hasHighlight = false;

    private List<GameObject> shapes = new List<GameObject>();
    private List<GameObject> attachementObjects = new List<GameObject>();

    public const float defaultY = 0f;

    private bool isDragging;
    private Plane dragPlane = new Plane(Vector3.up, new Vector3(0f, defaultY, 0f));
    private Vector3 dragOffset;
    private bool isHovering = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        UpdateMaterial();
    }

    public void Init(List<GridCoord> coords, GridCoord offset) {
        foreach (GridCoord coord in coords) {
            GameObject shapeUnit = Instantiate(shapeUnitPrefab, new Vector3(coord.x + offset.x, defaultY, coord.z + offset.z), Quaternion.identity, transform);
            shapes.Add(shapeUnit);

            foreach (GridCoord otherCoord in coords) {
                if (otherCoord.x == coord.x && otherCoord.z == coord.z + 1) {
                    GameObject connector = Instantiate(connectorPrefab, new Vector3(coord.x + offset.x, defaultY, coord.z + offset.z + 0.5f), Quaternion.Euler(90f, 0f, 0f), transform);
                    attachementObjects.Add(connector);
                } else if (otherCoord.z == coord.z && otherCoord.x == coord.x + 1) {
                    GameObject connector = Instantiate(connectorPrefab, new Vector3(coord.x + offset.x + 0.5f, defaultY, coord.z + offset.z), Quaternion.Euler(90f, 90f, 0f), transform);
                    attachementObjects.Add(connector);
                } else {
                    continue;
                }
            }
        }

        AdjustToGrid();
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

    private void AdjustToGrid() {
        transform.position = new Vector3(Mathf.Round(transform.position.x), defaultY, Mathf.Round(transform.position.z));
        OnShapePositionChanged?.Invoke(this);
    }

    public List<GridCoord> GridPositions() {
        List<GridCoord> gridCoords = new List<GridCoord>();

        for (int i = 0; i < shapes.Count; i++) {
            GameObject shapeUnit = shapes[i];
            Vector3 position = shapeUnit.transform.position;
            GridCoord gridCoord = new GridCoord(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));
            gridCoords.Add(gridCoord);
        }

        return gridCoords;
    }


    public void SetProblem(bool problem) {
        hasProblem = problem;
        UpdateMaterial();
    }

    private Material DefaultMaterial() {
        if (hasProblem) {
            //Debug.Log("ERROR MATERIAL");
            return errorMaterial;
        } else {
            //Debug.Log("DEFAULT MATERIAL");
            return defaultMaterial;
        }
    }

    public void SetHighlight(bool highlight) {
        hasHighlight = highlight;
        UpdateMaterial();
    }

    private void UpdateMaterial() {
        foreach (GameObject shape in shapes) ApplyMaterialOnRenderer(shape.GetComponent<Renderer>());
        foreach (GameObject attachementObject in attachementObjects) ApplyMaterialOnRenderer(attachementObject.GetComponent<Renderer>());
    }

    private void ApplyMaterialOnRenderer(Renderer renderer) {
        if (renderer == null) return;

        if (hasHighlight) {
            renderer.material = highlightMaterial;
        } else {
            renderer.material = DefaultMaterial();
        }
    }
}
