
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Handles the player's melee attacks.
/// Supports area attacks, elemental damage and difficulty modifiers.
public class MeleeCombat : MonoBehaviour
{
    [Header("Attack Settings")]
    public Camera playerCamera;
    public float attackAngle = 120f;

    public WeaponStats weaponStats;
    public EquipmentManager equipment;

    [Header("Hardcore Mode")]
    public DayNightManager dayNightManager;
    public float hardcoreDamageMultiplier = 0.75f;

    [Header("World Difficulty")]
    public WorldDifficultyManager worldDifficultyManager;

    public bool attackReady = true;
    private bool coroutineReady = false;

    /// Checks for a left mouse click to trigger a melee attack.
    /// Only attacks when the cursor is locked to prevent clicks
    /// on inventory or menu buttons from triggering attacks.
    void Update()
    {
        if (Input.GetMouseButtonDown(0) &&
            Cursor.lockState == CursorLockMode.Locked)
        {
            if (attackReady)
            {
                DetectHit();
            }
        }
    }

    void DetectHit()
    {
        StartCoroutine(attackCooldown(1f));
        WeaponData stats = weaponStats.GetStats(equipment.Equipped);

        // Allow unarmed attacks.
        if (stats == null)
        {
            stats = new WeaponData(1, DamageType.Physical, 1f, 90f);
        }

        // Finds every collider within attack range.
        Collider[] hits = Physics.OverlapSphere(
            playerCamera.transform.position,
            stats.damageRange
        );

        HashSet<EnemyHealth> hitEnemies = new HashSet<EnemyHealth>();

        bool hitSomething = false;

        foreach (Collider hit in hits)
        {
            EnemyHealth enemy = hit.GetComponentInParent<EnemyHealth>();

            if (enemy == null || !hitEnemies.Add(enemy))
                continue;

            Vector3 direction = enemy.transform.position - playerCamera.transform.position;
            direction.y = 0f;
            Vector3 forward = playerCamera.transform.forward;
            forward.y = 0f;
            float angle = Vector3.Angle(
                forward.normalized,
                direction.normalized
            );

            if (angle > stats.damageArea / 2f)
                continue;

            hitSomething = true;

            Debug.Log("Hit: " + enemy.name);

            ApplyDamage(enemy, stats);
        }

        if (!hitSomething)
        {
            Debug.Log("Missed!");
        }
    }

    void ApplyDamage(EnemyHealth enemy, WeaponData stats)
    {
        float typeBonus = 1f;
 
        if (enemy.biomeType == BiomeType.Ice &&
            stats.damageType == DamageType.Fire)
        {
            typeBonus = 2f;
        }

        float damage = stats.damageMultiplier * typeBonus;

        if (dayNightManager != null &&
            dayNightManager.hardcoreMode)
        {
            damage *= hardcoreDamageMultiplier;
        }

        if (worldDifficultyManager != null)
        {
            float difficulty = Mathf.Max(
                1f,
                worldDifficultyManager.worldDifficulty
            );

            damage /= difficulty;
        }

        enemy.TakeDamage(damage);
    }

    private IEnumerator attackCooldown(float cooldown)
    {
        attackReady = false;
        yield return new WaitForSeconds(cooldown);
        attackReady = true;
    }
}
