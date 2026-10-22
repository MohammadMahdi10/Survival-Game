using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGathering : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float gatherDistance = 4f;
    [SerializeField] private GameObject inventoryPanel;

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

    private void TryGather()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, gatherDistance))
        {
            GatherableResource resource = hit.collider.GetComponentInParent<GatherableResource>();

            if (resource != null)
            {
                resource.Gather();

                for (int i = 0; i < resource.amount; i++)
                    WorldPickup.DropNear(resource.resourceType, hit.point, 0.8f);
            }
        }
    }
}