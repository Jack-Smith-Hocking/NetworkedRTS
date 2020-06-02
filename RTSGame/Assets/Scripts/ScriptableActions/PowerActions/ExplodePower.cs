using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ScriptableActions.Character
{
    [CreateAssetMenu(fileName = "New ExplodePower", menuName = "ScriptableObject/Actions/Powers/Explode")]
    public class ExplodePower : PowerAction
    {
        [Space]
        [Tooltip("The FX to use when the power is performed")] public ScriptableActions.FXAction ExplodeFX = null;
        [Tooltip("The layers to ignore the explosion")] public LayerMask IgnoreLayers;
        [Tooltip("Amount of damage done to each Health in the radius")] public float ExplodeDamage = 1;
        [Tooltip("Radius outwards from the position of the GameObject to damage")] public float ExplodeRadius = 5;

        public override void InitialiseAction(PlayerInput playerInput, Animator anim, CharacterManager manager = null)
        {
            if (ExplodeFX) ExplodeFX = Instantiate(ExplodeFX);

            primaryBoundInput.PerformedActions += (InputAction.CallbackContext cc1) => { Perform(null); };
            base.InitialiseAction(playerInput, anim, manager);
        }

        public override void Perform(Transform trans)
        {
            if (!CanPerform()) return;

            if (!trans)
            {
                if (characterManager)
                {
                    trans = characterManager.transform;
                }
                else return;
            }

            currentTransform = trans;

            IsPerforming = true;
            UpdateAnimator();

            // Check if it is waiting for an animation event
            if (EventFired)
            {
                EventFired = false;

                if (ExplodeFX)
                {
                    ExplodeFX.DeleteFX();
                    ExplodeFX.Perform(trans);
                }

                // Check all colliders in radius
                // NOTE - Seeing as the Action will be instantiated when created the Collider array could be made a member variable
                Collider[] cols = Physics.OverlapSphere(trans.position, ExplodeRadius, ~IgnoreLayers);
                Health health = null;
                RaycastHit rayHit;

                // Deal damage to all colliders that have Health scripts attached
                foreach (Collider col in cols)
                {
                    if (col.transform == trans)
                    {
                        continue;
                    }
                    health = col.GetComponent<Health>();

                    if (health)
                    {
                        Physics.Raycast(new Ray(trans.position, col.transform.position - trans.position), out rayHit, col.gameObject.layer);
                        health.RpcTakeDamage(ExplodeDamage, rayHit.point, trans.gameObject);
                    }
                }
            }
        }

        protected override void UpdateAnimator()
        {
            if (animator)
            {
                animator.SetBool(AnimatorValName, IsPerforming);

                if (IsPerforming) IsPerforming = false;
            }
        }
    }
}

