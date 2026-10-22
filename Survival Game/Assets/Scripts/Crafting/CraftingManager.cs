using System.Collections.Generic;
using UnityEngine;

/// Manages crafting recipes and allows the player to craft items
/// using materials stored in their backpack.
public class CraftingManager : MonoBehaviour
{
    [SerializeField] private GridInventory backpack;
    [SerializeField] private Transform player;

    private Dictionary<ItemType, Dictionary<ItemType, int>> recipes;


    /// Creates the crafting recipes only when they are first needed.
    /// Keeping recipes in one dictionary allows new craftable items
    /// to be added without changing the crafting methods.
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

    /// Gets the crafting recipe for an item.
    /// Returns null if the item has no recipe.
    public Dictionary<ItemType, int> GetRecipe(ItemType item)
    {
        return Recipes.TryGetValue(item, out var recipe) ? recipe : null;
    }


    /// Checks whether the backpack contains every material required
    /// by the recipe. This prevents crafting when resources are missing,
    /// ensuring materials cannot be used beyond what the player owns.
    public bool CanCraft(ItemType item)
    {
        var ingredients = GetRecipe(item);
        if (ingredients == null) return false;

        foreach (var ingredient in ingredients)
            if (backpack.Count(ingredient.Key) < ingredient.Value)
                return false;

        return true;
    }


    /// Crafts an item only when all required materials are available.
    /// Consumes the ingredients and attempts to store the finished item.
    /// If the backpack is full, the item is dropped near the player
    /// so the crafted item is not lost.
    public bool Craft(ItemType item)
    {
        if (!CanCraft(item))
        {
            Debug.Log("Not enough materials for " + item);
            return false;
        }

        foreach (var ingredient in GetRecipe(item))
            for (int i = 0; i < ingredient.Value; i++)
                backpack.RemoveOne(ingredient.Key);

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

    /// Saves the recipe selected by the player in the crafting menu.
    public void SelectRecipe(ItemType item)
    {
        selectedRecipe = item;
        hasSelection = true;
    }

    /// Attempts to craft the currently selected recipe.
    /// Displays a message if no recipe has been selected.
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