using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    private Dictionary<ItemType, int> resources = new();

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

    public int GetResourceAmount(ItemType item)
    {
        if (resources.ContainsKey(item))
        {
            return resources[item];
        }

        return 0;
    }
}

