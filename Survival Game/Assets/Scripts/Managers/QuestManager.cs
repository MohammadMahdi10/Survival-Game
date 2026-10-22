using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Manages the player's main quest progression.
/// Keeps objectives in a fixed order and updates the quest UI
/// whenever the player completes the current objective.
public class QuestManager : MonoBehaviour {

    [SerializeField] private QuestUI questUI;

    /// Defines the possible stages of the main quest,
    /// from defeating the first boss to completing the game.
    public enum QuestStage {
        DefeatBoss,
        CollectStone,
        Tower,
        FinalBoss,
        CollectComponent,
        ReturnToSpaceship,
        Completed,
        Expired
    }

    private QuestStage currentStage = QuestStage.DefeatBoss;

    /// Returns the current quest stage so other scripts
    /// can check which objective the player is completing.
    public QuestStage GetCurrentStage() {
        return currentStage;
    }

    /// Refreshes the quest display after a stage changes,
    /// keeping the objective text in sync with quest progression.
    public void UpdateQuest() {
        questUI.UpdateQuestDisplay();
    }

    /// Advances the quest after the first boss is defeated.
    /// Only progresses from the correct stage to prevent
    /// objectives from being completed out of order.
    public void BossDefeated() {
        if (currentStage != QuestStage.DefeatBoss) {
            return;
        }

        currentStage = QuestStage.CollectStone;
        UpdateQuest();
    }

    /// Advances the quest when the special stone is collected.
    /// Requires the boss objective to be completed first.
    public void StoneCollected() {
        if (currentStage != QuestStage.CollectStone) {
            return;
        }

        currentStage = QuestStage.Tower;
        UpdateQuest();
    }

    /// Advances the quest after the stone is placed at the tower.
    /// Prevents the final boss objective from starting too early.
    public void StonePlaced() {
        if (currentStage != QuestStage.Tower) {
            return;
        }

        currentStage = QuestStage.FinalBoss;
        UpdateQuest();
    }

    /// Advances the quest when the final boss is defeated.
    /// Only accepts this event during the final boss stage.
    public void FinalBossDefeated() {
        if (currentStage != QuestStage.FinalBoss) {
            return;
        }

        currentStage = QuestStage.CollectComponent;
        UpdateQuest();
    }

    /// Advances the quest after collecting the repair component.
    /// Ensures the final boss has been defeated before the player
    /// can progress to repairing the spaceship.
    public void ComponentCollected() {
        if (currentStage != QuestStage.CollectComponent) {
            return;
        }

        currentStage = QuestStage.ReturnToSpaceship;
        UpdateQuest();
    }

    /// Completes the main quest when the spaceship is repaired.
    /// Requires the player to reach the final objective first,
    /// preventing the game from being completed out of sequence.
    public void SpaceshipRepaired() {
        if (currentStage != QuestStage.ReturnToSpaceship)
        {
            return;
        }

        currentStage = QuestStage.Completed;
        UpdateQuest();
    }

}
