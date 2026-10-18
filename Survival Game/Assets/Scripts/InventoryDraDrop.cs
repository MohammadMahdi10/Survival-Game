using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryDragDrop : MonoBehaviour
{
    [Serializable]
    public class GridView
    {
        public GridInventory inventory;
        public RectTransform layer;   // the item layer that sits on top of this grid
    }

    [SerializeField] private GridView[] grids;
    [SerializeField] private float cellSize = 100f;
    [SerializeField] private float spacing = 10f;

    private RectTransform panel;
    private RectTransform ghostRoot;
    private readonly List<Image> ghostCells = new();

    private bool holding;
    private ItemType heldType;
    private Vector2Int[] heldShape;
    private Vector2Int grabOffset;
    private GridView sourceGrid;
    private Vector2Int sourceOrigin;
    private Vector2Int[] sourceShape;

    private float Step => cellSize + spacing;

    private void Awake()
    {
        panel = (RectTransform)transform;
    }

    private void OnDisable()
    {
        // Closing the inventory mid-drag puts the item back where it came from
        if (holding)
        {
            sourceGrid.inventory.Place(heldType, sourceShape, sourceOrigin);
            EndHold();
        }
    }

    private bool TryGetCell(GridView g, Vector2 screen, out Vector2Int cell)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(g.layer, screen, null, out Vector2 p);
        Rect r = g.layer.rect;
        cell = new Vector2Int(Mathf.FloorToInt((p.x - r.xMin) / Step),
                              Mathf.FloorToInt((r.yMax - p.y) / Step));
        return cell.x >= 0 && cell.y >= 0 &&
               cell.x < g.inventory.Width && cell.y < g.inventory.Height;
    }

    private GridView GridUnderMouse(Vector2 screen, out Vector2Int cell)
    {
        foreach (var g in grids)
            if (g.layer.gameObject.activeInHierarchy && TryGetCell(g, screen, out cell))
                return g;
        cell = default;
        return null;
    }

    // Top-left corner of a grid, in this panel's space (y points down)
    private Vector2 GridCorner(GridView g)
    {
        Rect r = g.layer.rect;
        Vector3 world = g.layer.TransformPoint(new Vector3(r.xMin, r.yMax, 0f));
        Vector3 p = panel.InverseTransformPoint(world);
        Rect pr = panel.rect;
        return new Vector2(p.x - pr.xMin, pr.yMax - p.y);
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        Vector2 screen = Mouse.current.position.ReadValue();
        GridView hover = GridUnderMouse(screen, out Vector2Int cell);

        if (!holding)
        {
            if (hover != null && Mouse.current.leftButton.wasPressedThisFrame)
                TryPickUp(hover, cell);
            return;
        }

        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            heldShape = ItemShapes.Rotate(heldShape, ref grabOffset);
            BuildGhost();
        }

        Vector2Int origin = cell - grabOffset;
        bool valid = hover != null && hover.inventory.CanPlace(heldShape, origin);

        // Snap to the grid when over one, otherwise follow the cursor
        Vector2 pos;
        if (hover != null)
        {
            pos = GridCorner(hover) + new Vector2(origin.x * Step, origin.y * Step);
        }
        else
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(panel, screen, null, out Vector2 p);
            Rect pr = panel.rect;
            pos = new Vector2(p.x - pr.xMin, pr.yMax - p.y)
                  - new Vector2(grabOffset.x * Step + cellSize / 2f, grabOffset.y * Step + cellSize / 2f);
        }

        ghostRoot.anchoredPosition = new Vector2(pos.x, -pos.y);
        ghostRoot.SetAsLastSibling();

        Color c = valid ? ItemShapes.GetColor(heldType) : new Color(1f, 0.3f, 0.3f);
        c.a = 0.8f;
        foreach (var img in ghostCells) img.color = c;

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            if (valid) hover.inventory.Place(heldType, heldShape, origin);
            else sourceGrid.inventory.Place(heldType, sourceShape, sourceOrigin);
            EndHold();
        }
    }

    private void TryPickUp(GridView g, Vector2Int cell)
    {
        PlacedItem item = g.inventory.GetItemAt(cell);
        if (item == null) return;

        holding = true;
        heldType = item.type;
        heldShape = item.shape;
        sourceGrid = g;
        sourceShape = item.shape;
        sourceOrigin = item.origin;
        grabOffset = cell - item.origin;

        g.inventory.Remove(item);
        BuildGhost();
    }

    private void EndHold()
    {
        holding = false;
        if (ghostRoot != null) ghostRoot.gameObject.SetActive(false);
    }

    private void BuildGhost()
    {
        if (ghostRoot == null)
        {
            var go = new GameObject("Ghost", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            ghostRoot = (RectTransform)go.transform;
            ghostRoot.anchorMin = ghostRoot.anchorMax = ghostRoot.pivot = new Vector2(0, 1);
            ghostRoot.sizeDelta = Vector2.zero;
        }

        ghostRoot.gameObject.SetActive(true);
        foreach (Transform child in ghostRoot) Destroy(child.gameObject);
        ghostCells.Clear();

        var cells = new HashSet<Vector2Int>(heldShape);
        foreach (var offset in heldShape)
        {
            var go = new GameObject("cell", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(ghostRoot, false);

            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 1);

            float w = cellSize + (cells.Contains(offset + new Vector2Int(1, 0)) ? spacing : 0f);
            float h = cellSize + (cells.Contains(offset + new Vector2Int(0, 1)) ? spacing : 0f);
            rt.sizeDelta = new Vector2(w, h);
            rt.anchoredPosition = new Vector2(offset.x * Step, -offset.y * Step);

            var img = go.GetComponent<Image>();
            img.raycastTarget = false;
            ghostCells.Add(img);
        }
    }
}