using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PegController : MonoBehaviour {
    public GameObject peg;

    private List<GridPath> availablePaths;
    public const float defaultY = 0f;

    private bool isDragging = false;
    private bool isHovering = false;
    private Plane dragPlane = new Plane(Vector3.up, new Vector3(0f, defaultY, 0f));
    private Vector3 dragOffset = Vector3.zero;

    private bool isFixed = false;

    public void Init(List<GridPath> availablePaths) {
        this.availablePaths = availablePaths;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        HandleMouseOperations();
    }

    private void HandleMouseOperations() {
        if (isFixed) return;

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
                Vector3 newPositionCandidate = point + dragOffset;

                transform.position = GetOnLinePositionForCandidate(newPositionCandidate);
            }
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame) {
            isDragging = false;
            AdjustToGrid();
        }
    }

    private Vector3 GetOnLinePositionForCandidate(Vector3 targetPosition) {
        targetPosition = NormalizePosition(targetPosition);
        List<GridPath> availablePathsForPosition = GetAvailablePathsForPosition(transform.position);
        
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
                renderer.material.color = Color.yellow;
            } else {
                renderer.material.color = Color.white;
            }
        }
    }

    private void AdjustToGrid() {
        transform.position = new Vector3(Mathf.Round(transform.position.x), defaultY, Mathf.Round(transform.position.z));
    }

    public GridCoord GridPosition() {
        return new GridCoord(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
    }

    public void SetFixedPosition(GridCoord position) {
        transform.position = new Vector3(position.x, defaultY, position.z);
        isFixed = true;
    }

    public void SetFreeMove() {
        isFixed = false;
    }
}
