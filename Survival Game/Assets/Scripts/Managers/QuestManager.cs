using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Manages the player's main quest progression.
/// Keeps objectives in a fixed order and updates the quest UI
/// whenever the player completes the current objective.
public class QuestManager : MonoBehaviour {

    [SerializeField] private QuestUI questUI;

    [Header("Quest Locations")]
    [SerializeField] private Transform firstBossLocation;
    private Transform stoneLocation;
    [SerializeField] private Transform towerLocation;
    [SerializeField] private Transform finalBossLocation;
    private Transform componentLocation;
    [SerializeField] private Transform spaceshipLocation;

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

    /// Registers both the  stone's and components location respectively when it drops from the boss.
    /// Allows quest navigation to track dynamically spawned items
    /// instead of relying on a predefined scene location.
    public void SetStoneLocation(Transform stone) {
        stoneLocation = stone;
    }

    public void SetComponentLocation(Transform component)
    {
        componentLocation = component;
    }

    /// Returns the destination of the player's current quest.
    /// Uses the active quest stage to identify which location
    /// the navigation system should guide the player towards.
    public Transform GetCurrentQuestTarget() {
        switch (currentStage) {
            case QuestStage.DefeatBoss: {
                    return firstBossLocation;
                }
            case QuestStage.CollectStone: {
                    return stoneLocation;
                }
            case QuestStage.Tower: {
                    return towerLocation;
                }
            case QuestStage.FinalBoss: {
                    return finalBossLocation;
                }
            case QuestStage.CollectComponent: {
                    return componentLocation;
                }
            case QuestStage.ReturnToSpaceship: {
                    return spaceshipLocation;
                }
            default: {
                    return null;
                }
        }
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
