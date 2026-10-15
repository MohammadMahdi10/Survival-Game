using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class InventoryToggle : MonoBehaviour
{
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject darkOverlay;

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
    }
}