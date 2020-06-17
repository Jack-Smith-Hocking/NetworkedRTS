using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace ScriptableActions.Character
{
    public class CharacterAction : ScriptableActions.Action
    {
        [Tooltip("Used for exceptions")] public string ActionName;

        [Space]

        [Tooltip("The name of the input in the control map")] public string PrimaryActionName;
        [Tooltip("The name of the secondary input in the control map, this input will need to be triggered in tandem with the primary input")] public string SecondaryActionName;
        [Tooltip("The name of the value in the animator")] public string AnimatorValName;
        [Tooltip("The name of animation events this action will respond to")] public string AnimatorEventName;

        public bool AnimatorCanUpdate { get; set; } = true;
        /// <summary>
        /// Whether or not the CharacterAction is currently being performed
        /// </summary>
        public bool SecondaryPerforming { get; protected set; } = true;
        public bool IsPerforming { get; protected set; } = false;
        public bool EventFired { get; protected set; } = false;

        /// <summary>
        /// The currently bound input action
        /// </summary>
        protected BoundInput primaryBoundInput = new BoundInput();

        /// <summary>
        /// For secondary button mapping
        /// </summary>
        protected BoundInput secondaryBoundInput = new BoundInput();

        protected Animator animator = null;
        protected CharacterManager characterManager = null;

        /// <summary>
        /// Will bind an action to the appropriate InputAction to get input data from
        /// </summary>
        /// <param name="playerInput">The PlayerInput to search for the desired InputAction</param>
        public virtual void InitialiseAction(PlayerInput playerInput, Animator anim, CharacterManager manager = null)
        {
            animator = anim;
            characterManager = manager;

            BindInputs(playerInput);

            EventFired = (AnimatorEventName.Length == 0);
        }

        #region Update
        public override bool ActionUpdate(Transform trans)
        { 
            if (animator != null && AnimatorCanUpdate) UpdateAnimator();
            if (CanUpdate()) return UpdateAction(trans);

            return false;
        }

        protected virtual void UpdateAnimator() { }

        public virtual void AnimationEventFired() { EventFired = true; Perform(currentTransform); }

        protected override bool CanUpdate() { return (EventFired || AnimatorEventName.Length == 0); }
        #endregion

        #region Perform/Cancel
        public override void Perform(Transform trans) 
        {
            currentTransform = trans;
            if (CanPerform()) PerformAction(trans); 
        }
        public override void Cancel() { CancelAction(); }

        protected override void PerformAction(Transform trans) { }
        protected override void CancelAction() { EventFired = AnimatorEventName.Length == 0; }

        protected override bool CanPerform() 
        {
            bool canPerform = SecondaryPerforming;

            return canPerform && CanPerformAction;
        }
        #endregion

        public void UnbindInputs()
        {
            primaryBoundInput.Unbind(primaryBoundInput.InputAction);
            secondaryBoundInput.Unbind(secondaryBoundInput.InputAction);
        }

        public void BindInputs(PlayerInput playerInput)
        {
            // Each action will need to decide what they bind to 'performed' and 'cancelled' individually before this is called
            primaryBoundInput.Bind(playerInput, PrimaryActionName);

            if (SecondaryActionName.Length > 0)
            {
                secondaryBoundInput.PerformedActions += (InputAction.CallbackContext cc) => { SecondaryPerforming = true; };
                secondaryBoundInput.PerformedActions += (InputAction.CallbackContext cc) => { Perform(currentTransform); };

                secondaryBoundInput.CancelledActions += (InputAction.CallbackContext cc) => { SecondaryPerforming = false; };
                secondaryBoundInput.CancelledActions += (InputAction.CallbackContext cc) => { Cancel(); };

                SecondaryPerforming = false;
                secondaryBoundInput.Bind(playerInput, SecondaryActionName);
            }
        }
    }
}

