using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
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

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        Debug.Log(gameObject.name + " took " + damage +
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

        if (dropsItem) {
            foreach (Collider c in GetComponentsInChildren<Collider>()) {
                c.enabled = false;
            }

            WorldPickup.DropNear(dropItem, transform.position, 0.5f);
        }

        Destroy(gameObject);
    }

}