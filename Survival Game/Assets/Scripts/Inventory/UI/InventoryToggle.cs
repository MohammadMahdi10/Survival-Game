using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryToggle : MonoBehaviour
{
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject craftingMenu;
    [SerializeField] private GameObject darkOverlay;
    [SerializeField] private GameObject groundPanel;

    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private MonoBehaviour mouseLook;
    private bool gameWon = false;

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            Toggle();
    }

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

    public void DisableInventory() {
        gameWon = true;
    }
}