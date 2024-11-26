using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace GacoGames.QuestSystem
{
    [CreateAssetMenu(fileName = "MsQ_MainQuest", menuName = "GacoGames/Quest/Main Quest", order = 1)]
    public class MainQuest : QuestChain
    {
        public override QuestCategory QuestChainType => QuestCategory.MainQuest;
    }
}