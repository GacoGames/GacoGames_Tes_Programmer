using UnityEngine;
using GacoGames.QuestSystem;
using TMPro;

public class QuestNotification : MonoBehaviour
{
    public TextMeshProUGUI notifText;

    private void OnEnable()
    {
        QuestManager.OnQuestChainReceive += NotifyQuestReceived;
        QuestManager.OnQuestChainComplete += NotifyQuestCompleted;
        QuestManager.OnObjectiveIncrement += NotifyObjectiveIncrement;
        QuestManager.OnObjectiveComplete += NotifyObjectiveComplete;
        QuestManager.OnProceedNextQuest += NotifyProceedNextQuest;
    }

    private void OnDisable()
    {
        QuestManager.OnQuestChainReceive -= NotifyQuestReceived;
        QuestManager.OnQuestChainComplete -= NotifyQuestCompleted;
        QuestManager.OnObjectiveIncrement -= NotifyObjectiveIncrement;
        QuestManager.OnObjectiveComplete -= NotifyObjectiveComplete;
        QuestManager.OnProceedNextQuest -= NotifyProceedNextQuest;
    }

    private void NotifyQuestReceived(QuestChain questChain)
    {
        notifText.text = $"Quest Received: {questChain.questChainId}";
    }
    private void NotifyQuestCompleted(QuestChain questChain)
    {
        notifText.text = $"Quest Completed: {questChain.questChainId}";
    }
    private void NotifyObjectiveIncrement(QuestChain chain, QuestInstance instance)
    {
        Quest activeQ = chain.GetQuestInfo(instance.activeQuestIndex);
        string result = string.Empty;
        for(int i = 0; i < activeQ.Objectives.Count; i++)
        {
            Objective obj = activeQ.Objectives[i];
            int progress = instance.objectiveProgress[i];
            int max = obj.requiredAmount;
            result += $"{obj.Description} : {progress}/{max}\n";
        }
        notifText.text = result;
    }
    private void NotifyObjectiveComplete(QuestChain chain, QuestInstance instance)
    {
        notifText.text = $"Objective Completed";
    }
    private void NotifyProceedNextQuest(Quest quest)
    {
        notifText.text = $"Proceed to Next Quest: {quest.summary}";
    }
}
