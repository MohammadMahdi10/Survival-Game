using System.Collections.Generic;
using UnityEngine;

public class CraftingManager : MonoBehaviour
{
    [SerializeField] private GridInventory backpack;
    [SerializeField] private Transform player;

    private Dictionary<ItemType, Dictionary<ItemType, int>> recipes;

    // Built on first use, so it works even if this object starts inactive
    private Dictionary<ItemType, Dictionary<ItemType, int>> Recipes
    {
        get
        {
            if (recipes == null)
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
            }
            return recipes;
        }
    }

    public Dictionary<ItemType, int> GetRecipe(ItemType item)
    {
        return Recipes.TryGetValue(item, out var recipe) ? recipe : null;
    }

    public bool CanCraft(ItemType item)
    {
        var ingredients = GetRecipe(item);
        if (ingredients == null) return false;

        foreach (var ingredient in ingredients)
            if (backpack.Count(ingredient.Key) < ingredient.Value)
                return false;

        return true;
    }

    public bool Craft(ItemType item)
    {
        if (!CanCraft(item))
        {
            Debug.Log("Not enough materials for " + item);
            return false;
        }

        // Use up the materials
        foreach (var ingredient in GetRecipe(item))
            for (int i = 0; i < ingredient.Value; i++)
                backpack.RemoveOne(ingredient.Key);

        // Give the result, or drop it at the player's feet if there's no room
        if (backpack.TryAddItem(item) != null)
        {
            Debug.Log("Crafted " + item);
        }
        else
        {
            Vector3 center = player.position + player.forward * 1.5f;
            WorldPickup.DropNear(item, center, 0.7f);
            Debug.Log("Crafted " + item + ", but the backpack is full so it dropped on the ground");
        }
        return true;
    }

    private ItemType selectedRecipe;
    private bool hasSelection;

    public void SelectRecipe(ItemType item)
    {
        selectedRecipe = item;
        hasSelection = true;
    }

    public void CraftSelected()
    {
        if (!hasSelection)
        {
            Debug.Log("No recipe selected");
            return;
        }
        Craft(selectedRecipe);
    }

}