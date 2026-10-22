using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// Displays items inside an inventory grid.
/// Draws each item's shape, colour and label,
/// and updates the display whenever the inventory changes.
public class InventoryGridUI : MonoBehaviour
{
    [SerializeField] private GridInventory inventory;
    [SerializeField] private float cellSize = 100f;
    [SerializeField] private float spacing = 10f;

    /// Refreshes the inventory display when the UI becomes active.
    /// Listens for inventory changes to keep the display updated.
    private void OnEnable()
    {
        inventory.Changed += Refresh;
        Refresh();
    }

    /// Stops listening for inventory changes when the UI is disabled.
    /// This prevents unnecessary updates and duplicate event subscriptions.
    private void OnDisable()
    {
        inventory.Changed -= Refresh;
    }

    /// Redraws inventory items when the inventory changes.
    /// Preserves the Ghost object so the player's drag preview is not removed.
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

    /// Draws an inventory item's cells using its shape and colour.
    /// Positions the cells in the grid and adds the item's name label.
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


            // Extends adjacent cells into the gaps so multi-cell items
            // appear as one connected shape rather than separate squares.
            float w = cellSize + (cells.Contains(offset + new Vector2Int(1, 0)) ? spacing : 0f);
            float h = cellSize + (cells.Contains(offset + new Vector2Int(0, 1)) ? spacing : 0f);
            rt.sizeDelta = new Vector2(w, h);

            Vector2Int cell = item.origin + offset;
            rt.anchoredPosition = new Vector2(cell.x * (cellSize + spacing),
                                              -cell.y * (cellSize + spacing));

            go.GetComponent<Image>().color = color;
        }

        ItemLabels.Add(transform, item.type, item.shape, cellSize, spacing, item.origin);
    }
}