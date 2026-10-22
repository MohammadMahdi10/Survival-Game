using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// Manages the player's input actions in one central location.
/// Allows other scripts to access the same controls without
/// each script needing to configure its own input actions.
public class InputManager : MonoBehaviour
{
    public static InputManager instance;

    [Header("Input Actions")]
    public InputActionAsset controls;

    public InputAction moveAction;
    public InputAction jumpAction;
    public InputAction sprintAction;
    public InputAction unlockCursorAction;
    public InputAction interactAction;

    /// Sets up the shared InputManager instance and finds its input actions.
    /// Prevents duplicate managers from creating conflicting input references.
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;


        // Avoid looking up actions when no input asset has been assigned.
        if (controls == null)
        {
            return;
        }

        moveAction = controls.FindAction("Gameplay/Move", false);
        jumpAction = controls.FindAction("Gameplay/Jump", false);
        unlockCursorAction = controls.FindAction("Gameplay/UnlockCursor", false);
        sprintAction = controls.FindAction("Gameplay/Sprint", false);
        interactAction = controls.FindAction("Gameplay/Interact", false);
    }

    /// Enables the input actions when this component becomes active
    /// so they can respond to player input.
    private void OnEnable()
    {
        if (controls != null)
        {
            controls.Enable();
        }
    }

    /// Disables the input actions when this component is inactive
    /// to prevent them from remaining active unnecessarily.
    private void OnDisable()
    {
        if (controls != null)
        {
            controls.Disable();
        }
    }
}
