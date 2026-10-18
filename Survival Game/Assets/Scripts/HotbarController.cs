using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HotbarController : MonoBehaviour {
    private int selectedSlot = 0;
    [SerializeField] private Transform backpackGrid;

    private void Update() {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            SelectSlot(0);
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            SelectSlot(1);
        }

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            SelectSlot(2);
        }

        if (Keyboard.current.digit4Key.wasPressedThisFrame)
        {
            SelectSlot(3);
        }

        if (Keyboard.current.digit5Key.wasPressedThisFrame)
        {
            SelectSlot(4);
        }
    }

    private void SelectSlot(int index) {
        selectedSlot = index;
        Transform slot = backpackGrid.GetChild(selectedSlot);
    }
}
