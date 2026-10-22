using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Defines the different types of damage a weapon can deal.
public enum DamageType
{
    Physical,
    Fire,
    Water
}

/// Stores a weapon's damage values and damage type.
[System.Serializable]
public class WeaponData
{
    public float baseDamage;
    public float damageMultiplier;
    public DamageType damageType;

    public WeaponData(float damageMultiplier, DamageType damageType)
    {
        this.damageMultiplier = damageMultiplier;
        this.damageType = damageType;
    }

    /// Calculates the weapon's total damage using its base damage and multiplier.
    public float TotalDamage => baseDamage * damageMultiplier;
}

/// Stores weapon statistics and provides methods to retrieve weapon data
/// and calculate damage based on the current game mode.
public class WeaponStats : MonoBehaviour
{
    private Dictionary<ItemType, WeaponData> weaponStats;

    [Header("Hardcore Mode")]
    [SerializeField] private DayNightManager dayNightManager;
    [SerializeField] private float hardcoreDamageMultiplier = 0.75f;


    /// Creates the weapon statistics dictionary when first accessed.
    /// Stores each weapon's damage multiplier and elemental damage type.
    private Dictionary<ItemType, WeaponData> Stats
    {
        get
        {
            if (weaponStats == null)
            {
                weaponStats = new Dictionary<ItemType, WeaponData>
                {
                    [ItemType.BasicSword] = new WeaponData(
                        10f, DamageType.Physical
                    ),

                    [ItemType.FireSword] = new WeaponData(
                        10f, DamageType.Fire
                    ),
                };
            }

            return weaponStats;
        }
    }


    /// Retrieves the statistics for a specific weapon.
    /// Returns null if the weapon has no stored statistics.
    public WeaponData GetStats(ItemType item)
    {
        return Stats.TryGetValue(item, out var data)
            ? data
            : null;
    }


    /// Calculates a weapon's damage and applies the hardcore damage
    /// reduction when hardcore mode is active.
    /// Returns zero if the weapon has no stored statistics.
    public float GetDamage(ItemType item) {
        WeaponData data = GetStats(item);

        if (data == null) {
            return 0f;
        }

        float damage = data.TotalDamage;

        if (dayNightManager != null && dayNightManager.hardcoreMode) {
            damage *= hardcoreDamageMultiplier;
        }

        return damage;
    }
}
