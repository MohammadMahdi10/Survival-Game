using TMPro;
using UnityEngine;

public static class ItemLabels
{
    // Adds a text label on the cell nearest the centre of the item
    public static void Add(Transform parent, ItemType type, Vector2Int[] shape,
                           float cellSize, float spacing, Vector2Int origin)
    {
        Vector2 centre = Vector2.zero;
        foreach (var c in shape) centre += c;
        centre /= shape.Length;

        Vector2Int best = shape[0];
        float bestDist = float.MaxValue;
        foreach (var c in shape)
        {
            float d = ((Vector2)c - centre).sqrMagnitude;
            if (d < bestDist) { bestDist = d; best = c; }
        }

        var go = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);

        float step = cellSize + spacing;
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 1);
        rt.sizeDelta = new Vector2(cellSize, cellSize);
        rt.anchoredPosition = new Vector2((origin.x + best.x) * step, -(origin.y + best.y) * step);

        Color bg = ItemShapes.GetColor(type);
        float luminance = 0.299f * bg.r + 0.587f * bg.g + 0.114f * bg.b;

        var text = go.GetComponent<TextMeshProUGUI>();
        text.text = ItemShapes.GetName(type);
        text.alignment = TextAlignmentOptions.Center;
        text.enableAutoSizing = true;
        text.fontSizeMin = 12;
        text.fontSizeMax = 24;
        text.enableWordWrapping = true;
        text.margin = new Vector4(4, 4, 4, 4);
        text.color = luminance > 0.55f ? Color.black : Color.white;
        text.raycastTarget = false;
    }
}