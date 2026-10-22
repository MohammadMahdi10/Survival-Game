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

    private bool quitting;

    /// Records when the application is closing so ground items
    /// are not dropped again during shutdown.
    private void OnApplicationQuit() {
        quitting = true;

    }

    /// Loads nearby world pickups into the ground inventory when opened.
    /// Only removes pickups from the world after they successfully fit
    /// into the grid, preventing items from being lost.
    private void OnEnable()
    {
        int loaded = 0;

        // Iterate over a copy because pickups may be removed from the active list.
        foreach (var pickup in new List<WorldPickup>(WorldPickup.All))
        {
            if (Vector3.Distance(player.position, pickup.transform.position) > pickupRadius)
                continue;

            if (groundGrid.TryAddItem(pickup.type) != null)
            {
                Destroy(pickup.gameObject);
                loaded++;
            }
        }
        Debug.Log("[Ground] opened: loaded " + loaded + ", ground grid now has " + groundGrid.Items.Count);
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