using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace GacoGames.QuestSystem
{
    [CreateAssetMenu(fileName = "SQ_SideQuest", menuName = "GacoGames/Quest/Side Quest", order = 1)]
    public class SideQuest : QuestChain
    {
        public override QuestCategory QuestChainType => QuestCategory.SideQuest;

        // [Button]
        // void SaveAsJson()
        // {
        //     string json = JsonConvert.SerializeObject(this, Formatting.Indented);
        //     GUIUtility.systemCopyBuffer = json;
        // }
    }
}