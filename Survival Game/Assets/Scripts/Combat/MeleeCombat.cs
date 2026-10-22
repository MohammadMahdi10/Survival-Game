using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeCombat : MonoBehaviour
{
    public Camera playerCamera;
    public float attackRange = 2f;
    public WeaponStats weaponStats;
    public EquipmentManager equipment;

    [Header("Hardcore Mode")]
    public DayNightManager dayNightManager;
    public float hardcoreDamageMultiplier = 0.75f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.Locked)
        {
            DetectHit();
        }
    }

    void DetectHit()
    {
        WeaponData stats = weaponStats.GetStats(equipment.Equipped);
        if (stats == null){
            stats = new WeaponData(1,DamageType.Physical);
        }
        float typeBonus = 1f;
        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, attackRange))
        {
            Debug.Log("Hit: " + hit.collider.name);

            EnemyHealth enemy = hit.collider
                .GetComponentInParent<EnemyHealth>();

            if (enemy != null)
            {
                if (enemy.biomeType == BiomeType.Ice && stats.damageType == DamageType.Fire){
                    typeBonus = 2f;
                }
                float damage = 1f * stats.damageMultiplier * typeBonus;

                if (dayNightManager != null && dayNightManager.hardcoreMode) {
                    damage *= hardcoreDamageMultiplier;
                }

                enemy.TakeDamage(damage);
            }
        }
        else
        {
            Debug.Log("Missed!");
        }
    }

}
