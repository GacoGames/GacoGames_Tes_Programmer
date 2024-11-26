using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using I2.Loc;

using Sirenix.OdinInspector;

namespace GacoGames.QuestSystem
{
    [System.Serializable]
    public class Objective
    {
        [FoldoutGroup("@EditorDisplayName")]
        public ObjectiveTask task;
        [FoldoutGroup("@EditorDisplayName"), SerializeField]
        private string targetId;
        [FoldoutGroup("@EditorDisplayName")]
        public int requiredAmount = 1;
        [FoldoutGroup("@EditorDisplayName")]
        public string customDescriptionId;
        public string QuestTarget => targetId;

        public int RequiredAmount
        {
            get
            {
                switch (task)
                {
                    case ObjectiveTask.DefeatEnemySpecific:
                        return requiredAmount;
                    case ObjectiveTask.ObtainItem:
                        return requiredAmount;
                    default: return 1;
                }
            }
        }
        public string Description
        {
            get
            {
                if (!string.IsNullOrEmpty(customDescriptionId))
                {
                    return LocalizationManager.GetTranslation($"quest/objective/{customDescriptionId}");
                }
                else
                {
                    //build generic objective
                    string s = $"{TaskDescription(task)}";
                    return s;
                }
            }
        }

        private string TaskDescription(ObjectiveTask t)
        {
            return t.ToString() + " " + targetId;
        }

#if UNITY_EDITOR
        private string EditorDisplayName
        {
            get
            {
                if (!string.IsNullOrEmpty(customDescriptionId))
                {
                    return LocalizationManager.GetTranslation($"quest/objective/{customDescriptionId}");
                }
                else
                {
                    //build generic objective
                    string s = $"{TaskDescription(task)} x{requiredAmount}";
                    return s;
                }
            }
            set { }
        }
#endif
    }

    public enum ObjectiveTask
    {
        DefeatEnemySpecific,
        DefeatEnemyClass,
        DefeatEnemyFamily,
        TalkToNPC,
        QuestTrigger,
        ObtainItem,
    }
}