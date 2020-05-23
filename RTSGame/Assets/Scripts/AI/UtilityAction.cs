using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unit_System
{
    public abstract class UtilityAction : ScriptableObject
    {
        [Tooltip("An icon to be displayed for this Action wherever visual representation is necessary")] public Sprite ActionIcon = null;
        [Tooltip("A description for this Action to be displayed whenever one is necessary")] public string ActionDescription;

        public virtual void InitialiseAction(AIAgent agent)
        {

        }

        public virtual float UpdateAction(AIAgent agent)
        {
            return 0;
        }

        public virtual bool HasActionCompleted(AIAgent agent)
        {
            return false;
        }
        public virtual float EvaluateAction(AIAgent agent)
        {
            return 0.0f;
        }
    }
}