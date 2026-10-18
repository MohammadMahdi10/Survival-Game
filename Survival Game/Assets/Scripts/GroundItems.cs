using System.Collections.Generic;
using UnityEngine;

public class GroundItems : MonoBehaviour
{
    [SerializeField] private GridInventory groundGrid;
    [SerializeField] private Transform player;
    [SerializeField] private float pickupRadius = 2f;
    [SerializeField] private float dropDistance = 1.5f;

    private bool quitting;

    private void OnApplicationQuit() { quitting = true; }

    private void OnEnable()
    {
        int loaded = 0;
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

    private void OnDisable()
    {
        if (quitting) return;

        Debug.Log("[Ground] closed: dropping " + groundGrid.Items.Count);

        foreach (var item in new List<PlacedItem>(groundGrid.Items))
        {
            groundGrid.Remove(item);
            Vector3 center = player.position + player.forward * dropDistance;
            WorldPickup.DropNear(item.type, center, 0.7f);
        }
    }
}