
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour {

    [SerializeField] private QuestUI questUI;

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

    public QuestStage GetCurrentStage() {
        return currentStage;
    }

    public void UpdateQuest() {
        questUI.UpdateQuestDisplay();
    }

    public void BossDefeated() {
        if (currentStage != QuestStage.DefeatBoss) {
            return;
        }

        currentStage = QuestStage.CollectStone;
        UpdateQuest();
    }

    public void StoneCollected() {
        if (currentStage != QuestStage.CollectStone) {
            return;
        }

        currentStage = QuestStage.Tower;
        UpdateQuest();
    }

    public void StonePlaced() {
        if (currentStage != QuestStage.Tower) {
            return;
        }

        currentStage = QuestStage.FinalBoss;
        UpdateQuest();
    }

    public void FinalBossDefeated() {
        if (currentStage != QuestStage.FinalBoss) {
            return;
        }

        currentStage = QuestStage.CollectComponent;
        UpdateQuest();
    }

    public void ComponentCollected() {
        if (currentStage != QuestStage.CollectComponent) {
            return;
        }

        currentStage = QuestStage.ReturnToSpaceship;
        UpdateQuest();
    }
}
