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

    [Header("UI Settings")]
    public Image healthFillImage;

    public GameObject itemDropPrefab; // Assign the item prefab in the Inspector
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
        if (itemDropPrefab != null)
        {
            Vector3 dropPosition = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
            Instantiate(itemDropPrefab, dropPosition, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}