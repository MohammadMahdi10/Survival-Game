using System;
using System.Collections.Generic;
using UnityEngine;

/// Shows the equipped item's model in the player's first-person view,
/// because playtesters couldn't tell whether equipping had worked.
/// Models are assigned per item in the Inspector, and the script listens
/// to EquipmentManager so equipment logic doesn't depend on visuals.
public class HeldItemView : MonoBehaviour
{
    /// Pairs an item with the model shown when it is equipped, plus how to
    /// hold it. Imported models come in very different sizes and angles,
    /// so each one is fitted here instead of editing third-party prefabs.
    [Serializable]
    public class ItemModel
    {
        public ItemType item;
        public GameObject prefab;

        [Tooltip("Length of the model's longest side in the hand, in metres.")]
        public float size = 0.6f;
        public Vector3 positionOffset;
        public Vector3 rotation;
    }

    [SerializeField] private EquipmentManager equipment;
    [SerializeField] private List<ItemModel> models = new();

    private GameObject current;

    /// Listens for equipment changes and shows the current item straight away.
    private void OnEnable()
    {
        equipment.Changed += Refresh;
        Refresh();
    }

    /// Stops listening while disabled so a hidden view isn't updated.
    private void OnDisable()
    {
        equipment.Changed -= Refresh;
    }

    /// Replaces whatever is in the hand with the newly equipped item's model,
    /// or leaves the hand empty when nothing is equipped.
    private void Refresh()
    {
        if (current != null) Destroy(current);
        current = null;

        if (!equipment.HasEquipped) return;

        ItemModel model = FindModel(equipment.Equipped);
        if (model == null)
        {
            // Warn rather than fail, so a missing model is easy to spot
            // in testing but never stops the player equipping the item.
            Debug.LogWarning("No held model assigned for " + equipment.Equipped);
            return;
        }

        current = Instantiate(model.prefab, transform, false);
        current.transform.localPosition = model.positionOffset;
        current.transform.localRotation = Quaternion.Euler(model.rotation);
        FitToSize(current, model.size);

        // The held item is only visual. Imported models often include
        // colliders, which would block the attack and gathering raycasts
        // fired from the camera.
        foreach (var c in current.GetComponentsInChildren<Collider>())
            Destroy(c);
    }

    /// Scales a model so its longest side matches the target size.
    /// This makes any imported model fit the hand without hand-tuning its
    /// scale, however large or small the original asset was made.
    private static void FitToSize(GameObject model, float targetSize)
    {
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        // Combine every part's bounds, since models are often split into
        // several meshes (e.g. blade and handle).
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        float longest = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        if (longest <= 0f) return;

        model.transform.localScale *= targetSize / longest;
    }

    /// Finds the settings for an item, or null if no model is assigned.
    private ItemModel FindModel(ItemType item)
    {
        foreach (var entry in models)
            if (entry.item == item && entry.prefab != null)
                return entry;
        return null;
    }
}