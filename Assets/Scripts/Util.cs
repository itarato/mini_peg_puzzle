using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Util {
    public const float defaultY = 0f;
    private static Plane dragPlane = new Plane(Vector3.up, new Vector3(0f, defaultY, 0f));

    public static TouchKind CreateTouch(float timespan, float distance) {
        if (distance > 5f) return TouchKind.Regular;
        if (timespan < 0.2f) return TouchKind.Short;
        if (timespan < 1f) return TouchKind.Long;
        return TouchKind.Regular;
    }

    public static Vector3 GetCurrentMouseWorldPoint() {
        Vector2 mousePosition = TouchPosition();
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        if (dragPlane.Raycast(ray, out float distance)) {
            Vector3 point = ray.GetPoint(distance);
            point.y = defaultY;
            return point;
        }

        return Vector3.zero;
    }

    public static Vector2 TouchPosition() {
        if (Touchscreen.current != null) {
            if (Touchscreen.current.press.wasPressedThisFrame || Touchscreen.current.press.wasReleasedThisFrame) {
                return Touchscreen.current.primaryTouch.position.ReadValue();
            } else {
                return Touchscreen.current.position.ReadValue();
            }
        } else {
            return Mouse.current.position.ReadValue();
        }
    }

    public static void ApplyMaterialToList(List<GameObject> gameObjects, Material material, int materialIndex) {
        foreach (var gameObject in gameObjects) {
            Renderer renderer = gameObject.GetComponent<Renderer>();
            if (renderer == null) return;

            Material[] materials = renderer.materials;

            if (materials.Length <= materialIndex) System.Array.Resize(ref materials, materialIndex + 1);

            materials[materialIndex] = material;
            renderer.materials = materials;
        }
    }

    public static void ChangeColor(List<GameObject> gameObjects, Color color) {
        if (color == null) return;

        foreach (var gameObject in gameObjects) {
            Renderer renderer = gameObject.GetComponent<Renderer>();
            if (renderer == null) return;
            renderer.material.color = color;
        }
    }
}
