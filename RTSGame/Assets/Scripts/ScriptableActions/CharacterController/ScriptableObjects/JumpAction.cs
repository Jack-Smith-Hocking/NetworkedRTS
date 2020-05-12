using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ScriptableActions.Character
{
    [CreateAssetMenu(fileName = "New JumpAction", menuName = "ScriptableObject/Actions/Character/MoveAction/Jump")]
    public class JumpAction : MoveAction
    {
        [Tooltip("Determines how high the character will jump per jump, eg. at jump one you can jump 0.5 * jumpHeight and at jump 2 you can jump 1 * JumpHeight. Each jump is equal to 0.1 of the X-Axis")] public AnimationCurve JumpHeightCurve;
        [Tooltip("How high the character can jump")] public float JumpHeight = 0;
        [Tooltip("The amount of time before the player can jump again")] public float JumpDelay = 0;
        public int MaxJumpCount = 1;

        [Space]
        public bool needGrounded = true;

        protected int currentJumpCount = 0;
        protected float currentJumpDelay = 0;

        public override void InitialiseAction(PlayerInput playerInput, Animator anim, CharacterManager abCharacterManager)
        {
            primaryBoundInput.PerformedActions += (InputAction.CallbackContext cc) => { Perform(currentTransform); };

            base.InitialiseAction(playerInput, anim, abCharacterManager);
        }

        protected override bool UpdateAction(Transform trans)
        {
            if (IsPerforming)
            {
                Cancel();
                return true;
            }

            if (characterManager.IsGrounded && currentJumpDelay <= 0.05f)
            {
                Cancel();
                return false;
            }

            return false;
        }

        protected override void UpdateAnimator()
        {
            animator.SetBool(AnimatorValName, IsPerforming);
        }

        protected override void PerformAction(Transform trans)
        {
            IsPerforming = true;
            UpdateAnimator();

            if (EventFired)
            {
                // Calculate JumpHeight based off of the gravity this Action's owner is using
                currentJumpDelay = 0;
                float jump = JumpHeightCurve.Evaluate((float)currentJumpCount / 10.0f) * JumpHeight;
                MoveForce = new Vector3(0, Mathf.Sqrt(Mathf.Abs(-2 * (characterManager.Gravity.y) * jump)), 0);
                currentJumpCount++;

                Debug.LogError(jump);
            }
        }

        protected override void CancelAction()
        {
            MoveForce = Vector3.zero;
            currentJumpDelay = Time.time + JumpDelay;

            base.CancelAction();
        }

        protected override bool CanPerform()
        {
            bool canPerform = base.CanPerform();

            if (characterManager.IsGrounded)
            {
                currentJumpCount = 0;
            }

            if (currentJumpCount > 0)
            {
                canPerform = canPerform && (currentJumpCount < MaxJumpCount);
            }
            else
            {
                canPerform = canPerform && (characterManager.IsGrounded || !needGrounded) && (Time.time >= currentJumpDelay);
            }

            return canPerform;
        }
    }
}

