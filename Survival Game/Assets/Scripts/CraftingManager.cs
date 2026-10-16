using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingManager : MonoBehaviour
{
    private Dictionary<ItemType, Dictionary<ItemType, int>> recipes;

    private void Start()
    {
        recipes = new Dictionary<ItemType, Dictionary<ItemType, int>>
        {
            [ItemType.BasicSword] = new()
            {
                [ItemType.Wood] = 2,
                [ItemType.PlainsStone] = 2
            },
            [ItemType.FireSword] = new()
            {
                [ItemType.Wood] = 2,
                [ItemType.DesertStone] = 4
            },
            [ItemType.WaterBucket] = new()
            {
                [ItemType.Wood] = 3,
                [ItemType.PlainsStone] = 1
            }
        };

        Debug.Log("LOADED!");
    }

    public Dictionary<ItemType, int> GetRecipe(ItemType item)
    {
        if (recipes.ContainsKey(item))
        {
            return recipes[item];
        }

        return null;
    }
}



