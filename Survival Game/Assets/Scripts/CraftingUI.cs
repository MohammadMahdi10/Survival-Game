using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CraftingUI : MonoBehaviour {
    [SerializeField] private GameObject craftingScreen;
    [SerializeField] private GameObject materialsScreen;

    [SerializeField] private GameObject basicSwordPage;
    [SerializeField] private GameObject fireSwordPage;
    [SerializeField] private GameObject bucketPage;
    [SerializeField] private CraftingManager craftingManager;

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

    private void ShowRecipe(GameObject selectedPage) {
        Debug.Log("Recipe selected: " + selectedPage.name);

        craftingScreen.SetActive(false);

        basicSwordPage.SetActive(false);
        fireSwordPage.SetActive(false);
        bucketPage.SetActive(false);

        selectedPage.SetActive(true);
        materialsScreen.SetActive(true);
    }

    public void ShowCraftingScreen() {
        materialsScreen.SetActive(false);
        craftingScreen.SetActive(true);
    }
    public void CraftSelectedItem() {
        bool success = craftingManager.CraftItem(selectedRecipe);

        if (success)
        {
            Debug.Log("YES");
        }
        else
        {
            Debug.Log("NO");
        }

        UpdateMaterialCounters();
    }

    public void UpdateMaterialCounters() {
        Dictionary<ItemType, int> ingredients = craftingManager.GetRecipe(selectedRecipe);

        if (ingredients == null) {
            return;
        }

        TextMeshProUGUI[] materialTexts = {materialText1, materialText2};

        int index = 0;

        foreach (var ingredient in ingredients) {
            ItemType material = ingredient.Key;
            int requiredAmount = ingredient.Value;

            int ownedAmount = resourceManager.GetResourceAmount(material);

            materialTexts[index].text = "x" + requiredAmount + " " + material + " [" + ownedAmount + "]";
            index++;
        }
    }

}
