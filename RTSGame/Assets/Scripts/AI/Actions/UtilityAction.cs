using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTS_System.AI
{
    public abstract class UtilityAction : ScriptableObject
    {
        [Header("Action Data")]
        [Tooltip("An icon to be displayed for this Action wherever visual representation is necessary")] public Sprite ActionIcon = null;
        [TextArea] [Tooltip("A description for this Action to be displayed whenever one is necessary")] public string ActionDescription;

        /// <summary>
        /// Sets up any necessary data 
        /// </summary>
        /// <param name="agent">The AIAgent to affect</param>
        public virtual void InitialiseAction(AIAgent agent)
        {

        }

        /// <summary>
        /// Updates the action
        /// </summary>
        /// <param name="agent">The AIAgent to affect</param>
        /// <returns></returns>
        public virtual float UpdateAction(AIAgent agent)
        {
            return 0;
        }

        /// <summary>
        /// Determines whether an action has been completed
        /// </summary>
        /// <param name="agent">The AIAgent to affect</param>
        /// <returns>Action has completed</returns>
        public virtual bool HasActionCompleted(AIAgent agent)
        {
            return false;
        }
        /// <summary>
        /// Determines how valuable it is to perform this action at this time
        /// </summary>
        /// <param name="agent">The AIAgent to affect</param>
        /// <returns>The 'value' of this action</returns>
        public virtual float EvaluateAction(AIAgent agent)
        {
            return 0.0f;
        }
    }
}