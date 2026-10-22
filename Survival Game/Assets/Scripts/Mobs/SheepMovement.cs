using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Controls a sheep's simple wandering movement.
/// Moves the sheep back and forth between two positions
/// to create movement without needing complex AI or pathfinding.
public class SheepMovement : MonoBehaviour {

    [SerializeField] private Transform sheepVisuals;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float moveDistance = 4f;

    private Vector3 startPosition;
    private Vector3 endPosition;
    private Quaternion originalRotation;

    private bool movingForward = true;


    /// Records the sheep's starting position and visual rotation.
    /// Uses these values to define its movement path and restore
    /// its original facing direction when it turns around.
    private void Start() {
        startPosition = transform.position;
        endPosition = startPosition + transform.right * moveDistance;
        originalRotation = sheepVisuals.localRotation;
    }


    /// Moves the sheep between its starting and ending positions.
    /// Reverses direction at each endpoint so the sheep
    /// continuously patrols the same short path.
    private void Update() {
        Vector3 destination;

        if (movingForward) {
            destination = endPosition;
        }
        else {
            destination = startPosition;
        }

        transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, destination) < 0.05f) {
            movingForward = !movingForward;


            // Rotate only the visual model so the sheep faces its
            // travel direction without changing the movement path.
            if (movingForward) {
                sheepVisuals.localRotation = originalRotation;
            }
            else {
                sheepVisuals.localRotation = originalRotation * Quaternion.Euler(0f, 180f, 0f);
            }
        }
    }
}
