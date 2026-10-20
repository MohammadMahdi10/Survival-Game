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

    public ItemType lootType = ItemType.BasicSword;
    public float dropRadius = 1.0f;

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

    private void OnMouseDown()
    {
        TakeDamage(10f);
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        UpdateHealthUI();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        WorldPickup.DropNear(lootType, transform.position, dropRadius);
        if (dropsItem)
        {
            // Switch off colliders first so the drop's ground check doesn't land on the boss
            foreach (var c in GetComponentsInChildren<Collider>())
                c.enabled = false;

            WorldPickup.DropNear(dropItem, transform.position, 0.5f);
        }

        Destroy(gameObject);
    }
}