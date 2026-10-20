using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WorldPickup : MonoBehaviour
{
    public static readonly List<WorldPickup> All = new();
    public static float DespawnDistance = 30f;

    public ItemType type;

    private Transform label;
    private float nextCheck;

    private void OnEnable() => All.Add(this);
    private void OnDisable() => All.Remove(this);

    private void Update()
    {
        // The special item never despawns, or the player could lose it for good
        if (type == ItemType.SpecialItem) return;

        // Only check once a second, it doesn't need to run every frame
        if (Time.time < nextCheck) return;
        nextCheck = Time.time + 1f;

        Camera cam = Camera.main;
        if (cam == null) return;

        float limit = DespawnDistance;
        if ((cam.transform.position - transform.position).sqrMagnitude > limit * limit)
            Destroy(gameObject);
    }

    private void LateUpdate()
    {
        // Keep the label facing the player
        if (label == null) return;
        Camera cam = Camera.main;
        if (cam != null) label.rotation = cam.transform.rotation;
    }

    // Placeholder visual: a small coloured cube with a name above it
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

    // Drops an item on the ground at a random spot around a point
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