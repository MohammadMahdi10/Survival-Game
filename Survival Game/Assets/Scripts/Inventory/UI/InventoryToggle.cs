using UnityEngine;
using UnityEngine.InputSystem;

/// Controls opening and closing the player's inventory interface.
/// Also pauses player movement, manages the mouse cursor,
/// and prevents the inventory from opening after winning the game.
public class InventoryToggle : MonoBehaviour
{
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject craftingMenu;
    [SerializeField] private GameObject darkOverlay;
    [SerializeField] private GameObject groundPanel;

    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private MonoBehaviour mouseLook;
    private bool gameWon = false;

    /// Checks every frame whether the player presses E
    /// to open or close the inventory.
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            Toggle();
    }

    /// Opens or closes the inventory, crafting menu and ground panel.
    /// Disables movement and mouse look while the inventory is open
    /// to prevent the player from moving or attacking while using the UI.
    /// Restores player controls when the inventory closes.
    public void Toggle(){

        if (gameWon) {
            return;
        }


        bool isOpen = !inventoryPanel.activeSelf;
        
        inventoryPanel.SetActive(isOpen);
        groundPanel.SetActive(isOpen);
        craftingMenu.SetActive(isOpen);
        darkOverlay.SetActive(isOpen);

        if (isOpen)
        {
            playerMovement.StopMovement();
        }

        playerMovement.enabled = !isOpen;
        mouseLook.enabled = !isOpen;

        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isOpen;

    }

    /// Locks inventory toggling after the player wins.
    /// This prevents inventory input from interfering with the victory screen.
    public void DisableInventory() {
        gameWon = true;
    }
}