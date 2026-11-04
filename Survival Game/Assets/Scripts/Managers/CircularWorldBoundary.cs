using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Generates an invisible circular wall around the playable world.
/// Prevents the player from travelling beyond the world border
/// while allowing the surrounding terrain to remain visible.
public class CircularWorldBoundary : MonoBehaviour {
    [Header("Boundary Settings")]
    [SerializeField] private float radius = 1100f;
    [SerializeField] private float wallHeight = 4000f;
    [SerializeField] private float wallThickness = 10f;
    [SerializeField] private int segments = 128;

    /// Creates a circular boundary from individual BoxColliders.
    /// Each segment follows the edge of the playable circle
    /// and extends vertically to block movement across terrain.
    [ContextMenu("Generate Boundary")]
    public void GenerateBoundary() {
        if (segments < 3 || radius <= 0f || wallHeight <= 0f || wallThickness <= 0f) {
            return;
        }

        Transform existing = transform.Find("GeneratedBoundary");

        if (existing != null) {
            if (Application.isPlaying) {
                Destroy(existing.gameObject);
            }
            else {
                DestroyImmediate(existing.gameObject);
            }
        }

        GameObject container = new GameObject("GeneratedBoundary");
        container.transform.SetParent(transform, false);

        float angleStep = Mathf.PI * 2f / segments;
        float segmentLength = 2f * radius * Mathf.Tan(Mathf.PI / segments) + wallThickness;

        for (int i = 0; i < segments; i++) {
            float angle = (i + 0.5f) * angleStep;
            Vector3 direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));

            GameObject wall = new GameObject("BoundarySegment_" + i);
            wall.transform.SetParent(container.transform, false);
            wall.transform.localPosition = direction * radius + Vector3.up * (wallHeight * 0.5f - 1000f);
            wall.transform.localRotation = Quaternion.LookRotation(direction);

            BoxCollider collider = wall.AddComponent<BoxCollider>();
            collider.size = new Vector3(segmentLength, wallHeight, wallThickness);
        }
    }
}

