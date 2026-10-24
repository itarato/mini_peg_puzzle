using UnityEngine;
using System.Collections.Generic;

public class GameController : MonoBehaviour {
    public GameObject shapePrefab;
    public GameObject pegPrefab;

    private List<GameObject> shapes = new List<GameObject>();
    private List<GameObject> pegs = new List<GameObject>();
    private ShapeController lastTouchedShape;
    private int hintCount = 0;
    private List<ShapeSource> solution = new List<ShapeSource>();

    private List<List<GridCoord>> shapeCoords = new List<List<GridCoord>> {
        new List<GridCoord> {
            new GridCoord(0, 0),
            new GridCoord(1, 0),
            new GridCoord(0, -1), 
            new GridCoord(0, -2),
        }, // L shape
        new List<GridCoord> { 
            new GridCoord(0, 0), 
            new GridCoord(1, 0), 
            new GridCoord(0, -1),
        }, // Mini L shape
        new List<GridCoord> { 
            new GridCoord(0, 0), 
            new GridCoord(0, -1), 
            new GridCoord(0, -2),
        }, // Line shape
        new List<GridCoord> { 
            new GridCoord(0, 0), 
            new GridCoord(1, 0), 
            new GridCoord(1, -1), 
            new GridCoord(2, -1), 
        }, // Snake shape
        new List<GridCoord> { 
            new GridCoord(0, 0), 
            new GridCoord(1, 0), 
            new GridCoord(0, -1), 
            new GridCoord(1, -1), 
        }, // Square shape
        new List<GridCoord> { 
            new GridCoord(0, 0), 
            new GridCoord(1, 0), 
            new GridCoord(2, 0),
            new GridCoord(1, -1),
        }, // T shape
    };

    private List<GridCoord> shapeStartPositions = new List<GridCoord> {
        new GridCoord(-3, -4),
        new GridCoord(-1, 6),
        new GridCoord(-3, 6),
        new GridCoord(-1, -5),
        new GridCoord(2, -4),
        new GridCoord(1, 5),
    };

    private List<List<GridPath>> pegCoords = new List<List<GridPath>> {
        new List<GridPath> { new GridPath(-2, 2, -1, 2), new GridPath(-2, 2, -2, 1), new GridPath(-2, 1, 1, 1), new GridPath(-1, 1, -1, -1) },
        new List<GridPath> { new GridPath(1, 2, 2, 2), new GridPath(2, 2, 2, -2), new GridPath(1, -2, 2, -2) },
        new List<GridPath> { new GridPath(-2, -1, -2, -2), new GridPath(-2, -2, 0, -2), new GridPath(0, -2, 0, 0), new GridPath(0, 0, 1, 0) },
    };

    void Start() {
        for (int i = 0; i < shapeCoords.Count; i++) {
            GameObject shape = Instantiate(shapePrefab, new Vector3(shapeStartPositions[i].x, Util.defaultY, shapeStartPositions[i].z), Quaternion.identity);
            ShapeController shapeController = shape.GetComponent<ShapeController>();
            shapeController.Init(shapeCoords[i], this);
            shapes.Add(shape);
        }

        foreach (List<GridPath> paths in pegCoords) {
            GameObject peg = Instantiate(pegPrefab, new Vector3(paths[0].start.x, Util.defaultY, paths[0].start.z), Quaternion.identity);
            PegController petController = peg.GetComponent<PegController>();
            petController.Init(paths);
            pegs.Add(peg);
        }

        lastTouchedShape = shapes[0].GetComponent<ShapeController>();

        EvaluateAllShapesPositions();

        SetupRandomLevel();
    }

    public void SetLastTouchedShape(ShapeController shape) {
        lastTouchedShape = shape;
    }

    void Update() {
    }

    private void ResetShapePositions() {
        for (int i = 0; i < shapes.Count; i++) {
            shapes[i].transform.position = new Vector3(shapeStartPositions[i].x, Util.defaultY, shapeStartPositions[i].z);
            shapes[i].transform.rotation = Quaternion.identity;

            shapes[i].GetComponent<ShapeController>().Reset();
        }
    }

    private void OnEnable() {
        ShapeController.OnShapePositionChanged += OnShapePositionChanged;
    }

    private void OnDisable() {
        ShapeController.OnShapePositionChanged -= OnShapePositionChanged;
    }

    private void OnShapePositionChanged(ShapeController shapeController) {
        EvaluateAllShapesPositions();
    }

    private void EvaluateAllShapesPositions() {
        bool isComplete = true;

        HashSet<GridCoord> pegCoords = new HashSet<GridCoord>();
        Dictionary<GridCoord, ShapeController> occupiedCoords = new Dictionary<GridCoord, ShapeController>();

        foreach (GameObject peg in pegs) {
            PegController pegController = peg.GetComponent<PegController>();
            pegCoords.Add(pegController.GridPosition());
        }

        foreach (GameObject shape in shapes) {
            ShapeController shapeController = shape.GetComponent<ShapeController>();
            if (shapeController == null) return;

            shapeController.SetProblem(false);

            foreach (GridCoord coord in shapeController.GridPositions()) {
                if (occupiedCoords.ContainsKey(coord)) {
                    shapeController.SetProblem(true);
                    occupiedCoords[coord].SetProblem(true);

                    isComplete = false;
                } else {
                    if (pegCoords.Contains(coord)) {
                        shapeController.SetProblem(true);
                        isComplete = false;
                    }

                    occupiedCoords.Add(coord, shapeController);
                }

                if (coord.x < -2 || coord.x > 2 || coord.z < -2 || coord.z > 2) isComplete = false;
            }
        }

        if (isComplete) StartCompletionPhase();
    }

    public void SetupRandomLevel() {
        System.Random rand = new System.Random();
        SetupLevel(rand.Next(0, 6884));
    }

    public void SetupLevel(int level) {
        ResetShapePositions();

        hintCount = 0;

        string encoded = LoadLevelString(level);
        string[] shape_parts_raw = encoded.Split(';');

        solution.Clear();

        for (int i = 0; i < 6; i++) {
            string[] unit_parts_raw = shape_parts_raw[i].Split(',');
            int x = int.Parse(unit_parts_raw[0]);
            int z = int.Parse(unit_parts_raw[1]);
            int rotation = int.Parse(unit_parts_raw[2]);
            bool is_mirrored = unit_parts_raw[3] == "1";

            ShapeSource source = new ShapeSource(i, x - 2, 2 - z, rotation, is_mirrored);
            solution.Add(source);
        }

        string[] peg_parts = shape_parts_raw[6].Split(',');
        for (int i = 0; i < 3; i++) {
            int mask = int.Parse(peg_parts[i]);
            int pegx = (mask % 5) - 2;
            int pegz = 2 - (mask / 5);

            PegController pegController = pegs[i].GetComponent<PegController>();
            pegController.SetFixedPosition(new GridCoord(pegx, pegz));
        }

        ResetAllShapeMaterials();
    }

    private string LoadLevelString(int level) {
        TextAsset content = Resources.Load<TextAsset>("solutions");
        if (content == null) {
            Debug.LogError("File not found");
            return "";
        }

        string[] lines = content.text.Split("\n");
        return lines[level];
    }

    public void RotateLastTouchedShape() {
        lastTouchedShape.Rotate();
    }

    public void MirrorLastTouchedShape() {
        lastTouchedShape.Mirror();
    }

    public void ResetAllShapeMaterials() {
        foreach (GameObject shape in shapes) {
            shape.GetComponent<ShapeController>().ResetMaterial();
        }
    }

    public void SetupFreeMode() {
        foreach (var peg in pegs) {
            peg.GetComponent<PegController>().SetFreeMove();
        }
        ResetShapePositions();
    }

    public void GiveHint() {
        if (hintCount >= 6) return;

        ResetShapePositions();

        hintCount++;

        for (int i = 0; i < hintCount; i++) {
            ShapeSource source = solution[i];
            shapes[source.index].GetComponent<ShapeController>().ApplySource(source);
        }

        ResetAllShapeMaterials();
        EvaluateAllShapesPositions();
    }

    private void StartCompletionPhase() {
        foreach (var shape in shapes) {
            shape.GetComponent<ShapeController>().StartCompletionPhase();
        }

        Invoke(nameof(SetupRandomLevel), 2f);
    }
}
