using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace GacoGames.QuestSystem
{
    [System.Serializable]
    public class Quest
    {
        [TextArea]
        public string summary;
        public List<Objective> Objectives => _objectives;

        [SerializeField]
        private List<Objective> _objectives;
    }
}