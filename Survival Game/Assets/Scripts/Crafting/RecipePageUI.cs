using TMPro;
using UnityEngine;

/// Displays the materials required for a crafting recipe.
/// Shows how many materials the player has and highlights
/// ingredients when enough are available.
public class RecipePageUI : MonoBehaviour
{
    [SerializeField] private ItemType recipeItem;
    [SerializeField] private CraftingManager craftingManager;
    [SerializeField] private GridInventory backpack;
    [SerializeField] private TMP_Text[] lines;

    /// Updates the recipe display when the page opens.
    /// Listens for inventory changes so material counts stay updated.
    private void OnEnable()
    {
        backpack.Changed += Refresh;
        Refresh();
    }


    /// Stops listening for inventory changes when the recipe page closes.
    /// This prevents unnecessary updates and avoids leaving event subscriptions active.

    private void OnDisable()
    {
        backpack.Changed -= Refresh;
    }


    /// Updates the required materials and current quantities for the recipe.
    /// Highlights ingredients in green when enough are available,
    /// helping the player quickly identify which materials are still needed.

    private void Refresh()
    {
        var recipe = craftingManager.GetRecipe(recipeItem);
        if (recipe == null) return;

        int i = 0;
        foreach (var ingredient in recipe)
        {
            if (i >= lines.Length) break;

            int have = backpack.Count(ingredient.Key);
            lines[i].text = "x" + ingredient.Value + " " +
                            ItemShapes.GetName(ingredient.Key) + " [" + have + "]";

            lines[i].color = have >= ingredient.Value ? new Color(0.5f, 1f, 0.5f) : Color.white;
            i++;
        }
    }
}