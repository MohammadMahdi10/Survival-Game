using System.Collections.Generic;
using UnityEngine;

public class WorldPickup : MonoBehaviour
{
    public static readonly List<WorldPickup> All = new();

    public ItemType type;

    private void OnEnable() => All.Add(this);
    private void OnDisable() => All.Remove(this);

    public static float DespawnDistance = 30f;
    private float nextCheck;

    private void Update()
    {
        if (Time.time < nextCheck) return;
        nextCheck = Time.time + 10f;

        Camera cam = Camera.main;
        if (cam == null) return;

        float limit = DespawnDistance;
        if ((cam.transform.position - transform.position).sqrMagnitude > limit * limit)
            Destroy(gameObject);
    }

    // Placeholder: a small  cube
    public static WorldPickup Spawn(ItemType type, Vector3 position)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = type + " (pickup)";
        Destroy(go.GetComponent<Collider>());
        go.transform.position = position;
        go.transform.localScale = Vector3.one * 0.4f;
        go.GetComponent<Renderer>().material.color = ItemShapes.GetColor(type);

        var pickup = go.AddComponent<WorldPickup>();
        pickup.type = type;
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