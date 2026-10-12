using System.Collections.Generic;
using Unity.AppUI.Core;
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
}
