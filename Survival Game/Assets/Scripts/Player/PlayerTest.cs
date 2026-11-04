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

    private void Awake() {
        controller = GetComponent<CharacterController>();
    }

    /// Moves the test player using WASD and applies gravity.
    /// Uses collision-aware movement so the circular boundary
    /// can prevent the player from leaving the playable area.
    private void Update() {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (controller.isGrounded && verticalVelocity < 0f) {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 movement = direction * moveSpeed;
        movement.y = verticalVelocity;

        controller.Move(movement * Time.deltaTime);
    }
}
