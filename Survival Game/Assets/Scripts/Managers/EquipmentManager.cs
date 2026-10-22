using System;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    [SerializeField] private GridInventory backpack;
    [SerializeField] private Transform player;

    public event Action Changed;

    public bool HasEquipped { get; private set; }
    public ItemType Equipped { get; private set; }

    public bool IsEquipped(ItemType type) => HasEquipped && Equipped == type;

    // Moves an item out of a grid and into the equipment slot
    public bool Equip(GridInventory from, PlacedItem item)
    {
        if (!ItemShapes.IsEquippable(item.type)) return false;

        ItemType newType = item.type;
        from.Remove(item);

        // Whatever was equipped goes back to the backpack
        if (HasEquipped) ReturnToBackpack(Equipped);

        Equipped = newType;
        HasEquipped = true;
        Changed?.Invoke();
        Debug.Log("EQUIPPED: " + Equipped);
        return true;
    }

    public void Unequip()
    {
        if (!HasEquipped) return;

        ItemType old = Equipped;
        HasEquipped = false;
        ReturnToBackpack(old);
        Changed?.Invoke();
    }

    private void ReturnToBackpack(ItemType type)
    {
        if (backpack.TryAddItem(type) != null) return;

        Vector3 center = player.position + player.forward * 1.5f;
        WorldPickup.DropNear(type, center, 0.7f);
        Debug.Log(type + " didn't fit in the backpack, so it was dropped on the ground");
    }
}