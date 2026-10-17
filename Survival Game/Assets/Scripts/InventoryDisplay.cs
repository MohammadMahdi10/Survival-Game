using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InventoryDisplay : MonoBehaviour {
    [SerializeField] private Transform backpackGrid;
    [SerializeField] private ResourceManager resourceManager;

    private List<TextMeshProUGUI> slotLabels = new();

    private void Start() {
        foreach (Transform slot in backpackGrid) {
            GameObject labelObject = new GameObject("ItemLabel");

            labelObject.transform.SetParent(slot, false);

            TextMeshProUGUI label = labelObject.AddComponent<TextMeshProUGUI>();

            RectTransform rect = label.GetComponent<RectTransform>();

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = 36;
            label.color = Color.white;

            slotLabels.Add(label);
        }

        UpdateDisplay();
    }

    public void UpdateDisplay() {
        foreach (TextMeshProUGUI label in slotLabels) {
            label.text = "";
        }

        int slotIndex = 0;

        ItemType[] allItems =
            {
                ItemType.Wood,
                ItemType.PlainsStone,
                ItemType.DesertStone,
                ItemType.BasicSword,
                ItemType.FireSword,
                ItemType.WaterBucket
            };

        foreach (ItemType item in allItems) {
            int amount = resourceManager.GetResourceAmount(item);

            for (int i = 0; i < amount; i++) {
                if (slotIndex >= slotLabels.Count) {
                    return;
                }

                slotLabels[slotIndex].text = resourceManager.GetItemLabel(item);

                slotIndex++;
            }
        }
    }
}

