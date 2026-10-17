using System.Collections;
using System.Collections.Generic;
using System.Resources;
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

    public bool RemoveResource(ItemType item, int amount)
    {
        if (amount <= 0)
            return false;

        if (GetResourceAmount(item) < amount)
            return false;

        resources[item] -= amount;
        return true;
    }

    public int GetResourceAmount(ItemType item)
    {
        if (resources.ContainsKey(item))
        {
            return resources[item];
        }

        return 0;
    }


    public string GetItemLabel(ItemType item)
    {
        switch (item)
        {
            case ItemType.Wood:
                return "W";

            case ItemType.PlainsStone:
                return "SP";

            case ItemType.DesertStone:
                return "SD";

            case ItemType.BasicSword:
                return "BS";

            case ItemType.FireSword:
                return "FS";

            case ItemType.WaterBucket:
                return "WB";

            default:
                return "?";
        }
    }


    [ContextMenu("Check Wood Amount")]
    public void CheckWoodAmount()
    {
        Debug.Log("Player owns " + GetResourceAmount(ItemType.Wood) + " Wood");
    }

    [ContextMenu("Test Item Labels")]
    public void TestItemLabels()
    {
        Debug.Log(GetItemLabel(ItemType.Wood));
        Debug.Log(GetItemLabel(ItemType.PlainsStone));
        Debug.Log(GetItemLabel(ItemType.FireSword));
    }
}

