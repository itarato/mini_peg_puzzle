using UnityEngine;

public class ShapeUnitController : MonoBehaviour {
    public Material defaultMaterial;
    public Material highlightMaterial;

    private Renderer renderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        renderer = GetComponent<Renderer>();
        renderer.material = defaultMaterial;
    }

    // Update is called once per frame
    void Update() {

    }

    public void SetHighlight(bool highlight) {
        if (highlight) {
            renderer.material = highlightMaterial;
        } else {
            renderer.material = defaultMaterial;
        }
    }
}
