
using UnityEngine;
using UnityEngine.InputSystem;

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
