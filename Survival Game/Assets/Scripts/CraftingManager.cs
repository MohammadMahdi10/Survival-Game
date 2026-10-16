using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingManager : MonoBehaviour
{
    private Dictionary<ItemType, Dictionary<ItemType, int>> recipes;
    [SerializeField] private ResourceManager resourceManager;

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

    public bool CanCraft(ItemType item)
    {

        Dictionary<ItemType, int> ingredients = GetRecipe(item);

        if (ingredients == null)
        {
            return false;
        }

        foreach (var ingredient in ingredients)
        {
            ItemType material = ingredient.Key;
            int requiredAmount = ingredient.Value;

            int playerAmount = resourceManager.GetResourceAmount(material);

            if (playerAmount < requiredAmount)
            {
                return false;
            }
        }

        Debug.Log("YES");
        return true;
    }

}



