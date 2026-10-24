using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using UnityEngine;

public class ShapeController : MonoBehaviour {
    public static event Action<ShapeController> OnShapePositionChanged;

    public GameObject shapeUnitPrefab;
    public GameObject connectorPrefab;

    public Material defaultMaterial;

    public Material highlightOutlineMaterial;
    public Material errorOutlineMaterial;
    public Material celebrationMaterial;

    private bool hasProblem = false;
    private bool hasHighlight = false;

    private List<GameObject> units = new List<GameObject>();
    private List<GameObject> connectors = new List<GameObject>();

    private bool isDragging;
    private Vector3 dragOffset;

    private ShapeUnitController lastTouchedUnit;
    private bool isTouchedThisFrame = false;
    private bool isReleasedThisFrame = false;
    private bool shouldRotate = false;
    private bool shouldMirror = false;

    private GameController gameController;

    private List<GridCoord> originalCoords;

    private const float elevatedY = 1f;

    private bool isLocked = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        UpdateMaterial();
    }

    public void Init(List<GridCoord> coords, GameController gameController) {
        this.gameController = gameController;
        originalCoords = coords;

        Reset();

        ApplyMaterial(defaultMaterial, 0);
    }

    void Update() {
        HandleTouchOperations();

        if (shouldRotate) Rotate();
        if (shouldMirror) Mirror();

        isReleasedThisFrame = false;
        isTouchedThisFrame = false;

        shouldRotate = false;
        shouldMirror = false;
    }

    public void Reset() {
        ResetFrom(originalCoords);
    }

    private void ResetFrom(List<GridCoord> unitCoords) {
        foreach (var shape in units) Destroy(shape);
        foreach (var connector in connectors) Destroy(connector);

        units.Clear();
        connectors.Clear();

        foreach (GridCoord coord in unitCoords) {
            GameObject shapeUnit = Instantiate(shapeUnitPrefab, new Vector3(coord.x, Util.defaultY, coord.z), Quaternion.Euler(90f, 0f, 0f));
            shapeUnit.transform.SetParent(transform, false);
            units.Add(shapeUnit);

            ShapeUnitController shapeUnitController = shapeUnit.GetComponent<ShapeUnitController>();
            shapeUnitController.SetParent(this);

            foreach (GridCoord otherCoord in unitCoords) {
                GameObject connector;

                if (otherCoord.x == coord.x && otherCoord.z == coord.z + 1) {
                    connector = Instantiate(connectorPrefab, new Vector3(coord.x, Util.defaultY, coord.z + 0.5f), Quaternion.Euler(90f, 0f, 0f));
                    connectors.Add(connector);
                } else if (otherCoord.z == coord.z && otherCoord.x == coord.x + 1) {
                    connector = Instantiate(connectorPrefab, new Vector3(coord.x + 0.5f, Util.defaultY, coord.z), Quaternion.Euler(90f, 90f, 0f));
                    connectors.Add(connector);
                } else {
                    continue;
                }

                connector.transform.SetParent(transform, false);
            }
        }

        AdjustToGrid();
        isLocked = false;
    }

    public void Rotate() {
        Quaternion targetRotation = transform.rotation * Quaternion.Euler(0f, 90f, 0f);

        Vector3 unitPos = lastTouchedUnit.transform.position - transform.position;
        Vector3 diff = new Vector3(unitPos.x - unitPos.z, Util.defaultY, unitPos.x + unitPos.z);
        Vector3 targetPosition = transform.position + diff;

        isLocked = true;
        StartCoroutine(RotateRoutine(targetRotation, targetPosition, 0.1f));
    }

    public void Mirror() {
        AdjustToGrid();

        int minx = int.MaxValue;
        int maxx = int.MinValue;

        foreach (var unit in units) {
            minx = Math.Min(minx, Mathf.RoundToInt(unit.transform.position.x));
            maxx = Math.Max(maxx, Mathf.RoundToInt(unit.transform.position.x));
        }

        foreach (var unit in units) {
            unit.transform.position = new Vector3(
                minx + maxx - unit.transform.position.x,
                unit.transform.position.y,
                unit.transform.position.z
            );
        }

        foreach (var connector in connectors) {
            connector.transform.position = new Vector3(
                minx + maxx - connector.transform.position.x,
                connector.transform.position.y,
                connector.transform.position.z
            );
        }
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
        isLocked = false;
    }

    public void OnUnitTouch(ShapeUnitController unit) {
        gameController.SetLastTouchedShape(this);
        isTouchedThisFrame = true;
        lastTouchedUnit = unit;
    }

    public void OnUnitRelease(TouchKind touchKind) {
        isReleasedThisFrame = true;

        if (touchKind == TouchKind.Short) shouldRotate = true;
        if (touchKind == TouchKind.Long) shouldMirror = true;
    }

    private void HandleTouchOperations() {
        if (isLocked) return;

        if (isTouchedThisFrame) {
            gameController.ResetAllShapeMaterials();

            isDragging = true;
            Vector3 mouseWorldPoint = Util.GetCurrentMouseWorldPoint();
            dragOffset = transform.position - mouseWorldPoint;
            dragOffset.y = 0f;
            SetHighlight(true);
        }

        if (isReleasedThisFrame) {
            isDragging = false;
            SetHighlight(false);
            AdjustToGrid();
        }

        if (isDragging) {
            Vector3 mouseWorldPoint = Util.GetCurrentMouseWorldPoint();
            transform.position = mouseWorldPoint + dragOffset + Vector3.up * elevatedY;
        }
    }

    private void AdjustToGrid() {
        transform.position = new Vector3(Mathf.Round(transform.position.x), Util.defaultY, Mathf.Round(transform.position.z));
        OnShapePositionChanged?.Invoke(this);
    }

    public List<GridCoord> GridPositions() {
        List<GridCoord> gridCoords = new List<GridCoord>();

        for (int i = 0; i < units.Count; i++) {
            GameObject shapeUnit = units[i];
            Vector3 position = shapeUnit.transform.position;
            GridCoord gridCoord = new GridCoord(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));
            gridCoords.Add(gridCoord);
        }

        return gridCoords;
    }


    public void SetProblem(bool problem) {
        if (hasProblem == problem) return;

        hasProblem = problem;
        UpdateMaterial();
    }

    public void StartCompletionPhase() {
        isLocked = true;

        ApplyMaterial(celebrationMaterial, 0);
        ApplyMaterial(null, 1);
    }

    public void SetHighlight(bool highlight) {
        hasHighlight = highlight;
        UpdateMaterial();
    }

    public void ResetMaterial() {
        hasHighlight = false;
        hasProblem = false;
        UpdateMaterial();
    }

    private void ApplyMaterial(Material material, int materialIndex) {
        Util.ApplyMaterialToList(units, material, materialIndex);
        Util.ApplyMaterialToList(connectors, material, materialIndex);
    }

    private void UpdateMaterial() {
        if (hasProblem) {
            ApplyMaterial(errorOutlineMaterial, 1);
        } else if (hasHighlight) {
            ApplyMaterial(highlightOutlineMaterial, 1);
        } else {
            ApplyMaterial(null, 1);
        }
    }

    private int Height() {
        int min = int.MaxValue;
        int max = int.MinValue;

        foreach (var unit in units) {
            min = Math.Min(min, Mathf.RoundToInt(unit.transform.position.z));
            max = Math.Max(max, Mathf.RoundToInt(unit.transform.position.z));
        }

        return max - min + 1;
    }

    public void RotateWithoutAnimationInPlace() {
        int h = Height();
        Debug.Log("H = " + h);
        transform.RotateAround(UpRightCorner(), Vector3.up, 90f);
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + h - 1);
    }

    public Vector3 UpRightCorner() {
        int minx = int.MaxValue;
        int minz = int.MaxValue;

        foreach (var unit in units) {
            minx = Math.Min(minx, Mathf.RoundToInt(unit.transform.position.x));
            minz = Math.Min(minz, Mathf.RoundToInt(unit.transform.position.z));
        }

        return new Vector3(minx, transform.position.y, minz);
    }

    public void ApplySource(ShapeSource source) {
        transform.position = new Vector3(source.x, Util.defaultY, source.z);

        for (int i = 0; i < source.rotation; i++) RotateWithoutAnimationInPlace();

        if (source.is_mirrored) Mirror();
    }
}
