using TMPro;
using UnityEngine;

public class RecipePageUI : MonoBehaviour
{
    [SerializeField] private ItemType recipeItem;
    [SerializeField] private CraftingManager craftingManager;
    [SerializeField] private GridInventory backpack;
    [SerializeField] private TMP_Text[] lines;

    private void OnEnable()
    {
        backpack.Changed += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        backpack.Changed -= Refresh;
    }

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

            // Green once you have enough
            lines[i].color = have >= ingredient.Value ? new Color(0.5f, 1f, 0.5f) : Color.white;
            i++;
        }
    }
}