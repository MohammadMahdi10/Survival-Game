using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryToggle : MonoBehaviour
{
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject craftingMenu;
    [SerializeField] private GameObject darkOverlay;

    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private MonoBehaviour mouseLook;
    [SerializeField] private CraftingUI craftingUI;

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            Toggle();
    }

    public void Toggle()
    {
        bool isOpen = !inventoryPanel.activeSelf;

        inventoryPanel.SetActive(isOpen);
        craftingMenu.SetActive(isOpen);
        darkOverlay.SetActive(isOpen);

        if (isOpen)
        {
            playerMovement.StopMovement();
            craftingUI.UpdateMaterialCounters();
        }

        playerMovement.enabled = !isOpen;
        mouseLook.enabled = !isOpen;

        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isOpen;

    }
}