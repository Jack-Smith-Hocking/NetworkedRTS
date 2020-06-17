using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ScriptableActions.Character
{
    [CreateAssetMenu(fileName = "New WalkAction", menuName = "ScriptableObject/Actions/Character/MoveAction/Walk")]
    public class WalkAction : MoveAction
    {
        [Tooltip("The max speed that this action can move at")] public float MaxSpeed = 0;

        [Space]

        [Tooltip("How quickly this action will reach its max speed")] public float Acceleration = 0;
        [Tooltip("X-Axis represents the ratio of current speed compared to max speed (currentSpeed / MaxSpeed), Y-Axis represents the amount of acceleration to be applied, essentially: (CurrentSpeed + (Acceleration * yVal(CurrentSpeed/MaxSpeed))")] public AnimationCurve AccelerationCurve;

        [Tooltip("How quickly this action will reach no speed")] public float Deceleration = 0;
        [Tooltip("X-Axis represents the ratio of current speed compared to no speed (1 - (currentSpeed / MaxSpeed)), Y-Axis represents the amount of deceleration to be applied, essentially: (CurrentSpeed + (Deceleration * yVal(1 - (CurrentSpeed/MaxSpeed))")] public AnimationCurve DecelerationCurve;

        public override void InitialiseAction(PlayerInput playerInput, Animator anim, CharacterManager abCharacterManager)
        {
            primaryBoundInput.PerformedActions += (InputAction.CallbackContext cc) => { Perform(currentTransform); };
            primaryBoundInput.CancelledActions += (InputAction.CallbackContext cc) => { Cancel(); };

            base.InitialiseAction(playerInput, anim, abCharacterManager);
        }

        protected override bool UpdateAction(Transform trans)
        {
            float xVal = IsPerforming ? (MoveForce.sqrMagnitude / (MaxSpeed * MaxSpeed)) : 1 - (MoveForce.sqrMagnitude / (MaxSpeed * MaxSpeed));
            float speedMultiplier = (IsPerforming ? Mathf.Clamp01(AccelerationCurve.Evaluate(xVal)) * Acceleration : Mathf.Clamp01(DecelerationCurve.Evaluate(xVal)) * Deceleration);

            // Lerp towards max speed depending on acceleration/deceleration
            MoveForce = Vector3.Lerp(MoveForce, MoveDirection * MaxSpeed, speedMultiplier * Time.deltaTime);

            return IsPerforming;
        }

        protected override void UpdateAnimator()
        {
            float speed = MoveForce.magnitude;
            animator.SetFloat(AnimatorValName, speed);
        }

        protected override void PerformAction(Transform trans)
        {
            IsPerforming = true;
            UpdateAnimator();

            if (EventFired)
            {
                MoveDirection = new Vector3(primaryBoundInput.CurrentVec2Val.x, 0, primaryBoundInput.CurrentVec2Val.y);
            }
        }
    }
}

