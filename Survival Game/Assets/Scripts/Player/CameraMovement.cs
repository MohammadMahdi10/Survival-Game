
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    public Transform playerBody;
    public float mouseSensitivity = 0.1f;

    private float xRotation = 0f;

    private void Start()
    {
        LockCursor();
    }

    private void Update()
    {
        // Unlock cursor using the Input Manager action
        if (InputManager.instance != null &&
            InputManager.instance.unlockCursorAction != null &&
            InputManager.instance.unlockCursorAction.WasPressedThisFrame())
        {
            UnlockCursor();
        }

        // Click to lock the cursor again
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame &&
            Cursor.lockState != CursorLockMode.Locked)
        {
            LockCursor();
        }

        // Don't move the camera while the cursor is unlocked
        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        if (Mouse.current == null)
            return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        float mouseX = mouseDelta.x * mouseSensitivity;
        float mouseY = mouseDelta.y * mouseSensitivity;

        // Look up and down
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Look left and right
        if (playerBody != null)
        {
            playerBody.Rotate(Vector3.up * mouseX);
        }
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
