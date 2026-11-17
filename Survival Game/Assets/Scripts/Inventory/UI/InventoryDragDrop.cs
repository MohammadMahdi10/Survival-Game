using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

/// Handles dragging, dropping, rotating and equipping inventory items,
/// and guides the player with a hover highlight and context hints,
/// since playtesters didn't realise items could be dragged.
public class InventoryDragDrop : MonoBehaviour
{
    /// Stores an inventory and the UI layer used to display its items.
    [Serializable]
    public class GridView
    {
        public GridInventory inventory;
        public RectTransform layer;
    }

    [SerializeField] private GridView[] grids;
    [SerializeField] private float cellSize = 100f;
    [SerializeField] private float spacing = 10f;
    [SerializeField] private EquipmentManager equipment;
    [SerializeField] private RectTransform equippedSlot;

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

    [Header("Guidance")]
    [SerializeField] private TMP_Text hintText;
    [SerializeField] private Color hoverColour = new Color(1f, 1f, 1f, 0.25f);

    private RectTransform hoverRoot;
    private readonly List<Image> hoverCells = new();
    private PlacedItem hoveredItem;

    /// Calculates the distance between neighbouring inventory cells,
    /// including the spacing between them.
    private float Step => cellSize + spacing;


    /// Gets the RectTransform of the inventory panel when the script starts.
    private void Awake()
    {
        panel = (RectTransform)transform;
    }

    /// Returns a held item to its original inventory position
    /// if the inventory closes during dragging.
    /// This prevents items from disappearing when a drag is interrupted.
    private void OnDisable()
    {
        UpdateHover(null, null);
        if (holding)
        {
            sourceGrid.inventory.Place(heldType, sourceShape, sourceOrigin);
            EndHold();
        }
    }


    /// Converts the mouse's screen position into a cell position
    /// within an inventory grid.
    /// Returns true if the position is inside the grid.
    private bool TryGetCell(GridView g, Vector2 screen, out Vector2Int cell)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(g.layer, screen, null, out Vector2 p);
        Rect r = g.layer.rect;
        cell = new Vector2Int(Mathf.FloorToInt((p.x - r.xMin) / Step),
                              Mathf.FloorToInt((r.yMax - p.y) / Step));
        return cell.x >= 0 && cell.y >= 0 &&
               cell.x < g.inventory.Width && cell.y < g.inventory.Height;
    }


    /// Finds which inventory grid the mouse is currently hovering over.
    /// Returns null if the mouse is not over an active grid.
    private GridView GridUnderMouse(Vector2 screen, out Vector2Int cell)
    {
        foreach (var g in grids)
            if (g.layer.gameObject.activeInHierarchy && TryGetCell(g, screen, out cell))
                return g;
        cell = default;
        return null;
    }

    /// Converts the grid's top-left corner into the inventory panel's
    /// coordinate space. This allows items to align correctly even
    /// when the inventory grids have different parent UI objects.
    private Vector2 GridCorner(GridView g)
    {
        Rect r = g.layer.rect;
        Vector3 world = g.layer.TransformPoint(new Vector3(r.xMin, r.yMax, 0f));
        Vector3 p = panel.InverseTransformPoint(world);
        Rect pr = panel.rect;
        return new Vector2(p.x - pr.xMin, pr.yMax - p.y);
    }


    /// Handles inventory mouse and keyboard input every frame.
    /// Allows the player to pick up, move, rotate, place, equip
    /// and unequip items, and updates quests when special items are collected.
    private void Update()
    {
        if (Mouse.current == null) return;

        Vector2 screen = Mouse.current.position.ReadValue();
        GridView hover = GridUnderMouse(screen, out Vector2Int cell);

        if (!holding)
        {
            PlacedItem underMouse = hover != null ? hover.inventory.GetItemAt(cell) : null;
            UpdateHover(hover, underMouse);
            SetHint(IdleHint(underMouse, screen));
            bool equipPressed = Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame;

            if (equipPressed)
            {
                if (hover != null)
                {
                    PlacedItem item = hover.inventory.GetItemAt(cell);
                    if (item != null && !equipment.Equip(hover.inventory, item))
                        Debug.Log(ItemShapes.GetName(item.type) + " can't be equipped");
                }
                else if (equippedSlot != null &&
                         RectTransformUtility.RectangleContainsScreenPoint(equippedSlot, screen))
                {
                    equipment.Unequip();
                }
            }

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
        SetHint(valid ? "Release to place  •  [R] Rotate"
            : hover == null ? "Release to put it back  •  [R] Rotate"
            : "Doesn't fit here  •  [R] Rotate");

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

        if (Mouse.current.leftButton.wasReleasedThisFrame) {
            if (valid) {
                hover.inventory.Place(heldType, heldShape, origin);
            }
            else {
                sourceGrid.inventory.Place(heldType, sourceShape, sourceOrigin);
            }

            EndHold();
        }
    }


    /// Picks up an item from an inventory grid when clicked.
    /// Saves its original position and shape, removes it from the grid,
    /// and creates a visual preview while dragging.
    private void TryPickUp(GridView g, Vector2Int cell)
    {
        PlacedItem item = g.inventory.GetItemAt(cell);
        UpdateHover(null, null);   // the dragged preview replaces the highlight
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

    /// Finishes dragging an item and hides its visual preview.
    private void EndHold()
    {
        holding = false;
        if (ghostRoot != null) ghostRoot.gameObject.SetActive(false);
    }


    /// Creates or refreshes the drag preview of the held item,
    /// showing its shape, colour and label while it is being dragged.
    private void BuildGhost()
    {
        ShowOverlay(ref ghostRoot, "Ghost", 50);
        BuildShapeCells(ghostRoot, heldShape, ghostCells);
        ItemLabels.Add(ghostRoot, heldType, heldShape, cellSize, spacing, Vector2Int.zero);
    }

    /// Highlights the item under the mouse so players can see items are
    /// interactive. Only rebuilds when the hovered item changes, rather than
    /// every frame.
    private void UpdateHover(GridView grid, PlacedItem item)
    {
        if (item == hoveredItem) return;
        hoveredItem = item;

        if (item == null)
        {
            if (hoverRoot != null) hoverRoot.gameObject.SetActive(false);
            return;
        }

        // Sits above the panels (10) but below the drag preview (50).
        ShowOverlay(ref hoverRoot, "Hover", 40);
        BuildShapeCells(hoverRoot, item.shape, hoverCells);
        foreach (var img in hoverCells) img.color = hoverColour;

        Vector2 pos = GridCorner(grid) + new Vector2(item.origin.x * Step, item.origin.y * Step);
        hoverRoot.anchoredPosition = new Vector2(pos.x, -pos.y);
    }

    /// Chooses the hint for when nothing is being dragged, based on what is
    /// under the mouse, so players discover dragging and equipping themselves.
    private string IdleHint(PlacedItem underMouse, Vector2 screen)
    {
        if (underMouse != null)
            return ItemShapes.IsEquippable(underMouse.type)
                ? "Click and drag to move  •  [F] Equip"
                : "Click and drag to move";

        if (equipment.HasEquipped && equippedSlot != null &&
            RectTransformUtility.RectangleContainsScreenPoint(equippedSlot, screen))
            return "[F] Unequip";

        return "Drag items to organise them  •  [R] rotates a held item";
    }

    /// Updates the hint text only when it changes, because setting TMP text
    /// every frame forces the text to be rebuilt unnecessarily.
    private void SetHint(string message)
    {
        if (hintText != null && hintText.text != message) hintText.text = message;
    }

    /// Creates one time and shows an overlay for the drag preview or hover
    /// highlight. Each overlay gets its own sorting canvas because the backpack
    /// and ground panels sort separately, and the overlay must draw above both.
    /// Canvas settings are applied after activating, since Unity only applies
    /// override sorting reliably to an active canvas.
    private RectTransform ShowOverlay(ref RectTransform root, string name, int sortingOrder)
    {
        if (root == null)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(transform, false);
            root = (RectTransform)go.transform;
            root.anchorMin = root.anchorMax = root.pivot = new Vector2(0, 1);
            root.sizeDelta = Vector2.zero;
        }

        root.gameObject.SetActive(true);
        Canvas canvas = root.GetComponent<Canvas>();
        if (canvas == null) canvas = root.gameObject.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = sortingOrder;
        return root;
    }

    /// Fills an overlay with one image per cell of a shape. Neighbouring cells
    /// stretch into the spacing so multi-cell items read as one piece.
    /// Shared by the drag preview and hover highlight so both match the grid.
    private void BuildShapeCells(RectTransform root, Vector2Int[] shape, List<Image> images)
    {
        foreach (Transform child in root) Destroy(child.gameObject);
        images.Clear();

        var cells = new HashSet<Vector2Int>(shape);
        foreach (var offset in shape)
        {
            var go = new GameObject("cell", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(root, false);

            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 1);

            float w = cellSize + (cells.Contains(offset + new Vector2Int(1, 0)) ? spacing : 0f);
            float h = cellSize + (cells.Contains(offset + new Vector2Int(0, 1)) ? spacing : 0f);
            rt.sizeDelta = new Vector2(w, h);
            rt.anchoredPosition = new Vector2(offset.x * Step, -offset.y * Step);

            var img = go.GetComponent<Image>();
            img.raycastTarget = false;
            images.Add(img);
        }
    }
}