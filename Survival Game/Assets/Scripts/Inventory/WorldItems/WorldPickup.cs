using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// Manages items dropped into the game world.
/// Creates item visuals and labels, places items on the ground,
/// and removes distant pickups while protecting important quest items.
public class WorldPickup : MonoBehaviour
{
    public static readonly List<WorldPickup> All = new();
    public static float DespawnDistance = 30f;

    public ItemType type;

    private Transform label;
    private float nextCheck;

    /// Adds this pickup to the list of active world pickups
    /// when the GameObject becomes enabled.
    private void OnEnable() => All.Add(this);

    /// Removes this pickup from the list of active world pickups
    /// when the GameObject becomes disabled.
    private void OnDisable() => All.Remove(this);


    /// Removes ordinary pickups when they are too far from the camera
    /// to prevent unnecessary objects from building up in the world.
    /// Checks once per second to avoid unnecessary calculations.
    private void Update()
    {
        if (type == ItemType.SpecialItem || type == ItemType.RepairComponent) {
            return;
        }

        if (Time.time < nextCheck) return;
        nextCheck = Time.time + 1f;

        Camera cam = Camera.main;
        if (cam == null) return;

        float limit = DespawnDistance;
        if ((cam.transform.position - transform.position).sqrMagnitude > limit * limit)
            Destroy(gameObject);
    }


    /// Keeps the item's label facing the camera so it remains readable
    /// as the player moves around the world.
    private void LateUpdate()
    {
        if (label == null) return;
        Camera cam = Camera.main;
        if (cam != null) label.rotation = cam.transform.rotation;
    }


    /// Creates a world pickup using a coloured cube and item name.
    /// Uses shared item definitions so all pickups display consistently
    /// without requiring separate prefabs for every item type.
    public static WorldPickup Spawn(ItemType type, Vector3 position)
    {
        var root = new GameObject(type + " (pickup)");
        root.transform.position = position;

        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "Visual";
        Destroy(cube.GetComponent<Collider>());
        cube.transform.SetParent(root.transform, false);
        cube.transform.localScale = Vector3.one * 0.4f;
        cube.GetComponent<Renderer>().material.color = ItemShapes.GetColor(type);

        var labelGo = new GameObject("Label");
        labelGo.transform.SetParent(root.transform, false);
        labelGo.transform.localPosition = new Vector3(0f, 0.55f, 0f);

        var text = labelGo.AddComponent<TextMeshPro>();
        text.text = ItemShapes.GetName(type);
        text.fontSize = 2f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.outlineWidth = 0.2f;
        text.outlineColor = Color.black;

        var pickup = root.AddComponent<WorldPickup>();
        pickup.type = type;
        pickup.label = labelGo.transform;
        return pickup;
    }


    /// Spawns an item at a random position near the specified point.
    /// Uses a downward raycast to place the item on the ground,
    /// helping prevent it from appearing underground or floating.
    public static WorldPickup DropNear(ItemType type, Vector3 center, float radius)
    {
        Vector2 offset = Random.insideUnitCircle * radius;
        Vector3 start = center + new Vector3(offset.x, 2f, offset.y);
        Vector3 pos = new Vector3(start.x, center.y, start.z);

        if (Physics.Raycast(start, Vector3.down, out RaycastHit hit, 10f, ~0,
                            QueryTriggerInteraction.Ignore))
            pos = hit.point;

        return Spawn(type, pos + Vector3.up * 0.2f);
    }
}