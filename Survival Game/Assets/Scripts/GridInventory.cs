using System;
using System.Collections.Generic;
using UnityEngine;

public class PlacedItem
{
    public ItemType type;
    public Vector2Int origin;
    public Vector2Int[] shape;
}

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

    // Created on first use, so it works even while the panel is inactive
    private PlacedItem[,] Cells
    {
        get
        {
            if (cells == null) cells = new PlacedItem[width, height];
            return cells;
        }
    }

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

    // Finds the first free spot, scanning left to right, top to bottom
    public PlacedItem TryAddItem(ItemType type)
    {
        Vector2Int[] shape = ItemShapes.GetShape(type);
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                if (CanPlace(shape, new Vector2Int(x, y)))
                    return Place(type, shape, new Vector2Int(x, y));
        return null;
    }

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

    public int Count(ItemType type)
    {
        int n = 0;
        foreach (var item in items) if (item.type == type) n++;
        return n;
    }

    public PlacedItem GetItemAt(Vector2Int cell)
    {
        if (cell.x < 0 || cell.y < 0 || cell.x >= width || cell.y >= height) return null;
        return Cells[cell.x, cell.y];
    }

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

    [ContextMenu("Test Clear")]
    private void TestClear()
    {
        if (!Application.isPlaying) return;
        foreach (var item in new List<PlacedItem>(items)) Remove(item);
    }
}