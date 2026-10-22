using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.UI;
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

    public ItemType lootType = ItemType.BasicSword;
    public float dropRadius = 1.0f;
    [SerializeField] private QuestManager questManager;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = currentHealth / maxHealth;
        }
    }

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

    void Die() {

        Debug.Log("Enemy has died!");
        if (isMiniBoss)
        {
            if (questManager != null) {
                questManager.BossDefeated();
            }
        }

        if (isFinalBoss) {
            if (questManager != null) {
                questManager.FinalBossDefeated();
            }
        }

        if (dropsItem) {
            foreach (Collider c in GetComponentsInChildren<Collider>()) {
                c.enabled = false;
            }

            WorldPickup.DropNear(dropItem, transform.position, 0.5f);
        }

        Destroy(gameObject);
    }

}