using UnityEngine;

/// Controls the crafting menu interface.
/// Allows the player to select recipes and view their required materials.
public class CraftingUI : MonoBehaviour
{
    [SerializeField] private GameObject craftingScreen;
    [SerializeField] private GameObject materialsScreen;

    [SerializeField] private GameObject basicSwordPage;
    [SerializeField] private GameObject fireSwordPage;
    [SerializeField] private GameObject bucketPage;
    [SerializeField] private CraftingManager craftingManager;

    /// Selects the Basic Sword recipe and displays its crafting page.
    public void ShowBasicSword()
    {
        craftingManager.SelectRecipe(ItemType.BasicSword);
        ShowRecipe(basicSwordPage);
    }

    /// Selects the Fire Sword recipe and displays its crafting page.
    public void ShowFireSword()
    {
        craftingManager.SelectRecipe(ItemType.FireSword);
        ShowRecipe(fireSwordPage);
    }

    /// Selects the Water Bucket recipe and displays its crafting page.
    public void ShowBucket()
    {
        craftingManager.SelectRecipe(ItemType.WaterBucket);
        ShowRecipe(bucketPage);
    }

    /// Hides the recipe selection screen and other recipe pages.
    /// Displays the selected recipe page and its required materials.
    private void ShowRecipe(GameObject selectedPage)
    {
        Debug.Log("Recipe selected: " + selectedPage.name);

        craftingScreen.SetActive(false);

        basicSwordPage.SetActive(false);
        fireSwordPage.SetActive(false);
        bucketPage.SetActive(false);

        selectedPage.SetActive(true);
        materialsScreen.SetActive(true);
    }

    /// Returns the player to the main crafting screen
    /// and hides the materials screen.
    public void ShowCraftingScreen()
    {
        materialsScreen.SetActive(false);
        craftingScreen.SetActive(true);
    }
}
