using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// PLEASE NOTE: this script is mainly used for making a dummy player movement and not the actual player's script

/// Provides basic movement for testing the procedural world.
/// Uses a CharacterController so the player can collide with
/// terrain and invisible world boundaries without other systems.
public class TestPlayerMovement : MonoBehaviour {

    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float gravity = -20f;

    private CharacterController controller;
    private float verticalVelocity;
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float mouseSensitivity = 2f;

    private float cameraPitch;

    /// Rotates the player horizontally and the camera vertically.
    /// Locks the cursor and limits vertical rotation for
    /// first-person movement during world boundary testing.
    private void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LookAround() {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    private void Awake() {
        controller = GetComponent<CharacterController>();
    }

    /// Moves the test player using WASD and applies gravity.
    /// Uses collision-aware movement so the circular boundary
    /// can prevent the player from leaving the playable area.
    private void Update() {
        LookAround();
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;

        forward.y = 0f;
        right.y = 0f;

        Vector3 direction = (forward.normalized * vertical + right.normalized * horizontal).normalized;

        if (controller.isGrounded && verticalVelocity < 0f) {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 movement = direction * moveSpeed;
        movement.y = verticalVelocity;

        controller.Move(movement * Time.deltaTime);
    }
}
