using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WorldDifficultyManager : MonoBehaviour {
    [Header("Difficulty Settings")]
    public float worldDifficulty = 1f;
    public float difficultyIncreasePerBoss = 0.5f;

    [Header("UI")]
    public TMP_Text difficultyText;

    private void Start() {
        UpdateUI();
    }

    public void MiniBossDefeated() {
        worldDifficulty += difficultyIncreasePerBoss;

        UpdateUI();
    }

    private void UpdateUI() {
        if (difficultyText != null) {
            difficultyText.text = "x" + worldDifficulty.ToString("0.0");
        }
    }
}
