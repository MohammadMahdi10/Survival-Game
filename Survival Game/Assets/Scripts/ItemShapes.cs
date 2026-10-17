using System.Collections.Generic;
using UnityEngine;

public static class ItemShapes
{
    // Cells are (column, row) offsets from the item's top-left. Row 0 is the top.
    private static Vector2Int V(int x, int y) => new Vector2Int(x, y);

    private static readonly Dictionary<ItemType, Vector2Int[]> shapes = new()
    {
        [ItemType.DesertStone] = new[] { V(0, 0) },
        [ItemType.PlainsStone] = new[] { V(0, 0), V(1, 0), V(0, 1), V(1, 1) },
        [ItemType.Wood] = new[] { V(0, 0), V(0, 1) },
        [ItemType.BasicSword] = new[] { V(0, 0), V(0, 1), V(0, 2) },
        [ItemType.FireSword] = new[] { V(0, 0), V(0, 1), V(0, 2), V(1, 1) },
        [ItemType.WaterBucket] = new[] { V(0, 0), V(2, 0), V(0, 1), V(1, 1), V(2, 1) },
    };

    private static readonly Dictionary<ItemType, Color> colors = new()
    {
        [ItemType.DesertStone] = new Color(0.85f, 0.7f, 0.4f),
        [ItemType.PlainsStone] = new Color(0.6f, 0.6f, 0.6f),
        [ItemType.Wood] = new Color(0.55f, 0.35f, 0.2f),
        [ItemType.BasicSword] = new Color(0.75f, 0.8f, 0.85f),
        [ItemType.FireSword] = new Color(0.9f, 0.4f, 0.2f),
        [ItemType.WaterBucket] = new Color(0.3f, 0.6f, 0.9f),
    };

    public static Vector2Int[] GetShape(ItemType item)
        => shapes.TryGetValue(item, out var s) ? s : new[] { V(0, 0) };

    public static Color GetColor(ItemType item)
        => colors.TryGetValue(item, out var c) ? c : Color.white;
}