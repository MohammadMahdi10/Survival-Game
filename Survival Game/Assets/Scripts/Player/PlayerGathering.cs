using UnityEngine;
using UnityEngine.InputSystem;

/// Handles the player's resource gathering mechanic.
/// Allows the player to gather resources by clicking on them while looking at them.
public class PlayerGathering : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float gatherDistance = 4f;
    [SerializeField] private GameObject inventoryPanel;

    /// Checks every frame whether the player can gather resources.
    /// Attempts to gather a resource when the player clicks the left mouse button.
    /// Prevents gathering while the cursor is unlocked or the inventory is open.
    private void Update()
    {
        
        if (Cursor.lockState != CursorLockMode.Locked) return;
        if (inventoryPanel != null && inventoryPanel.activeSelf) return;

        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryGather();
        }
    }
    /// Checks whether the player is looking at a gatherable resource.
    /// Gathers the resource if it is within range and drops its collected items nearby.
    private void TryGather()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, gatherDistance))
        {
            GatherableResource resource = hit.collider.GetComponentInParent<GatherableResource>();

            if (resource != null)
            {
                resource.Gather();
                /// Drops the collected resources near the point where the player hit the gatherable object.
                for (int i = 0; i < resource.amount; i++)
                    WorldPickup.DropNear(resource.resourceType, hit.point, 0.8f);
            }
        }
    }
}