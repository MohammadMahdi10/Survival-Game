
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector3 velocity;
    private bool jumpRequested;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    // Gets input from input system
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    // Gets input from input system
    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            jumpRequested = true;
        }
    }

    private void Update()
    {
        bool grounded = controller.isGrounded;

        if (grounded && velocity.y < 0f)
            velocity.y = -2f;

        // Moves player in x and z axis and keep distance constant
        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y);
        move = Vector3.ClampMagnitude(move, 1f);

        // Changes direction when player rotated
        Vector3 movement = transform.right * move.x + transform.forward * move.z;

        controller.Move(movement * walkSpeed * Time.deltaTime);

        // Checks and causes player jump
        if (jumpRequested && grounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        jumpRequested = false;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
