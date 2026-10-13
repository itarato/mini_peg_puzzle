using UnityEngine;
using System.Collections.Generic;
using Unity.Collections;

public class GameController : MonoBehaviour {
    public GameObject shapePrefab;
    public GameObject pegPrefab;

    private List<GameObject> shapes = new List<GameObject>();
    private List<GameObject> pegs = new List<GameObject>();

    private List<List<GridCoord>> shapeCoords = new List<List<GridCoord>> {
        //new List<GridCoord> { new GridCoord(0, 0)}, // Test shape

        new List<GridCoord> { new GridCoord(0, 0), new GridCoord(1, 0), new GridCoord(0, 1), new GridCoord(0, 2) }, // L shape
        new List<GridCoord> { new GridCoord(0, 0), new GridCoord(1, 0), new GridCoord(0, 1) }, // Mini L shape
        new List<GridCoord> { new GridCoord(0, 0), new GridCoord(1, 0), new GridCoord(0, 1), new GridCoord(1, 1) }, // Square shape
        new List<GridCoord> { new GridCoord(0, 0), new GridCoord(1, 0), new GridCoord(2, 0) }, // Line shape
        new List<GridCoord> { new GridCoord(0, 0), new GridCoord(1, 0), new GridCoord(-1, 0), new GridCoord(0, 1) }, // T shape
        new List<GridCoord> { new GridCoord(0, 0), new GridCoord(1, 0), new GridCoord(1, 1), new GridCoord(2, 1) }, // Snake shape
    };

    private List<GridCoord> shapeStartPositions = new List<GridCoord> {
        //new GridCoord(-5, 3),

        new GridCoord(-5, 2),
        new GridCoord(-4, 0),
        new GridCoord(-5, -2),
        new GridCoord(5, 2),
        new GridCoord(4, 0),
        new GridCoord(5, -2),
    };

    private List<List<GridPath>> pegCoords = new List<List<GridPath>> {
        new List<GridPath> { new GridPath(-2, 2, -1, 2), new GridPath(-2, 2, -2, 1), new GridPath(-2, 1, 1, 1), new GridPath(-1, 1, -1, -1) },
        new List<GridPath> { new GridPath(-2, -1, -2, -2), new GridPath(-2, -2, 0, -2), new GridPath(0, -2, 0, 0), new GridPath(0, 0, 1, 0) },
        new List<GridPath> { new GridPath(1, 2, 2, 2), new GridPath(2, 2, 2, -2), new GridPath(1, -2, 2, -2) },
    };

    void Start() {
        for (int i = 0; i < shapeCoords.Count; i++) {
            GameObject shape = Instantiate(shapePrefab, new Vector3(shapeStartPositions[i].x, ShapeController.defaultY, shapeStartPositions[i].z), Quaternion.identity);
            ShapeController shapeController = shape.GetComponent<ShapeController>();
            shapeController.Init(shapeCoords[i], shapeStartPositions[i]);
            shapes.Add(shape);
        }

        foreach (List<GridPath> paths in pegCoords) {
            GameObject peg = Instantiate(pegPrefab, new Vector3(paths[0].start.x, PegController.defaultY, paths[0].start.z), Quaternion.identity);
            PegController petController = peg.GetComponent<PegController>();
            petController.Init(paths);
            pegs.Add(peg);
        }

        //Debug.Log("GAME CTRL START");
        EvaluateAllShapesPositions();
    }

    void Update() {
    }

    private void OnEnable() {
        //Debug.Log("GAME ON-ENABLE START");
        //Debug.Log("GameController enabled, subscribing to OnShapePositionChanged event.");
        ShapeController.OnShapePositionChanged += OnShapePositionChanged;

    }

    private void OnDisable() {
        ShapeController.OnShapePositionChanged -= OnShapePositionChanged;
    }

    private void OnShapePositionChanged(ShapeController shapeController) {
        //Debug.Log("Shape position changed, re-evaluating all shapes.");
        EvaluateAllShapesPositions();
    }

    private void EvaluateAllShapesPositions() {
        HashSet<GridCoord> pegCoords = new HashSet<GridCoord>();
        Dictionary<GridCoord, ShapeController> occupiedCoords = new Dictionary<GridCoord, ShapeController>();

        foreach (GameObject peg in pegs) {
            PegController pegController = peg.GetComponent<PegController>();
            pegCoords.Add(pegController.GridPosition());
        }

        foreach (GameObject shape in shapes) {
            ShapeController shapeController = shape.GetComponent<ShapeController>();
            if (shapeController == null) continue;


            shapeController.SetProblem(false);

            foreach (GridCoord coord in shapeController.GridPositions()) {
                if (occupiedCoords.ContainsKey(coord)) {
                    shapeController.SetProblem(true);
                    occupiedCoords[coord].SetProblem(true);
                } else {
                    if (pegCoords.Contains(coord)) shapeController.SetProblem(true);

                    occupiedCoords.Add(coord, shapeController);
                }
            }
        }
    }
}
