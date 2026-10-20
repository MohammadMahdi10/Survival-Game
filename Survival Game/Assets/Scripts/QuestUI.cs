using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuestUI : MonoBehaviour
{
    [SerializeField] private QuestManager questManager;
    [SerializeField] private TextMeshProUGUI questObjective;

    private void Start() {
        UpdateQuestDisplay();
    }

    public void UpdateQuestDisplay() {
        QuestManager.QuestStage stage = questManager.GetCurrentStage();

        switch (stage) {
            case QuestManager.QuestStage.DefeatBoss:
                questObjective.text = "Defeat the mini boss";
                break;

            case QuestManager.QuestStage.CollectStone:
                questObjective.text = "Collect the repair component";
                break;

            case QuestManager.QuestStage.Tower:
                questObjective.text = "Place item in the central tower";
                break;

            case QuestManager.QuestStage.FinalBoss:
                questObjective.text = "Defeat the final boss.";
                break;

            case QuestManager.QuestStage.CollectComponent:
                questObjective.text = "Collect the final component";
                break;

            case QuestManager.QuestStage.ReturnToSpaceship:
                questObjective.text = "Bring component to spaceship";
                break;

            case QuestManager.QuestStage.Completed:
                questObjective.text = "Space ship repaired!";
                break;

            case QuestManager.QuestStage.Expired:
                questObjective.text = "Space ship destroyed!";
                break;
        }
    }
}
