using UnityEngine;
using UnityEngine.InputSystem;

/// Handles the player's camera movement.
/// Allows the player to look around using the mouse and rotate the player body horizontally.
public class CameraMovement : MonoBehaviour
{
    public Transform playerBody;
    public float mouseSensitivity = 0.1f;

    private float xRotation = 0f;

    /// Locks the cursor when the game starts.
    /// Hides the cursor to allow the player to control the camera.
    private void Start()
    {
        LockCursor();
    }

    /// Checks whether the player presses escape to unlock the cursor.
    /// Locks the cursor again when the player clicks the left mouse button.
    /// Updates the camera's vertical rotation and the player's horizontal rotation based on mouse movement.
    private void Update()
    {
        if (InputManager.instance != null &&
            InputManager.instance.unlockCursorAction != null &&
            InputManager.instance.unlockCursorAction.WasPressedThisFrame())
        {
            UnlockCursor();
        }

        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame &&
            Cursor.lockState != CursorLockMode.Locked)
        {
            LockCursor();
        }

        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        if (Mouse.current == null)
            return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        float mouseX = mouseDelta.x * mouseSensitivity;
        float mouseY = mouseDelta.y * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        if (playerBody != null)
        {
            playerBody.Rotate(Vector3.up * mouseX);
        }
    }

    /// Unlocks the cursor and makes it visible.
    /// Allows the player to interact with the mouse freely.
    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// Locks the cursor and makes it invisible.
    /// Allows the player to control the camera using mouse movement.
    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
