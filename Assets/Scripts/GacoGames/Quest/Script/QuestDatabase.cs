using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GacoGames.QuestSystem
{
    [CreateAssetMenu(fileName = "Quest_DB", menuName = "GacoGames/Quest/Database", order = 1)]
    [ExecuteInEditMode]
    [HideMonoScript]
    public class QuestDatabase : SerializedScriptableObject
    {
        [EnumToggleButtons, HideLabel, SerializeField]
        private QuestDbSelection _selection;
        [InfoBox("To generate QuestChain Data, go to 'Settings' Tab and click 'Rebuild'. All Quests under the defined Path will be added automatically.")]
        [ShowIf("@_selection == QuestDbSelection.Database"), SerializeField]
        [ListDrawerSettings(HideRemoveButton = true, HideAddButton = true, DefaultExpandedState = true)]
        private List<QuestChain> questChainData;
        private Dictionary<string, QuestChain> allQuests = new Dictionary<string, QuestChain>();

        public void Init()
        {
            allQuests.Clear();
            foreach (QuestChain quest in questChainData)
            {
                allQuests.Add(quest.questChainId, quest);
            }
        }
        public QuestChain GetQuest(string questId)
        {
            if (!allQuests.ContainsKey(questId))
            {
                Debug.LogWarning("QUEST NOT FOUND IN DATABASE. PLEASE DOUBLE CHECK!!");
                return null;
            }

            return allQuests[questId];
        }


        //===== EDITOR STUFF =====//
        private enum QuestDbSelection
        {
            Database,
            Settings
        }
        [ShowIf("@_selection == QuestDbSelection.Settings"), SerializeField]
        private string assetPath;
#if UNITY_EDITOR
        [Button, ShowIf("@_selection == QuestDbSelection.Settings")]
        private void RebuildAllQuests()
        {
            questChainData = new List<QuestChain>();
            string[] guids = AssetDatabase.FindAssets("t:QuestChain", new[] { assetPath });
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                QuestChain item = AssetDatabase.LoadAssetAtPath<QuestChain>(assetPath);
                if (item != null)
                {
                    questChainData.Add(item);
                }
            }
            _selection = QuestDbSelection.Database;
        }
#endif
    }
}