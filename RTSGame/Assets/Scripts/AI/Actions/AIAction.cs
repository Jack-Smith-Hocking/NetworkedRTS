using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unit_System
{
    public abstract class AIAction : UtilityAction
    {
        public override float UpdateAction(AIAgent agent)
        {
            return EvaluateAction(agent);
        }

        /// <summary>
        /// When the action is actually performed 
        /// </summary>
        /// <param name="agent">The AIAgent to affect</param>
        /// <returns></returns>
        public virtual bool ExecuteAction(AIAgent agent)
        {
            return (agent != null);
        }
        /// <summary>
        /// When an action is entered for the first time
        /// </summary>
        /// <param name="agent">The AIAgent to affect</param>
        public virtual void EnterAction(AIAgent agent)
        {

        }

        /// <summary>
        /// Call this when an active action is cancelled
        /// </summary>
        /// <param name="agent">The AIAgent to affect</param>
        public virtual void ExitAction(AIAgent agent)
        {

        }

        /// <summary>
        /// Call this if the current action is cancelled, and for every action in an AIAction list if they are all cancelled
        /// </summary>
        /// <param name="agent">The AIAgent to affect</param>
        public virtual void CancelAction(AIAgent agent)
        {

        }

        /// <summary>
        /// When this action is 'Selected', used in the Selector script
        /// </summary>
        /// <param name="agent">The AIAgent to affect</param>
        public virtual bool SelectionAction(AIAgent agent)
        {
            return false;
        }
    }
}