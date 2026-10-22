using System;
using UnityEngine;

/// Manages the player's equipped item.
/// Allows items to be equipped from an inventory and returns
/// previous equipment to the backpack or world to prevent item loss.
public class EquipmentManager : MonoBehaviour
{
    [SerializeField] private GridInventory backpack;
    [SerializeField] private Transform player;

    public event Action Changed;

    public bool HasEquipped { get; private set; }
    public ItemType Equipped { get; private set; }

    /// Checks whether the player currently has a specific item equipped.
    public bool IsEquipped(ItemType type) => HasEquipped && Equipped == type;

    /// Equips an item from an inventory if it is equippable.
    /// Removes the item from its original grid and returns any previously
    /// equipped item to the backpack so it is not lost.
    /// Notifies other scripts when the equipped item changes.
    public bool Equip(GridInventory from, PlacedItem item)
    {
        if (!ItemShapes.IsEquippable(item.type)) return false;

        ItemType newType = item.type;
        from.Remove(item);


        // Return the previous item before replacing it with the new equipment.
        if (HasEquipped) ReturnToBackpack(Equipped);

        Equipped = newType;
        HasEquipped = true;
        Changed?.Invoke();
        Debug.Log("EQUIPPED: " + Equipped);
        return true;
    }


    /// Unequips the current item and returns it to the backpack.
    /// Notifies other scripts so the equipment display can update.
    public void Unequip()
    {
        if (!HasEquipped) return;

        ItemType old = Equipped;
        HasEquipped = false;
        ReturnToBackpack(old);
        Changed?.Invoke();
    }

    /// Attempts to return an item to the player's backpack.
    /// Drops it near the player if there is no space,
    /// preventing equipped items from being lost.
    private void ReturnToBackpack(ItemType type)
    {
        if (backpack.TryAddItem(type) != null) return;

        Vector3 center = player.position + player.forward * 1.5f;
        WorldPickup.DropNear(type, center, 0.7f);
        Debug.Log(type + " didn't fit in the backpack, so it was dropped on the ground");
    }
}