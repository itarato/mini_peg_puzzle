using UnityEngine;
using System.Collections.Generic;

public class GameController : MonoBehaviour
{
    public GameObject shapePrefab; // Reference to the shape prefab

    private List<GameObject> shapes = new List<GameObject>(); // List to hold the spawned shapes

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        shapes.Add(Instantiate(shapePrefab, Vector3.zero, Quaternion.identity));
    }

    // Update is called once per frame
    void Update()
    {
    }
}
