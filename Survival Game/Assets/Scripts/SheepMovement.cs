using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheepMovement : MonoBehaviour {

    [SerializeField] private Transform sheepVisuals;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float moveDistance = 4f;

    private Vector3 startPosition;
    private Vector3 endPosition;
    private Quaternion originalRotation;

    private bool movingForward = true;

    private void Start() {
        startPosition = transform.position;
        endPosition = startPosition + transform.right * moveDistance;
        originalRotation = sheepVisuals.localRotation;
    }

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

            if (movingForward) {
                sheepVisuals.localRotation = originalRotation;
            }
            else {
                sheepVisuals.localRotation = originalRotation * Quaternion.Euler(0f, 180f, 0f);
            }
        }
    }
}
