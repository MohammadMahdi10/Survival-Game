using UnityEngine;
using UnityEngine.InputSystem;

/// Handles the player's interaction with the central tower.
/// Allows the player to place a special item in the tower to progress the quest and activate the final boss.
public class TowerInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private GridInventory playerInventory;
    [SerializeField] private QuestManager questManager;
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private GameObject finalBoss;

    [Header("Settings")]
    [SerializeField] private float interactionDistance = 4f;

    /// Checks whether the player presses the interaction button.
    /// Prevents interaction when the input system is unavailable or the cursor is unlocked.
    private void Update() {
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

            TryInteract();
        }
    }

    /// Checks whether the player can interact with the tower.
    /// Requires the player to be within range and at the correct quest stage.
    /// Removes a special item from the inventory, progresses the quest, and activates the final boss.
    private void TryInteract() {
        if (player == null || playerInventory == null || questManager == null || interactionPoint == null)
        {
            return;
        }

        float distance = Vector3.Distance(player.position, interactionPoint.position);

        if (distance > interactionDistance) {
            return;
        }

        if (questManager.GetCurrentStage() != QuestManager.QuestStage.Tower) {
            return;
        }

        bool removed = playerInventory.RemoveOne(ItemType.SpecialItem);

        if (!removed) {
            return;
        }

        questManager.StonePlaced();

        if (finalBoss != null) {
            finalBoss.SetActive(true);
        }
    }
}
