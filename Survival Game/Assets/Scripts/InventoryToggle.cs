using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class InventoryToggle : MonoBehaviour
{
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject darkOverlay;

    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private MonoBehaviour mouseLook;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
            Toggle();
    }

    public void Toggle()
    {
        bool isOpen = !inventoryPanel.activeSelf;

        inventoryPanel.SetActive(isOpen);
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
}