using System;
using System.Collections.Generic;
using UnityEngine;

/// Stores information about an item placed inside an inventory grid,
/// including its type, position and shape.
public class PlacedItem
{
    public ItemType type;
    public Vector2Int origin;
    public Vector2Int[] shape;
}

/// Manages a grid based inventory where items occupy different cell shapes.
/// Handles placing, removing, counting and locating items within the grid.
public class GridInventory : MonoBehaviour
{
    [SerializeField] private int width = 5;
    [SerializeField] private int height = 5;

    public event Action Changed;

    private PlacedItem[,] cells;
    private readonly List<PlacedItem> items = new();

    public int Width => width;
    public int Height => height;
    public IReadOnlyList<PlacedItem> Items => items;

    /// Creates the inventory cell array when it is first needed.
    /// This allows the inventory to work even when its UI panel is inactive.
    private PlacedItem[,] Cells
    {
        get
        {
            if (cells == null) cells = new PlacedItem[width, height];
            return cells;
        }
    }

    /// Checks whether every cell of an item fits within the inventory
    /// without overlapping other items.
    /// This prevents items from being placed outside the grid
    /// or occupying cells that are already in use.
    public bool CanPlace(Vector2Int[] shape, Vector2Int origin)
    {
        foreach (var offset in shape)
        {
            Vector2Int c = origin + offset;
            if (c.x < 0 || c.y < 0 || c.x >= width || c.y >= height) return false;
            if (Cells[c.x, c.y] != null) return false;
        }
        return true;
    }


    /// Places an item into the inventory if its cells are available.
    /// Records the item in the grid and item list, then triggers the
    /// Changed event so the inventory UI can update automatically.
    /// Returns null if the item cannot be placed.
    public PlacedItem Place(ItemType type, Vector2Int[] shape, Vector2Int origin)
    {
        if (!CanPlace(shape, origin)) return null;

        var item = new PlacedItem { type = type, origin = origin, shape = shape };
        foreach (var offset in shape)
        {
            Vector2Int c = origin + offset;
            Cells[c.x, c.y] = item;
        }
        items.Add(item);
        Changed?.Invoke();
        return item;
    }

    /// Finds the first available space for an item by searching the grid
    /// from left to right and top to bottom.
    /// Returns null if the item does not fit anywhere.
    public PlacedItem TryAddItem(ItemType type)
    {
        Vector2Int[] shape = ItemShapes.GetShape(type);
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                if (CanPlace(shape, new Vector2Int(x, y)))
                    return Place(type, shape, new Vector2Int(x, y));
        return null;
    }

    /// Removes an item from the inventory.
    /// Frees its occupied cells and notifies other scripts of the change.
    public void Remove(PlacedItem item)
    {
        foreach (var offset in item.shape)
        {
            Vector2Int c = item.origin + offset;
            Cells[c.x, c.y] = null;
        }
        items.Remove(item);
        Changed?.Invoke();
    }

    /// Finds and removes one item of the specified type.
    /// Returns true if an item was removed, or false if none was found.
    public bool RemoveOne(ItemType type)
    {
        foreach (var item in items)
        {
            if (item.type == type)
            {
                Remove(item);
                return true;
            }
        }
        return false;
    }

    /// Counts how many items of a specific type are stored in the inventory.
    public int Count(ItemType type)
    {
        int n = 0;
        foreach (var item in items) if (item.type == type) n++;
        return n;
    }

    /// Returns the item occupying a particular inventory cell.
    /// Returns null if the cell is empty or outside the grid.
    public PlacedItem GetItemAt(Vector2Int cell)
    {
        if (cell.x < 0 || cell.y < 0 || cell.x >= width || cell.y >= height) return null;
        return Cells[cell.x, cell.y];
    }

    /// Adds a collection of test items to the inventory during Play Mode.
    /// Logs whether each item was successfully placed.
    [ContextMenu("Test Fill")]
    private void TestFill()
    {
        if (!Application.isPlaying) { Debug.Log("Press Play first."); return; }

        ItemType[] test = {
            ItemType.WaterBucket, ItemType.PlainsStone, ItemType.Wood, ItemType.Wood,
            ItemType.BasicSword, ItemType.DesertStone, ItemType.FireSword
        };

        foreach (var t in test)
            Debug.Log(t + (TryAddItem(t) != null ? " placed" : " DID NOT FIT"));
    }

    /// Removes all items from the inventory during Play Mode.
    /// Used for quickly testing an empty inventory.
    [ContextMenu("Test Clear")]
    private void TestClear()
    {
        if (!Application.isPlaying) return;


        // Iterate over a copy because Remove() modifies the original list.
        // This prevents errors caused by changing a collection during iteration.
        foreach (var item in new List<PlacedItem>(items)) Remove(item);
    }
}