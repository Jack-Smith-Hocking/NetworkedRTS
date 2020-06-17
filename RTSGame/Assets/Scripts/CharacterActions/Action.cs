using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptableActions
{
    public class Action : ScriptableObject
    {
        [Tooltip("An icon to be displayed for this Action wherever visual representation is necessary")] public Sprite Icon = null;
        [Tooltip("A description for this Action to be displayed whenever one is necessary")] public string Description;

        public bool CanPerformAction { get; set; } = true;

        [Space]

        protected Transform currentTransform = null;

        #region Update
        /// <summary>
        /// External call to update the Action
        /// </summary>
        /// <param name="trans"></param>
        /// <returns></returns>
        public virtual bool ActionUpdate(Transform trans)
        {
            if (CanUpdate()) return UpdateAction(trans);

            return false;
        }
        /// <summary>
        /// An interior update managed by the action, is called by ActionUpdate which is externally called
        /// </summary>
        /// <param name="trans"></param>
        /// <returns></returns>
        protected virtual bool UpdateAction(Transform trans) { return true; }
        
        /// <summary>
        /// Used for evaluating if the Action can update, is checked in ActionUpdate
        /// </summary>
        /// <returns></returns>
        protected virtual bool CanUpdate() { return true; }
        #endregion

        #region Perform/Cancel
        /// <summary>
        /// External call to perform the Action, is generally an input callback
        /// </summary>
        /// <param name="trans"></param>
        public virtual void Perform(Transform trans) { if (CanPerform()) PerformAction(trans); }
        /// <summary>
        /// Cancels the progress of the current Action appropriately 
        /// </summary>
        public virtual void Cancel() { CancelAction(); }

        /// <summary>
        /// Internal Perform, is where the logic of the perform is executed
        /// </summary>
        /// <param name="trans"></param>
        protected virtual void PerformAction(Transform trans) { }

        /// <summary>
        /// Internal Cancel, is where the logic of the cancel is executed 
        /// </summary>
        protected virtual void CancelAction() { }

        /// <summary>
        /// Evaluates whether or not the Action can be performed, is executed in Perform
        /// </summary>
        /// <returns></returns>
        protected virtual bool CanPerform() { return CanPerformAction; }
        #endregion
    }
}
