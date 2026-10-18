using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryGridUI : MonoBehaviour
{
    [SerializeField] private GridInventory inventory;
    [SerializeField] private float cellSize = 100f;
    [SerializeField] private float spacing = 10f;

    private void OnEnable()
    {
        inventory.Changed += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        inventory.Changed -= Refresh;
    }

    private void Refresh()
    {
        foreach (Transform child in transform)
        {
            if (child.name == "Ghost") continue;
            Destroy(child.gameObject);
        }

        foreach (var item in inventory.Items)
            DrawItem(item);
    }

    private void DrawItem(PlacedItem item)
    {
        var cells = new HashSet<Vector2Int>(item.shape);
        Color color = ItemShapes.GetColor(item.type);

        foreach (var offset in item.shape)
        {
            var go = new GameObject(item.type + " cell", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);

            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 1);

            // Stretch into the gap when the neighbour is part of the same item
            float w = cellSize + (cells.Contains(offset + new Vector2Int(1, 0)) ? spacing : 0f);
            float h = cellSize + (cells.Contains(offset + new Vector2Int(0, 1)) ? spacing : 0f);
            rt.sizeDelta = new Vector2(w, h);

            Vector2Int cell = item.origin + offset;
            rt.anchoredPosition = new Vector2(cell.x * (cellSize + spacing),
                                              -cell.y * (cellSize + spacing));

            go.GetComponent<Image>().color = color;
        }
    }
}