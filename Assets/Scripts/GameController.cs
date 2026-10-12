using UnityEngine;
using System.Collections.Generic;

public class GameController : MonoBehaviour {
    public GameObject shapePrefab; // Reference to the shape prefab

    private List<GameObject> shapes = new List<GameObject>(); // List to hold the spawned shapes

    private List<List<GridCoord>> shapeCoords = new List<List<GridCoord>> {
        new List<GridCoord> { new GridCoord(0, 0), new GridCoord(1, 0), new GridCoord(0, 1), new GridCoord(0, 2) }, // L shape
        new List<GridCoord> { new GridCoord(0, 0), new GridCoord(1, 0), new GridCoord(0, 1) }, // Mini L shape
        new List<GridCoord> { new GridCoord(0, 0), new GridCoord(1, 0), new GridCoord(0, 1), new GridCoord(1, 1) }, // Square shape
        new List<GridCoord> { new GridCoord(0, 0), new GridCoord(1, 0), new GridCoord(2, 0) }, // Line shape
        new List<GridCoord> { new GridCoord(0, 0), new GridCoord(1, 0), new GridCoord(-1, 0), new GridCoord(0, 1) }, // T shape
        new List<GridCoord> { new GridCoord(0, 0), new GridCoord(1, 0), new GridCoord(1, 1), new GridCoord(2, 1) }, // Snake shape
    };

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        foreach (List<GridCoord> coords in shapeCoords) {
            GameObject shape = Instantiate(shapePrefab, Vector3.zero, Quaternion.identity);
            ShapeController shapeController = shape.GetComponent<ShapeController>();
            if (shapeController != null) {
                shapeController.Init(coords);
            }
            shapes.Add(shape);
        }
    }

    // Update is called once per frame
    void Update() {
    }
}
