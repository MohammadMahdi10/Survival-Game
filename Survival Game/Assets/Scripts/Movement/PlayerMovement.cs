
using UnityEngine;
using UnityEngine.InputSystem;

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

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

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

        // Only sprint when stamina is available.
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

    public void StopMovement()
    {
        moveInput = Vector2.zero;
        jumpRequested = false;
        stats.SetSprinting(false);
        velocity.x = 0f;
        velocity.z = 0f;
    }
}
