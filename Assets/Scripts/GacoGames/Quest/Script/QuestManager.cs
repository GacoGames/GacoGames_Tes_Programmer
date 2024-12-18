using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using Newtonsoft.Json;

namespace GacoGames.QuestSystem
{
    public class QuestManager : SerializedMonoBehaviour
    {
        public static QuestManager Instance;
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                Init();
            }
        }
        public static event Action<QuestChain> OnQuestChainReceive;
        public static event Action<QuestChain> OnQuestChainComplete;
        public static event Action<QuestChain, QuestInstance> OnObjectiveIncrement;
        public static event Action<QuestChain, QuestInstance> OnObjectiveComplete;
        public static event Action<Quest> OnProceedNextQuest;
        public static event Action OnQuestTargetUpdate;

        //Playerprefs Key
        public static string QUEST_ONGOING = "QUEST_ONGOING";
        public static string QUEST_COMPLETED = "QUEST_COMPLETED";
        public static string QUEST_COMPLETED_SIDE = "QUEST_COMPLETED_SIDE";

        [SerializeField] 
        private QuestDatabase database;
        [ReadOnly, FoldoutGroup("Quest Progression")]
        public Dictionary<string, QuestInstance> ongoingQuests;
        [ReadOnly, FoldoutGroup("Quest Progression")]
        public List<QuestRecord> completedMainQuests;
        [ReadOnly, FoldoutGroup("Quest Progression")]
        public List<QuestRecord> completedSideQuests;
        [ReadOnly, FoldoutGroup("Quest Progression")]
        public Dictionary<string, List<string>> allQuestTargets;

        [ButtonGroup("Quest Progression/btn"), Button]
        private void Init()
        {
            database.Init();

            ongoingQuests = new Dictionary<string, QuestInstance>();
            allQuestTargets = new Dictionary<string, List<string>>();

            LoadQuestProgression();
        }
        // [ButtonGroup("Quest Progression/btn"), Button("Load")]
        private void LoadQuestProgression()
        {
            //Loads all quests that has been received by players
            //Also loads completed quests, as they can be used for archives
            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                TypeNameHandling = TypeNameHandling.Auto
            };

            //sementara pake playerprefs dulu.
            string ongoing = PlayerPrefs.GetString(QUEST_ONGOING);
            ongoingQuests = JsonConvert.DeserializeObject<Dictionary<string, QuestInstance>>(ongoing, jsonSettings);

            string completed = PlayerPrefs.GetString(QUEST_COMPLETED);
            completedMainQuests = JsonConvert.DeserializeObject<List<QuestRecord>>(completed, jsonSettings);

            string completedSq = PlayerPrefs.GetString(QUEST_COMPLETED_SIDE);
            completedSideQuests = JsonConvert.DeserializeObject<List<QuestRecord>>(completedSq, jsonSettings);

            BuildTargetList();
        }
        // [ButtonGroup("Quest Progression/btn"), Button("Save")]
        private void SaveQuestProgression()
        {
            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                TypeNameHandling = TypeNameHandling.Auto
            };

            var ongoing = JsonConvert.SerializeObject(ongoingQuests, jsonSettings);
            PlayerPrefs.SetString(QUEST_ONGOING, ongoing);

            var completed = JsonConvert.SerializeObject(completedMainQuests, jsonSettings);
            PlayerPrefs.SetString(QUEST_COMPLETED, completed);

            GUIUtility.systemCopyBuffer = ongoing;
        }
        private void BuildTargetList()
        {
            //connects target to multiple quests that share the same target
            //For example : Quest 'Boar extermination' / 'Promotional task' / 'Danger in the Woods' all shares the target 'enemy_boar'
            //Defeating 1 Boar will advance all three quests' objective
            ongoingQuests ??= new Dictionary<string, QuestInstance>();
            allQuestTargets.Clear();

            foreach (var questProgress in ongoingQuests)
            {
                QuestChain questChain = database.GetQuest(questProgress.Key);
                for (int i = 0; i < questChain.Quests.Count; i++)
                {
                    Quest quest = questChain.Quests[i];

                    if (questProgress.Value.activeQuestIndex != i) continue; //only list target in active quest

                    for (int j = 0; j < quest.Objectives.Count; j++)
                    {
                        Objective objective = quest.Objectives[j];

                        if (questProgress.Value.objectiveProgress[j] >= objective.RequiredAmount) continue; //don't list already-done objective target

                        string target = objective.QuestTarget;

                        if (!allQuestTargets.ContainsKey(target))
                        {
                            allQuestTargets.Add(target, new List<string>() { questChain.questChainId });
                        }
                        else
                        {
                            if (!allQuestTargets[target].Contains(questChain.questChainId))
                                allQuestTargets[target].Add(questChain.questChainId);
                        }
                    }
                }
            }

            OnQuestTargetUpdate?.Invoke();
        }

        #region Quest Progression
        public void ReceiveNewQuest(QuestChain quest)
        {
            //check duplicate quest. just do nothing if it ever happen.
            //duplicate quest might happen due to editorial human error, or probably some backend issue.
            allQuestTargets ??= new Dictionary<string, List<string>>();
            ongoingQuests ??= new Dictionary<string, QuestInstance>();

            bool duplicate = ongoingQuests.ContainsKey(quest.questChainId);
            bool alreadyCompleted = IsQuestCompleted(quest.questChainId);

            if (IsQuestCompleted(quest.questChainId))
            {
                Debug.Log($"Quest {quest.questChainId} has been completed.");
                return;
            }

            if (!duplicate)
            {
                ongoingQuests.Add(quest.questChainId, new QuestInstance(quest));
                BuildTargetList();

                OnQuestChainReceive?.Invoke(quest);
            }
        }
        public void DoObjective(string objectId)
        {
            if (!allQuestTargets.ContainsKey(objectId))
            {
                Debug.Log($"{objectId} is not a target of any ongoing Quest.");
                return;
            }

            var linkedQuests = allQuestTargets[objectId];
            foreach (var questId in linkedQuests)
            {
                DoObjective(questId, objectId);
            }
        }
        public void DoObjective(string questChainId, string objectId)
        {
            if (!allQuestTargets.ContainsKey(objectId))
            {
                Debug.Log($"{objectId} is not a target of any ongoing Quest.");
                return;
            }

            if (!ongoingQuests.ContainsKey(questChainId))
            {
                Debug.Log($"{questChainId} is not an active Quest.");
                return;
            }

            QuestChain questChain = database.GetQuest(questChainId);
            QuestInstance progress = ongoingQuests[questChainId];

            //Get QuestChain's active quest
            Quest currentQuest = questChain.Quests[progress.activeQuestIndex];

            //find objective with the targetid
            int index = currentQuest.Objectives.FindIndex(objective => objective.QuestTarget == objectId);
            if (index >= 0)
            {
                //increment objective progression
                progress.objectiveProgress[index]++;

                //check completion
                if (progress.objectiveProgress[index] >= currentQuest.Objectives[index].RequiredAmount)
                {
                    OnObjectiveComplete?.Invoke(questChain, progress);
                    CheckQuestCompletion(questChainId);
                }
                else
                {
                    OnObjectiveIncrement?.Invoke(questChain, progress);
                }
            }
        }

        private void CheckQuestCompletion(string questId)
        {
            QuestChain questChain = database.GetQuest(questId);
            QuestInstance progress = ongoingQuests[questId];
            Quest currentQuest = questChain.Quests[progress.activeQuestIndex];

            int sum = 0;
            for (int i = 0; i < progress.objectiveProgress.Count; i++)
            {
                if (progress.objectiveProgress[i] >= currentQuest.Objectives[i].RequiredAmount)
                {
                    sum++;
                }
            }

            if (sum >= progress.objectiveProgress.Count)
            {
                //proceed to next quest in current QuestChain
                progress.activeQuestIndex++;

                if (progress.activeQuestIndex >= questChain.Quests.Count)
                {
                    //Complete QuestChain.
                    completedMainQuests ??= new List<QuestRecord>();
                    completedSideQuests ??= new List<QuestRecord>();

                    completedMainQuests.Add(new QuestRecord(questId));
                    ongoingQuests.Remove(questId);

                    OnQuestChainComplete?.Invoke(questChain);
                }
                else
                {
                    //build objective progress container for the next Quest
                    currentQuest = questChain.Quests[progress.activeQuestIndex];
                    progress.objectiveProgress.Clear();
                    foreach (var _ in currentQuest.Objectives)
                        progress.objectiveProgress.Add(0);

                    OnProceedNextQuest?.Invoke(questChain.Quests[progress.activeQuestIndex]);
                }
            }

            BuildTargetList();
        }
        #endregion

        #region QUERY
        public bool IsQuestCompleted(string questId)
        {
            if (completedMainQuests == null) return false;
            return completedMainQuests.Exists(q => q.id == questId);
        }
        public bool IsQuestOngoing(string questId)
        {
            if(ongoingQuests == null) return false;
            return ongoingQuests.ContainsKey(questId);
        }
        public bool IsQuestAvailableToTake(string questId)
        {
            if (IsQuestCompleted(questId)) return false;
            if (IsQuestOngoing(questId)) return false;

            QuestChain questChain = database.GetQuest(questId);

            if (questChain.requiredQuests == null) return true;

            foreach (var required in questChain.requiredQuests)
            {
                if (!IsQuestCompleted(required.questChainId)) return false;
            }

            return true;
        }
        public bool IsQuestObjective(string objectId)
        {
            return allQuestTargets.ContainsKey(objectId);
        }
        public List<string> GetQuestChainssByObjective(string objectId)
        {
            if (!allQuestTargets.ContainsKey(objectId))
                return null;
            return allQuestTargets[objectId];
        }
        public List<Quest> GetQuestsByObjective(string objectId)
        {
            if (!allQuestTargets.ContainsKey(objectId))
                return null;

            List<Quest> quests = new List<Quest>();
            List<QuestChain> questChains = new List<QuestChain>();
            foreach (var questChain in allQuestTargets[objectId])
            {
                QuestChain qc = database.GetQuest(questChain);
                questChains.Add(qc);
            }

            List<QuestInstance> questProgress = new List<QuestInstance>();
            foreach (var questChain in questChains)
            {
                if (ongoingQuests.ContainsKey(questChain.questChainId))
                    questProgress.Add(ongoingQuests[questChain.questChainId]);
            }

            foreach (var progress in questProgress)
            {
                Quest quest = database.GetQuest(progress.questChainId).Quests[progress.activeQuestIndex];
                quests.Add(quest);
            }

            return quests;
        }
        public QuestInstance GetQuest(string questId)
        {
            if(!ongoingQuests.ContainsKey(questId)) 
                return null;
            return ongoingQuests[questId];
        }
        #endregion

        #region DEBUG
        [FoldoutGroup("Helper"), BoxGroup("Helper/Quest")]
        public QuestChain questToReceive;
        [BoxGroup("Helper/Quest"), Button]
        void ReceiveQuest()
        {
            ReceiveNewQuest(questToReceive);
        }
        [BoxGroup("Helper/Objective")]
        public string doObjective;
        [BoxGroup("Helper/Objective"), Button]
        void DoObjectiveTest()
        {
            DoObjective(doObjective);
        }
        // [BoxGroup("Helper/ObjectiveNPC")]
        // public string doQuest, doObjectId;
        // [BoxGroup("Helper/ObjectiveNPC"), Button]
        // void DoObjectiveTest2()
        // {
        //     DoObjective(doQuest, doObjectId);
        // }
        #endregion
    }

    [Serializable]
    public class QuestInstance
    {
        //this class only stores a questchain's progression data
        public QuestInstance() { }
        public QuestInstance(QuestChain quest)
        {
            questChainId = quest.questChainId;
            objectiveProgress = new List<int>();

            foreach (var _ in quest.Quests[0].Objectives)
                objectiveProgress.Add(0);
        }

        public string questChainId;
        public int activeQuestIndex;  //index of active quest in QuestChain
        public List<int> objectiveProgress; //progression of each objective in active quest
    }
    [Serializable]
    public class QuestRecord
    {
        public QuestRecord() { }
        public QuestRecord(string questId)
        {
            id = questId;
            completed = DateTime.Now;
        }
        public string id;
        public DateTime completed;
    }
}