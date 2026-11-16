using System.Collections.Generic;
using UnityEngine;

/// Manages items on the ground near the player.
/// Loads nearby items into the ground inventory when it opens
/// and returns remaining items to the world when it closes.
public class GroundItems : MonoBehaviour
{
    [SerializeField] private GridInventory groundGrid;
    [SerializeField] private Transform player;
    [SerializeField] private float pickupRadius = 2f;
    [SerializeField] private float dropDistance = 1.5f;
    [SerializeField] private GridInventory backpack;

    private bool quitting;

    /// Records when the application is closing so ground items
    /// are not dropped again during shutdown.
    private void OnApplicationQuit() {
        quitting = true;

    }

    /// Collects nearby world pickups when the inventory opens.
    /// Items go straight into the backpack if they fit, no need to drag drop.
    /// Anything that doesn't fit goes to the ground grid so the player can rearrange and decide.
    /// Pickups only removed from the world once they have a place so they dont dissapear othwerise.
    private void OnEnable()
    {
        int toBackpack = 0, toGround = 0;

        // Iterate over a copy because destroyed pickups leave the active list.
        foreach (var pickup in new List<WorldPickup>(WorldPickup.All))
        {
            if (Vector3.Distance(player.position, pickup.transform.position) > pickupRadius)
                continue;

            if (backpack.TryAddItemAnyRotation(pickup.type) != null)
            {
                toBackpack++;
            }
            else if (groundGrid.TryAddItem(pickup.type) != null)
            {
                toGround++;
            }
            else
            {
                continue;   // no space, leave it in on the ground
            }

            Destroy(pickup.gameObject);
        }

        Debug.Log("[Pickup] backpack: " + toBackpack + ", ground: " + toGround);
    }

    /// Returns items remaining in the ground inventory to the world.
    /// This prevents items from disappearing when the panel closes.
    /// Skips this process during shutdown to avoid unnecessary item spawning.
    private void OnDisable()
    {
        if (quitting) return;

        foreach (var item in new List<PlacedItem>(groundGrid.Items))
        {
            groundGrid.Remove(item);
            Vector3 center = player.position + player.forward * dropDistance;
            WorldPickup.DropNear(item.type, center, 0.7f);
        }
    }
}