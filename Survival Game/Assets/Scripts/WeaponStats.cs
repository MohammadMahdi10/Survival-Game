using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DamageType
{
    Physical,
    Fire,
    Water
}

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

    public float TotalDamage => baseDamage * damageMultiplier;
}

public class WeaponStats : MonoBehaviour
{
    private Dictionary<ItemType, WeaponData> weaponStats;

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

    public WeaponData GetStats(ItemType item)
    {
        return Stats.TryGetValue(item, out var data)
            ? data
            : null;
    }
}
