using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Stores and tracks the quantities of different resource types.
/// Uses a dictionary so resources can be managed without
/// creating separate variables for every item type.
public class ResourceManager : MonoBehaviour
{
    private Dictionary<ItemType, int> resources = new();

    /// Adds a quantity of a resource to the stored total.
    /// Ignores zero or negative amounts to prevent invalid additions.
    /// Creates a new entry if the resource has not been stored before.
    public void AddResource(ItemType item, int amount)
    {
        if (amount <= 0)
            return;

        if (resources.ContainsKey(item))
        {
            resources[item] += amount;
        }
        else
        {
            resources[item] = amount;
        }
    }

    /// Returns the stored quantity of a specific resource.
    /// Returns zero when the resource has no entry, allowing
    /// other scripts to check amounts without errors.
    public int GetResourceAmount(ItemType item)
    {
        if (resources.ContainsKey(item))
        {
            return resources[item];
        }

        return 0;
    }
}

