using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;

    [Header("Input Actions")]
    public InputActionAsset controls;

    public InputAction moveAction;
    public InputAction jumpAction;
    public InputAction sprintAction;
    public InputAction unlockCursorAction;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        if (controls == null)
        {
            Debug.LogError("InputManager: No Input Actions asset assigned!");
            return;
        }

        moveAction = controls.FindAction("Gameplay/Move", false);
        jumpAction = controls.FindAction("Gameplay/Jump", false);
        unlockCursorAction = controls.FindAction("Gameplay/UnlockCursor", false);
        sprintAction = controls.FindAction("Gameplay/Sprint", false);

    }

    private void OnEnable()
    {
        if (controls != null)
            controls.Enable();
    }

    private void OnDisable()
    {
        if (controls != null)
            controls.Disable();
    }

}
