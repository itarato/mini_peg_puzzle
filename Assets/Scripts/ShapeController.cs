using System;
using System.Collections;
using System.Collections.Generic;
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

    private ShapeUnitController lastTouchedUnit;
    private bool isTouchedThisFrame = false;
    private bool isReleasedThisFrame = false;
    private bool isDoubleTouchedThisFrame = false;

    private bool isMirrored = false;

    private GameController gameController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        UpdateMaterial();
    }

    public void Init(List<GridCoord> coords, GridCoord offset, GameController gameController) {
        this.gameController = gameController;

        foreach (GridCoord coord in coords) {
            GameObject shapeUnit = Instantiate(shapeUnitPrefab, new Vector3(coord.x + offset.x, defaultY, coord.z + offset.z), Quaternion.identity, transform);
            shapes.Add(shapeUnit);

            ShapeUnitController shapeUnitController = shapeUnit.GetComponent<ShapeUnitController>();
            shapeUnitController.SetParent(this);

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

    void Update() {
        HandleTouchOperations();

        if (isDoubleTouchedThisFrame) Rotate();

        isReleasedThisFrame = false;
        isTouchedThisFrame = false;
        isDoubleTouchedThisFrame = false;
    }

    public void ResetMirror() {
        if (!isMirrored) return;

        Mirror();
    }

    public void Rotate() {
        Quaternion targetRotation = transform.rotation * Quaternion.Euler(0f, 90f, 0f);

        Vector3 unitPos = lastTouchedUnit.transform.position - transform.position;
        Vector3 diff = new Vector3(unitPos.x - unitPos.z, defaultY, unitPos.x + unitPos.z);
        Vector3 targetPosition = transform.position + diff;

        StartCoroutine(RotateRoutine(targetRotation, targetPosition, 0.3f));
    }

    public void Mirror() {
        transform.RotateAround(lastTouchedUnit.transform.position, Vector3.left, 180f);
        AdjustToGrid();
    }

    IEnumerator RotateRoutine(Quaternion targetRotation, Vector3 targetPosition, float time) {
        Quaternion startRotation = transform.rotation;
        Vector3 startPosition = transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < time) {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / time;

            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);

            yield return null;
        }

        transform.rotation = targetRotation;
        transform.position = targetPosition;

        AdjustToGrid();
    }

    public void OnUnitTouch(ShapeUnitController unit, bool isDouble) {
        gameController.SetLastTouchedShape(this);

        if (isDouble) {
            isDoubleTouchedThisFrame = true;
        } else {
            isTouchedThisFrame = true;
        }

        lastTouchedUnit = unit;
    }

    public void OnUnitRelease() {
        isReleasedThisFrame = true;
    }

    private Vector3 GetCurrentMouseWorldPoint() {
        Vector2 mousePosition = TouchPosition();
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        if (dragPlane.Raycast(ray, out float distance)) {
            Vector3 point = ray.GetPoint(distance);
            point.y = defaultY;
            return point;
        }

        return Vector3.zero;
    }

    private Vector2 TouchPosition() {
        if (Touchscreen.current != null) {
            return Touchscreen.current.position.ReadValue();
        } else {
            return Mouse.current.position.ReadValue();
        }
    }

    private void HandleTouchOperations() {
        if (isTouchedThisFrame) {
            gameController.ClearAllShapeHighlights();

            isDragging = true;
            Vector3 mouseWorldPoint = GetCurrentMouseWorldPoint();
            dragOffset = transform.position - mouseWorldPoint;
            dragOffset.y = 0f;
            SetHighlight(true);
        }

        if (isReleasedThisFrame) {
            isDragging = false;
            AdjustToGrid();
        }

        if (isDragging) {
            Vector3 mouseWorldPoint = GetCurrentMouseWorldPoint();
            transform.position = mouseWorldPoint + dragOffset;
        }
    }

    private void AdjustToGrid() {
        transform.position = new Vector3(Mathf.Round(transform.position.x), defaultY, Mathf.Round(transform.position.z));
        OnShapePositionChanged?.Invoke(this);

        // TODO: Adjust rotation
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
            return errorMaterial;
        } else {
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
