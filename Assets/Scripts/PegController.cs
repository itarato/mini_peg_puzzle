using System.Collections.Generic;
using UnityEngine;

public class PegController : MonoBehaviour {
    public GameObject peg;

    private List<GridPath> availablePaths;

    private bool isDragging = false;
    private Vector3 dragOffset = Vector3.zero;
    private bool isTouchedThisFrame = false;
    private bool isReleasedThisFrame = false;

    private bool isFixed = false;

    public void Init(List<GridPath> availablePaths) {
        this.availablePaths = availablePaths;
    }

    void Start() {

    }

    void Update() {
        HandleMouseOperations();

        isTouchedThisFrame = false;
        isReleasedThisFrame = false;
    }

    private void OnMouseDown() {
        isTouchedThisFrame = true;
    }

    private void OnMouseUp() {
        isReleasedThisFrame = true;
    }

    private void HandleMouseOperations() {
        if (isFixed) return;

        if (isTouchedThisFrame) {
            isDragging = true;
            Vector3 mouseWorldPoint = Util.GetCurrentMouseWorldPoint();
            dragOffset = transform.position - mouseWorldPoint;
            dragOffset.y = 0f;
            SetHighlight(true);
        }

        if (isReleasedThisFrame) {
            SetHighlight(false);
            isDragging = false;
            AdjustToGrid();
        }

        if (isDragging) {
            Vector3 mouseWorldPoint = Util.GetCurrentMouseWorldPoint();
            transform.position = GetOnLinePositionForCandidate(mouseWorldPoint + dragOffset);
        }
    }

    private Vector3 GetOnLinePositionForCandidate(Vector3 targetPosition) {
        targetPosition = NormalizePosition(targetPosition);
        List<GridPath> availablePathsForPosition = GetAvailablePathsForPosition(transform.position);

        if (availablePathsForPosition.Count == 0) Debug.Log("NO MATCH | POS: " + transform.position);
        
        Vector3 outPosition = transform.position;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < availablePathsForPosition.Count; i++) {
            GridPath path = availablePathsForPosition[i];
            Vector3 candidate = path.GetClosestPointOnPath(targetPosition);

            float distance = Vector3.Distance(targetPosition, candidate);
            if (distance < closestDistance) {
                closestDistance = distance;
                outPosition = candidate;
            }
        }

        return outPosition;
    }

    private Vector3 NormalizePosition(Vector3 position) {
        return new Vector3(NormalizePointToGrid(position.x), position.y, NormalizePointToGrid(position.z));
    }

    private float NormalizePointToGrid(float v) {
        float snapDistance = Mathf.Abs(Mathf.Round(v) - v);
        if (snapDistance < 0.1f) {
            return Mathf.Round(v);
        } else {
            return v;
        }
    }

    private List<GridPath> GetAvailablePathsForPosition(Vector3 position) {
        List<GridPath> availablePathsForPosition = new List<GridPath>();
        foreach (GridPath path in availablePaths) {
            if (path.IsPointOnPath(position)) availablePathsForPosition.Add(path);
        }
        return availablePathsForPosition;
    }

    private void SetHighlight(bool highlight) {
        Renderer renderer = peg.GetComponent<Renderer>();
        if (renderer != null) {
            if (highlight) {
                renderer.material.color = Color.orange;
            } else {
                renderer.material.color = Color.white;
            }
        }
    }

    private void AdjustToGrid() {
        transform.position = new Vector3(Mathf.Round(transform.position.x), Util.defaultY, Mathf.Round(transform.position.z));
    }

    public GridCoord GridPosition() {
        return new GridCoord(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
    }

    public void SetFixedPosition(GridCoord position) {
        transform.position = new Vector3(position.x, Util.defaultY, position.z);
        isFixed = true;
    }

    public void SetFreeMove() {
        isFixed = false;
    }
}
