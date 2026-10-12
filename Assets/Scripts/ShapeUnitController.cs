using UnityEngine;

public class ShapeUnitController : MonoBehaviour {
    public Material defaultMaterial;
    public Material highlightMaterial;
    public Material errorMaterial;

    private Renderer renderer;
    private bool hasProblem = false;
    private bool hasHighlight = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        renderer = GetComponent<Renderer>();
        UpdateMaterial();
    }

    // Update is called once per frame
    void Update() {
    }

    public void SetProblem(bool problem) {
        hasProblem = problem;
        UpdateMaterial();
    }

    private Material DefaultMaterial() {
        if (hasProblem) {
            Debug.Log("ERROR MATERIAL");
            return errorMaterial;
        } else {
            Debug.Log("DEFAULT MATERIAL");
            return defaultMaterial;
        }
    }

    public void SetHighlight(bool highlight) {
        hasHighlight = highlight;
        UpdateMaterial();
    }

    private void UpdateMaterial() {
        //Debug.Log($"ShapeUnitController: DefaultMaterial called. hasProblem={hasProblem}, hasHighlight={hasHighlight}");
        if (renderer == null) {
            Debug.LogWarning("ShapeUnitController: Renderer is null, cannot update material.");
            return;
        }

        if (hasHighlight) {
            renderer.material = highlightMaterial;
        } else {
            renderer.material = DefaultMaterial();
        }
    }
}
