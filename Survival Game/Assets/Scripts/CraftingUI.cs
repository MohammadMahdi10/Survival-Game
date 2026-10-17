
using UnityEngine;

public class CraftingUI : MonoBehaviour
{
    [SerializeField] private GameObject craftingScreen;
    [SerializeField] private GameObject materialsScreen;

    [SerializeField] private GameObject basicSwordPage;
    [SerializeField] private GameObject fireSwordPage;
    [SerializeField] private GameObject bucketPage;

    [SerializeField] private CraftingManager craftingManager;

    private ItemType selectedRecipe;

    public void ShowBasicSword()
    {
        selectedRecipe = ItemType.BasicSword;
        ShowRecipe(basicSwordPage);
    }

    public void ShowFireSword()
    {
        selectedRecipe = ItemType.FireSword;
        ShowRecipe(fireSwordPage);
    }

    public void ShowBucket()
    {
        selectedRecipe = ItemType.WaterBucket;
        ShowRecipe(bucketPage);
    }

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

    public void ShowCraftingScreen()
    {
        materialsScreen.SetActive(false);
        craftingScreen.SetActive(true);
    }
    public void CraftSelectedItem()
    {
        bool success = craftingManager.CraftItem(selectedRecipe);

        if (success)
        {
            Debug.Log("YES");
        }
        else
        {
            Debug.Log("NO");
        }
    }

}
