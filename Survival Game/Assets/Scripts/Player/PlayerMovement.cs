using UnityEngine;
using UnityEngine.InputSystem;

/// Handles the player's movement.
/// Allows the player to walk, sprint, and jump while applying gravity and managing stamina.
public class PlayerMovement : MonoBehaviour
{
    public PlayerStats stats;
    public float walkSpeed = 5f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;
    public float sprintSpeed = 10f;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector3 velocity;
    private bool jumpRequested;
    private bool sprintHeld;

    /// Gets the CharacterController component attached to the player.
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    /// Reads the player's movement, sprint, and jump inputs.
    /// Updates the player's movement, applies gravity, and allows jumping when grounded.
    /// Prevents sprinting when stamina is unavailable or the player is not moving.
    private void Update()
    {
        if (InputManager.instance == null ||
            InputManager.instance.moveAction == null ||
            InputManager.instance.sprintAction == null ||
            InputManager.instance.jumpAction == null)
        {
            stats.SetSprinting(false);
            return;
        }

        moveInput = InputManager.instance.moveAction.ReadValue<Vector2>();
        jumpRequested = InputManager.instance.jumpAction.WasPressedThisFrame();
        sprintHeld = InputManager.instance.sprintAction.IsPressed();

        bool grounded = controller.isGrounded;

        if (grounded && velocity.y < 0f)
            velocity.y = -2f;

        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y);
        move = Vector3.ClampMagnitude(move, 1f);

        Vector3 movement =
            transform.right * move.x +
            transform.forward * move.z;

        bool canSprint = sprintHeld &&
                         Mathf.Floor(stats.currentStamina) > 0f &&
                         move.sqrMagnitude > 0f;

        stats.SetSprinting(canSprint);

        float speed = canSprint ? sprintSpeed : walkSpeed;

        controller.Move(movement * speed * Time.deltaTime);

        if (jumpRequested && grounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    /// Stops the player's movement and cancels any pending jump.
    /// Disables sprinting and resets horizontal velocity.
    public void StopMovement()
    {
        moveInput = Vector2.zero;
        jumpRequested = false;
        stats.SetSprinting(false);
        velocity.x = 0f;
        velocity.z = 0f;
    }
}
