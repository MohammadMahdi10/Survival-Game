using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpaceshipInteraction : MonoBehaviour {

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private GridInventory playerInventory;
    [SerializeField] private QuestManager questManager;
    [SerializeField] private GameObject interactionPoint;
    [SerializeField] private GameObject winScreen;
    [SerializeField] private InventoryToggle inventoryToggle;

    [Header("Settings")]
    [SerializeField] private float interactionDistance = 4f;

    private bool repaired = false;

    private void Update() {
        if (repaired) {
            return;
        }

        if (InputManager.instance == null) {
            return;
        }

        InputAction interact = InputManager.instance.interactAction;

        if (interact == null) {
            return;
        }

        if (interact.WasPressedThisFrame()) {
            if (Cursor.lockState != CursorLockMode.Locked) {
                return;
            }

            TryRepairSpaceship();
        }
    }

    private void TryRepairSpaceship()
    {
        if (player == null || playerInventory == null ||
            questManager == null || interactionPoint == null) {
            return;
        }

        float distance = Vector3.Distance(player.position, interactionPoint.transform.position);

        if (distance > interactionDistance) {
            return;
        }

        if (questManager.GetCurrentStage() != QuestManager.QuestStage.ReturnToSpaceship) {
            return;
        }

        bool removed = playerInventory.RemoveOne(ItemType.RepairComponent);

        if (!removed) {
            return;
        }

        transform.rotation = Quaternion.identity;
        interactionPoint.SetActive(false);

        repaired = true;

        questManager.SpaceshipRepaired();
        inventoryToggle.DisableInventory();
        winScreen.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;

    }
}
