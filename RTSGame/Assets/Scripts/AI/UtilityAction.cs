using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI_System
{
    public class UtilityAction : ScriptableObject
    {
        [Tooltip("An icon to be displayed for this Action wherever visual representation is necessary")] public Sprite ActionIcon = null;
        [Tooltip("A description for this Action to be displayed whenever one is necessary")] public string ActionDescription;

        public virtual bool ExecuteAction(AIAgent agent)
        {
            return (agent != null);
        }
        public virtual float UpdateAction(AIAgent agent)
        {
            return 0;
        }
    }
}