using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.UI;

/// Manages enemy health, damage and death.
/// Handles elemental weaknesses, health bar updates, item drops
/// and quest progression when bosses are defeated.
public enum BiomeType
{
    Default,
    Ice,
    Volcano,
    Forest,
    Desert
}

public class EnemyHealth : MonoBehaviour
{

    [Header("Biome Type")]
    public BiomeType biomeType = BiomeType.Ice;

    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Drop")]
    public bool dropsItem;
    public ItemType dropItem;

    [Header("UI Settings")]
    public Image healthFillImage;

    [Header("Quest Settings")]
    public bool isMiniBoss;
    public bool isFinalBoss;

    [Header("World Difficulty")]
    public WorldDifficultyManager worldDifficultyManager;

    public ItemType lootType = ItemType.BasicSword;
    public float dropRadius = 1.0f;
    [SerializeField] private QuestManager questManager;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    /// Updates the health bar to reflect the enemy's remaining health.
    /// Uses a proportion of maximum health so the same UI works
    /// for enemies with different health values.
    void UpdateHealthUI()
    {
        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = currentHealth / maxHealth;
        }
    }

    /// Applies incoming damage and updates the enemy's health.
    /// Fire weapons deal extra damage to ice enemies to reward
    /// using the correct elemental weapon against their weakness.
    public void TakeDamage(float damage, ItemType weaponType = ItemType.BasicSword)
    {
        float finalDamage = damage;

        if (biomeType == BiomeType.Ice && weaponType == ItemType.FireSword)
        {
            finalDamage *= 2.0f;
            Debug.Log("<color=red>SUPER EFFECTIVE! Fire Sword hit Ice Mini-Boss!</color>");
        }

        currentHealth -= finalDamage;

        Debug.Log(gameObject.name + " took " + finalDamage +
                  " damage. Health remaining: " + currentHealth);
        UpdateHealthUI();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    /// Handles enemy death, including quest updates and item drops.
    /// Defeating a mini-boss also increases world difficulty so
    /// subsequent encounters become more challenging.
    void Die() {

        Debug.Log("Enemy has died!");
        if (isMiniBoss) {
            if (questManager != null) {
                questManager.BossDefeated();
            }

            if (worldDifficultyManager != null) {
                worldDifficultyManager.MiniBossDefeated();
            }
        }

        if (isFinalBoss) {
            if (questManager != null) {
                questManager.FinalBossDefeated();
            }
        }

        if (dropsItem) {

            // Disable colliders before spawning loot so it can
            // appear near the enemy without colliding with its body.
            foreach (Collider c in GetComponentsInChildren<Collider>()) {
                c.enabled = false;
            }

            WorldPickup.DropNear(dropItem, transform.position, 0.5f);
        }

        Destroy(gameObject);
    }

}