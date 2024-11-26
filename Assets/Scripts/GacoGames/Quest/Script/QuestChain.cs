using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using I2.Loc;

namespace GacoGames.QuestSystem
{
    public abstract class QuestChain : ScriptableObject
    {
        public string QuestChainTitle => LocalizationManager.GetTranslation($"quest/{questChainId}/title");
        public string QuestChainSummary => LocalizationManager.GetTranslation($"quest/{questChainId}/summary");
        public List<Quest> Quests => quests;

        public Quest GetQuestInfo(int questIndex)
        {
            return quests[questIndex];
        }

        [PropertyOrder(-3)]
        public string questChainId;

        [EnumToggleButtons, PropertyOrder(-2), HideLabel, SerializeField]
        private QuestChainTab tab;

        [ShowIf("@tab == QuestChainTab.Setup"), SerializeField]
        private List<Quest> quests;


        [ShowIf("@tab == QuestChainTab.Setup")]
        public List<QuestChain> requiredQuests;

        [ShowIf("@tab == QuestChainTab.Setup")]
        public bool receiveRewardSilently;


#if UNITY_EDITOR
        //Editor View
        [ShowInInspector, DisplayAsString(EnableRichText = true, Overflow = false), HideLabel]
        [BoxGroup("Quest Chain Summary"), ShowIf("@tab == QuestChainTab.Info")]
        private string EditorQuestTitle
        {
            get => $"<color=#33c107><size=20><b>{QuestChainTitle}</b></size></color>";
            set { }
        }

        [ShowInInspector, DisplayAsString(EnableRichText = true, Overflow = false), HideLabel]
        [BoxGroup("Quest Chain Summary"), ShowIf("@tab == QuestChainTab.Info")]
        [PropertySpace(SpaceAfter = 10)]
        private string EditorQuestSummary
        {
            get => $"{QuestChainSummary}";
            set { }
        }
        
        private string QuestDetails(int index)
        {
            if (quests.Count > index)
            {
                string summary = $"<i>{quests[index].summary}</i>";
                string objective = string.Empty;

                for (int i = 0; i < quests[index].Objectives.Count; i++)
                {
                    objective += $"  - {quests[index].Objectives[i].Description}";
                    if (i < quests[index].Objectives.Count - 1) objective += "\n";
                }

                return $"{summary}\n<color=#dcdcaa>{objective}</color>";
            }
            else return "";
        }
        #region Quest Viewer
        private bool Available(int index)
        { 
            return tab == QuestChainTab.Info && quests.Count > index;
        }
        [ShowInInspector, DisplayAsString(EnableRichText = true, Overflow = false), HideLabel]
        [BoxGroup("Quest 0"), ShowIf("@Available(0)")]
        private string EditorQuestInfo0 { get => QuestDetails(0); set { } }
        [ShowInInspector, DisplayAsString(EnableRichText = true, Overflow = false), HideLabel]
        [BoxGroup("Quest 1"), ShowIf("@Available(1)")]
        private string EditorQuestInfo1 { get => QuestDetails(1); set { } }
        [ShowInInspector, DisplayAsString(EnableRichText = true, Overflow = false), HideLabel]
        [BoxGroup("Quest 2"), ShowIf("@Available(2)")]
        private string EditorQuestInfo2 { get => QuestDetails(2); set { } }
        [ShowInInspector, DisplayAsString(EnableRichText = true, Overflow = false), HideLabel]
        [BoxGroup("Quest 3"), ShowIf("@Available(3)")]
        private string EditorQuestInfo3 { get => QuestDetails(3); set { } }
        [ShowInInspector, DisplayAsString(EnableRichText = true, Overflow = false), HideLabel]
        [BoxGroup("Quest 4"), ShowIf("@Available(4)")]
        private string EditorQuestInfo4 { get => QuestDetails(4); set { } }
        [ShowInInspector, DisplayAsString(EnableRichText = true, Overflow = false), HideLabel]
        [BoxGroup("Quest 5"), ShowIf("@Available(5)")]
        private string EditorQuestInfo5 { get => QuestDetails(5); set { } }
        [ShowInInspector, DisplayAsString(EnableRichText = true, Overflow = false), HideLabel]
        [BoxGroup("Quest 6"), ShowIf("@Available(6)")]
        private string EditorQuestInfo6 { get => QuestDetails(6); set { } }
        [ShowInInspector, DisplayAsString(EnableRichText = true, Overflow = false), HideLabel]
        [BoxGroup("Quest 7"), ShowIf("@Available(7)")]
        private string EditorQuestInfo7 { get => QuestDetails(7); set { } }
        [ShowInInspector, DisplayAsString(EnableRichText = true, Overflow = false), HideLabel]
        [BoxGroup("Quest 8"), ShowIf("@Available(8)")]
        private string EditorQuestInfo8 { get => QuestDetails(8); set { } }
        [ShowInInspector, DisplayAsString(EnableRichText = true, Overflow = false), HideLabel]
        [BoxGroup("Quest 9"), ShowIf("@Available(9)")]
        private string EditorQuestInfo9 { get => QuestDetails(9); set { } }
        #endregion
#endif
    }

    public enum QuestChainTab
    {
        Info,
        Setup
    }
}