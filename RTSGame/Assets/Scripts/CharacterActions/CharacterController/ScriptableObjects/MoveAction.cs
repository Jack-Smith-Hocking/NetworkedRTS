using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptableActions.Character
{
    public class MoveAction : CharacterAction
    {
        /// <summary>
        /// The direction of movement, multiplied by the amount of force this frame (based on acceleration/deceleration)
        /// </summary>
        public Vector3 MoveForce { get; protected set; } = new Vector3(0, 0, 0);
        /// <summary>
        /// Direction being moved this frame
        /// </summary>
        public Vector3 MoveDirection { get; protected set; } = new Vector3(0, 0, 0);

        public bool TransformDirection { get; protected set; } = true;

        protected override void CancelAction()
        {
            MoveDirection = Vector3.zero;
            IsPerforming = false;

            base.CancelAction();
        }
    }
}
