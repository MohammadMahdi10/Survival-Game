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

    [Header("Hardcore Mode")]
    [SerializeField] private DayNightManager dayNightManager;
    [SerializeField] private float hardcoreDamageMultiplier = 0.75f;

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
