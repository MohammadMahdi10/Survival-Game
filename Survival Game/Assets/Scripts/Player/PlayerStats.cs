using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// Handles the player's stats and survival mechanics.
/// Manages health, stamina, hunger, damage reduction, difficulty scaling, and death.
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
    public float hungerDrain = 1f;

    [Header("Combat")]
    public float damageReduction = 1f;

    private bool sprinting;
    private bool blocking;
    private bool isDead = false;

    [Header("Stat Bar Fills")]
    public Image healthFill;
    public Image hungerFill;
    public Image staminaFill;

    [Header("Hardcore Mode")]
    public DayNightManager dayNightManager;
    public GameObject respawnButton;

    [Header("Hardcore Stat Multipliers")]
    public float hardcoreHealthMultiplier = 0.8f;
    public float hardcoreStaminaMultiplier = 0.8f;
    public float hardcoreHungerDrainMultiplier = 1.5f;

    private bool hardcoreStatsApplied = false;

    [Header("World Difficulty")]
    public WorldDifficultyManager worldDifficultyManager;

    public float worldHealthPenalty = 0.4f;
    public float worldStaminaPenalty = 0.4f;

    private float baseMaxHealth;
    private float baseMaxStamina;
    private float baseHungerDrain;

    /// Initialises the player's stats and stores their base values.
    /// Hides the Game Over UI and ensures current stats are within their limits.
    private void Awake()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
        currentHunger = Mathf.Clamp(currentHunger, 0f, maxHunger);

        if (gameOverUI != null)
            gameOverUI.SetActive(false);

        baseMaxHealth = maxHealth;
        baseMaxStamina = maxStamina;
        baseHungerDrain = hungerDrain;
    }

    /// Starts the coroutine that gradually drains the player's hunger.
    private void Start()
    {
        StartCoroutine(StatDrain());
    }

    /// Updates the player's stats and UI every frame.
    /// Applies hardcore and world difficulty settings and manages stamina consumption and regeneration.
    private void Update()
    {
        if (dayNightManager != null && dayNightManager.hardcoreMode && !hardcoreStatsApplied) {
            ApplyHardcoreStats();
        }

        UpdateDifficultyStats();
        UpdateStatBars();
        if (isDead) return;

        if (!sprinting && !blocking)
        {
            RestoreStamina(staminaRegenRate * Time.deltaTime);
        }

        // Calculates stamina consumption based on the player's current actions.
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

    /// Updates whether the player is sprinting.
    /// Prevents sprinting when the player has no stamina remaining.
    public void SetSprinting(bool active)
    {
        sprinting = active && currentStamina > 0f;
    }

    /// Updates whether the player is blocking.
    /// Reduces incoming damage while blocking and restores normal damage when blocking stops.
    public void SetBlocking(bool active)
    {
        blocking = active && currentStamina > 0f;
        damageReduction = blocking ? 0.1f : 1f;
    }

    /// Reduces the player's health when they take damage.
    /// Causes the player to die when their health reaches zero.
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

    /// Restores the player's health by the specified amount.
    /// Prevents health from exceeding its maximum value.
    public void Heal(float amount)
    {
        currentHealth = Mathf.Clamp(
            currentHealth + amount, 0f, maxHealth
        );
    }

    /// Handles the player's death.
    /// Displays the Game Over UI, disables player controls, and freezes the game.
    /// Hides the respawn button when hardcore mode is active.
    private void Die()
    {
        isDead = true;
        Debug.Log("Player has died!");

        // Displays the Game Over overlay.
        if (gameOverUI != null)
            gameOverUI.SetActive(true);

        if (dayNightManager != null && dayNightManager.hardcoreMode) {
            if (respawnButton != null) {
                respawnButton.SetActive(false);
            }
        }

        // Unlocks and displays the cursor for button interaction.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Disables movement, camera, and other scripts on the player and its children.
        MonoBehaviour[] scripts = GetComponentsInChildren<MonoBehaviour>();
        foreach (var script in scripts)
        {
            if (script != this)
            {
                script.enabled = false;
            }
        }

        // Freezes the game when the player dies.
        Time.timeScale = 0f;
    }

    /// Restarts the current scene.
    /// Resets the game's time scale before reloading the scene.
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// Consumes the specified amount of stamina.
    /// Returns false if the player does not have enough stamina.
    public bool UseStamina(float amount)
    {
        if (currentStamina < amount)
            return false;

        currentStamina = Mathf.Clamp(
            currentStamina - amount, 0f, maxStamina
        );

        return true;
    }

    /// Restores the player's stamina by the specified amount.
    /// Prevents stamina from exceeding its maximum value.
    public void RestoreStamina(float amount)
    {
        currentStamina = Mathf.Clamp(
            currentStamina + amount, 0f, maxStamina
        );
    }

    /// Changes the player's hunger by the specified amount.
    /// Allows hunger to increase or decrease without exceeding its limits.
    public void ChangeHunger(float amount)
    {
        currentHunger = Mathf.Clamp(
            currentHunger + amount, 0f, maxHunger
        );
    }

    /// Gradually drains the player's hunger over time.
    /// Continues reducing hunger once every second.
    private IEnumerator StatDrain()
    {
        while (true)
        {
            ChangeHunger(-hungerDrain);
            yield return new WaitForSeconds(1f);
        }
    }

    /// Updates the health, hunger, and stamina UI bars.
    /// Sets each bar's fill amount based on the player's current stats.
    private void UpdateStatBars()
    {
        if (healthFill != null)
            healthFill.fillAmount = currentHealth / maxHealth;

        if (hungerFill != null)
            hungerFill.fillAmount = currentHunger / maxHunger;

        if (staminaFill != null)
            staminaFill.fillAmount = currentStamina / maxStamina;
    }

    /// Marks hardcore stat adjustments as applied.
    /// Prevents hardcore stats from being applied more than once.
    private void ApplyHardcoreStats() {
        if (hardcoreStatsApplied) {
            return;
        }

        hardcoreStatsApplied = true;
    }

    /// Adjusts the player's maximum health, stamina, and hunger drain.
    /// Applies hardcore mode multipliers and penalties based on world difficulty.
    /// Keeps current health and stamina within their updated maximum values.
    public void UpdateDifficultyStats() {
        float difficulty = worldDifficultyManager != null ? worldDifficultyManager.worldDifficulty : 1f;

        float increase = Mathf.Max(0f, difficulty - 1f);

        float hardcoreHealth = hardcoreStatsApplied ? hardcoreHealthMultiplier : 1f;
        float hardcoreStamina = hardcoreStatsApplied ? hardcoreStaminaMultiplier : 1f;
        float hardcoreHunger = hardcoreStatsApplied ? hardcoreHungerDrainMultiplier : 1f;

        float newMaxHealth = baseMaxHealth * hardcoreHealth * Mathf.Max(0.1f, 1f - increase * worldHealthPenalty);
        float newMaxStamina = baseMaxStamina * hardcoreStamina * Mathf.Max(0.1f, 1f - increase * worldStaminaPenalty);

        maxHealth = newMaxHealth;
        maxStamina = newMaxStamina;

        currentHealth = Mathf.Min(currentHealth, maxHealth);
        currentStamina = Mathf.Min(currentStamina, maxStamina);

        hungerDrain = baseHungerDrain * hardcoreHunger * difficulty;
    }
}
