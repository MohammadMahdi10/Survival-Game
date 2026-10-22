using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Represents a resource that the player can gather.
/// Stores the type and amount of the resource available.
public class GatherableResource : MonoBehaviour
{
    public ItemType resourceType;
    public int amount = 1;

    /// Logs the amount and type of resource gathered.
    public void Gather()
    {
        Debug.Log("Gathered " + amount + " " + resourceType);
    }
}
