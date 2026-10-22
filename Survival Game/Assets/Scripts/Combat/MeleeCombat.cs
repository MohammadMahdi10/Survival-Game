using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Handles the player's melee attacks.
/// Detects enemies in front of the player and calculates weapon damage,
/// including elemental bonuses, hardcore mode and world difficulty.
public class MeleeCombat : MonoBehaviour
{
    public Camera playerCamera;
    public float attackRange = 2f;
    public WeaponStats weaponStats;
    public EquipmentManager equipment;

    [Header("Hardcore Mode")]
    public DayNightManager dayNightManager;
    public float hardcoreDamageMultiplier = 0.75f;

    [Header("World Difficulty")]
    public WorldDifficultyManager worldDifficultyManager;


    /// Checks for a left mouse click to trigger a melee attack.
    /// Only attacks when the cursor is locked to prevent clicks
    /// on inventory or menu buttons from triggering attacks.

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.Locked)
        {
            DetectHit();
        }
    }


    /// Uses a raycast from the player's camera to detect enemies in range.
    /// Applies weapon damage, elemental bonuses and difficulty modifiers
    /// before passing the final damage value to the enemy.

    void DetectHit()
    {
        WeaponData stats = weaponStats.GetStats(equipment.Equipped);


        // Use basic physical damage when no weapon is equipped,
        // so the player can still attack without a weapon.

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


                // Fire attacks deal double damage to ice enemies,
                // rewarding the player for choosing the right weapon.

                if (enemy.biomeType == BiomeType.Ice && stats.damageType == DamageType.Fire){
                    typeBonus = 2f;
                }
                float damage = 1f * stats.damageMultiplier * typeBonus;

                if (dayNightManager != null && dayNightManager.hardcoreMode) {
                    damage *= hardcoreDamageMultiplier;
                }

                if (worldDifficultyManager != null) {
                    float difficulty = Mathf.Max(1f, worldDifficultyManager.worldDifficulty);
                    damage /= difficulty;
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
