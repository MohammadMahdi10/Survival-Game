using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// Manages the game's world difficulty multiplier.
/// Increases difficulty when a mini-boss is defeated
/// and displays the current multiplier on the player's HUD.
public class WorldDifficultyManager : MonoBehaviour {
    [Header("Difficulty Settings")]
    public float worldDifficulty = 1f;
    public float difficultyIncreasePerBoss = 0.5f;

    [Header("UI")]
    public TMP_Text difficultyText;

    /// Displays the starting difficulty when the game begins
    /// so the HUD shows the correct multiplier immediately.
    private void Start() {
        UpdateUI();
    }

    /// Increases world difficulty after a mini-boss is defeated.
    /// This makes the game more challenging as the player progresses.
    public void MiniBossDefeated() {
        worldDifficulty += difficultyIncreasePerBoss;

        UpdateUI();
    }

    /// Updates the displayed difficulty multiplier.
    /// Keeps the HUD in sync whenever the difficulty changes.
    private void UpdateUI() {
        if (difficultyText != null) {
            difficultyText.text = "x" + worldDifficulty.ToString("0.0");
        }
    }
}
