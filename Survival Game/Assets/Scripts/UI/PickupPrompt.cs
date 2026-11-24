using TMPro;
using UnityEngine;

/// Shows pickup prompt when the player is near world pickups.
/// Added so players know what to press to pick up items.
public class PickupPrompt : MonoBehaviour
{
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private Transform player;
    [SerializeField] private GroundItems groundItems;

    private void Update()
    {
        // The cursor is only unlocked in menus, so hide the prompt there.
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            promptText.enabled = false;
            return;
        }

        // Use the same radius as the pickup itself, so the prompt only
        // appears when pressing E would actually collect something.
        float radius = groundItems.PickupRadius;
        int count = 0;
        WorldPickup nearby = null;

        foreach (var pickup in WorldPickup.All)
        {
            if (Vector3.Distance(player.position, pickup.transform.position) <= radius)
            {
                count++;
                nearby = pickup;
            }
        }

        promptText.enabled = count > 0;

        if (count == 1)
            promptText.text = "[E] Pick up " + ItemShapes.GetName(nearby.type);
        else if (count > 1)
            promptText.text = "[E] Pick up " + count + " items";
    }
}