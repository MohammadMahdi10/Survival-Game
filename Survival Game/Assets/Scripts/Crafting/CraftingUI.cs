using UnityEngine;


/// Controls the crafting menu interface.
/// Keeps recipe selection and screen navigation separate from
/// the actual crafting logic handled by CraftingManager.

public class CraftingUI : MonoBehaviour
{
    [SerializeField] private GameObject craftingScreen;
    [SerializeField] private GameObject materialsScreen;

    [SerializeField] private GameObject basicSwordPage;
    [SerializeField] private GameObject fireSwordPage;
    [SerializeField] private GameObject bucketPage;
    [SerializeField] private CraftingManager craftingManager;

    /// Functions below select craftable item recipe and displays its crafting page.
    public void ShowBasicSword()
    {
        craftingManager.SelectRecipe(ItemType.BasicSword);
        ShowRecipe(basicSwordPage);
    }

    public void ShowFireSword()
    {
        craftingManager.SelectRecipe(ItemType.FireSword);
        ShowRecipe(fireSwordPage);
    }

    public void ShowBucket()
    {
        craftingManager.SelectRecipe(ItemType.WaterBucket);
        ShowRecipe(bucketPage);
    }


    /// Displays the selected recipe page and hides the others.
    /// Using one shared method avoids repeating the same screen
    /// switching logic for every craftable item.

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
