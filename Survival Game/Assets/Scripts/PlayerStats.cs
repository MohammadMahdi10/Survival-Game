
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    [Header("UI")]
    public GameObject gameOverUI;

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float currentStamina = 100f;
    public float staminaDrainRate = 10f;
    public float staminaRegenRate = 10f;

    [Header("Hunger")]
    public float maxHunger = 100f;
    public float currentHunger = 100f;

    [Header("Thirst")]
    public float maxThirst = 100f;
    public float currentThirst = 100f;
    public float hungerDrain = 1f;
    public float thirstDrain = 1f;

    [Header("Combat")]
    public float damageReduction = 1f;

    private bool sprinting;
    private bool blocking;
    private bool isDead = false;

    [Header("Stat Bar Fills")]
    public Image healthFill;
    public Image hungerFill;
    public Image staminaFill;

    private void Awake()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
        currentHunger = Mathf.Clamp(currentHunger, 0f, maxHunger);
        currentThirst = Mathf.Clamp(currentThirst, 0f, maxThirst);

        if (gameOverUI != null)
            gameOverUI.SetActive(false);    
    }

    private void Start()
    {
        StartCoroutine(StatDrain());
    }

    private void Update()
    {
        UpdateStatBars();
        if (isDead) return;

        if (!sprinting && !blocking)
        {
            RestoreStamina(staminaRegenRate * Time.deltaTime);
        }

        // Centralized stamina consumption.
        float drain = 0f;

        if (sprinting)
            drain += staminaDrainRate;

        if (blocking)
            drain += staminaDrainRate;

        if (drain > 0f)
        {
            UseStamina(drain * Time.deltaTime);
        }
    }

    public void SetSprinting(bool active)
    {
        sprinting = active && currentStamina > 0f;
    }

    public void SetBlocking(bool active)
    {
        blocking = active && currentStamina > 0f;
        damageReduction = blocking ? 0.1f : 1f;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Clamp(
            currentHealth - (damageReduction * amount),
            0f, maxHealth
        );

        if (currentHealth <= 0f)
            Die();
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Clamp(
            currentHealth + amount, 0f, maxHealth
        );
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("Player has died!");

        // 1. Display Game Over overlay
        if (gameOverUI != null)
            gameOverUI.SetActive(true);

        // 2. Unlock and show cursor for button interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 3. Disable all movement, look, and input scripts on player and child objects
        MonoBehaviour[] scripts = GetComponentsInChildren<MonoBehaviour>();
        foreach (var script in scripts)
        {
            if (script != this)
            {
                script.enabled = false;
            }
        }

        // 4. Freeze engine time
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // Resume the game
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public bool UseStamina(float amount)
    {
        if (currentStamina < amount)
            return false;

        currentStamina = Mathf.Clamp(
            currentStamina - amount, 0f, maxStamina
        );

        return true;
    }

    public void RestoreStamina(float amount)
    {
        currentStamina = Mathf.Clamp(
            currentStamina + amount, 0f, maxStamina
        );
    }

    public void ChangeHunger(float amount)
    {
        currentHunger = Mathf.Clamp(
            currentHunger + amount, 0f, maxHunger
        );
    }

    public void ChangeThirst(float amount)
    {
        currentThirst = Mathf.Clamp(
            currentThirst + amount, 0f, maxThirst
        );
    }

    private IEnumerator StatDrain()
    {
        while (true)
        {
            ChangeHunger(-hungerDrain);
            ChangeThirst(-thirstDrain);
            yield return new WaitForSeconds(1f);
        }
    }
    private void UpdateStatBars()
    {
        if (healthFill != null)
            healthFill.fillAmount = currentHealth / maxHealth;

        if (hungerFill != null)
            hungerFill.fillAmount = currentHunger / maxHunger;

        if (staminaFill != null)
            staminaFill.fillAmount = currentStamina / maxStamina;
    }
}
