using System.Collections.Generic;
using UnityEngine;

/// Stores item shapes, colours and display names in one central location.
/// This allows inventory, crafting and equipment systems to reuse the
/// same item information without duplicating it across multiple scripts.
public static class ItemShapes
{
    /// Creates a grid position using column and row coordinates.
    /// Row zero represents the top of an item.
    private static Vector2Int V(int x, int y) => new Vector2Int(x, y);

    // Defines which grid cells each item occupies.
    // Coordinates are offsets from the item's top-left corner.
    private static readonly Dictionary<ItemType, Vector2Int[]> shapes = new()
    {
        [ItemType.DesertStone] = new[] { V(0, 0) },
        [ItemType.PlainsStone] = new[] { V(0, 0), V(1, 0), V(0, 1), V(1, 1) },
        [ItemType.Wood] = new[] { V(0, 0), V(0, 1) },
        [ItemType.BasicSword] = new[] { V(0, 0), V(0, 1), V(0, 2) },
        [ItemType.FireSword] = new[] { V(0, 0), V(0, 1), V(0, 2), V(1, 1) },
        [ItemType.WaterBucket] = new[] { V(0, 0), V(2, 0), V(0, 1), V(1, 1), V(2, 1) },
        [ItemType.SpecialItem] = new[] { V(1, 0), V(0, 1), V(1, 1), V(2, 1), V(1, 2) },
        [ItemType.RepairComponent] = new[] { V(1, 0), V(0, 1), V(1, 1), V(2, 1) },
        [ItemType.Mutton] = new[] { V(0, 0), V(0, 1), V(1, 1) },
    };

    // Stores the colour used to display each item in the inventory.
    private static readonly Dictionary<ItemType, Color> colors = new()
    {
        [ItemType.DesertStone] = new Color(0.85f, 0.7f, 0.4f),
        [ItemType.PlainsStone] = new Color(0.6f, 0.6f, 0.6f),
        [ItemType.Wood] = new Color(0.55f, 0.35f, 0.2f),
        [ItemType.BasicSword] = new Color(0.75f, 0.8f, 0.85f),
        [ItemType.FireSword] = new Color(0.9f, 0.4f, 0.2f),
        [ItemType.WaterBucket] = new Color(0.3f, 0.6f, 0.9f),
        [ItemType.SpecialItem] = new Color(0.7f, 0.3f, 0.9f),
        [ItemType.RepairComponent] = new Color(0.2f, 0.85f, 0.9f),
        [ItemType.Mutton] = new Color(0.7f, 0.35f, 0.25f),
    };

    // Stores the names shown to the player for each item.
    private static readonly Dictionary<ItemType, string> names = new()
    {
        [ItemType.DesertStone] = "Desert Stone",
        [ItemType.PlainsStone] = "Plains Stone",
        [ItemType.Wood] = "Wood",
        [ItemType.BasicSword] = "Basic Sword",
        [ItemType.FireSword] = "Fire Sword",
        [ItemType.WaterBucket] = "Water Bucket",
        [ItemType.SpecialItem] = "Special Item",
        [ItemType.RepairComponent] = "Repair Component",
        [ItemType.Mutton] = "Raw Mutton",
    };

    /// Checks whether an item can be equipped by the player.
    public static bool IsEquippable(ItemType item)
    => item == ItemType.BasicSword ||
       item == ItemType.FireSword ||
       item == ItemType.WaterBucket;

    /// Returns an item's display name.
    /// Uses the item type's name if no custom display name exists.
    public static string GetName(ItemType item)
        => names.TryGetValue(item, out var n) ? n : item.ToString();

    /// Returns the grid cells occupied by an item's shape.
    /// Uses a single cell shape if the item has no defined shape.
    public static Vector2Int[] GetShape(ItemType item)
        => shapes.TryGetValue(item, out var s) ? s : new[] { V(0, 0) };



    /// Rotates an item's shape by 90 degrees clockwise.
    /// Shifts the rotated cells back to the top-left of the grid
    /// so the item remains correctly aligned after rotation.
    /// Also updates the grab position so the item stays attached
    /// to the correct mouse position while being dragged.
    public static Vector2Int[] Rotate(Vector2Int[] shape, ref Vector2Int point)
    {
        var rotated = new Vector2Int[shape.Length];
        int minX = int.MaxValue, minY = int.MaxValue;

        for (int i = 0; i < shape.Length; i++)
        {
            rotated[i] = new Vector2Int(-shape[i].y, shape[i].x);
            minX = Mathf.Min(minX, rotated[i].x);
            minY = Mathf.Min(minY, rotated[i].y);
        }


        // Rotation can produce negative cell coordinates.
        // Shift all cells so the shape starts at zero again.
        var shift = new Vector2Int(minX, minY);
        for (int i = 0; i < rotated.Length; i++) rotated[i] -= shift;

        point = new Vector2Int(-point.y, point.x) - shift;
        return rotated;
    }

    /// Returns the colour used to display an item.
    /// Uses white if the item has no defined colour.
    public static Color GetColor(ItemType item)
        => colors.TryGetValue(item, out var c) ? c : Color.white;
}