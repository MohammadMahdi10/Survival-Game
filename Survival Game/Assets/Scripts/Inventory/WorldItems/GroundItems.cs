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

    public float PickupRadius => pickupRadius; // so prompt appears in same radius as pickup

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
        var picked = new Dictionary<ItemType, int>();
        int toGround = 0, leftBehind = 0;

        // Iterate over a copy because destroyed pickups leave the list of active items.
        foreach (var pickup in new List<WorldPickup>(WorldPickup.All))
        {
            if (Vector3.Distance(player.position, pickup.transform.position) > pickupRadius)
                continue;

            if (backpack.TryAddItemAnyRotation(pickup.type) != null)
            {
                picked.TryGetValue(pickup.type, out int n);
                picked[pickup.type] = n + 1;
            }
            else if (groundGrid.TryAddItem(pickup.type) != null)
            {
                toGround++;
            }
            else
            {
                leftBehind++;   // no room anywhere, leave it in the world
                continue;
            }

            Destroy(pickup.gameObject);
        }

        if (picked.Count > 0 || toGround > 0 || leftBehind > 0)
            PromptUI.Show(BuildPickupMessage(picked, toGround, leftBehind));
    }

    /// Builds a summary of action, e.g. "Picked up 2x Wood",
    /// and a warning line when the backpack couldn't fit everything.
    private static string BuildPickupMessage(Dictionary<ItemType, int> picked, int toGround, int leftBehind)
    {
        var lines = new List<string>();

        if (picked.Count > 0)
        {
            var parts = new List<string>();
            foreach (var entry in picked)
                parts.Add(entry.Value + "x " + ItemShapes.GetName(entry.Key));
            lines.Add("Picked up " + string.Join(", ", parts));
        }

        if (toGround > 0)
            lines.Add("Backpack full: " + toGround + " item(s) in Ground panel");

        if (leftBehind > 0)
            lines.Add("No room for " + leftBehind + " item(s)");

        return string.Join("\n", lines);
    }


    /// Returns items remaining in the ground inventory to the world,
    /// so items aren't lost when the panel closes.
    /// Skips this during shutdown to avoid spawning items while quitting.
    private void OnDisable()
    {
        if (quitting) return;

        int dropped = groundGrid.Items.Count;

        foreach (var item in new List<PlacedItem>(groundGrid.Items))
        {
            groundGrid.Remove(item);
            Vector3 center = player.position + player.forward * dropDistance;
            WorldPickup.DropNear(item.type, center, 0.7f);
        }

        if (dropped > 0)
            PromptUI.Show("Dropped " + dropped + " item(s)");
    }
}