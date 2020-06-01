using System.Collections;
using System.Collections.Generic;
using Unity.XR.OpenVR;
using UnityEngine;

namespace RTS_System.AI
{
    [CreateAssetMenu(fileName = "New DirectedAttack", menuName = "ScriptableObject/RTS/AI/DirectedAttack")]
    public class DirectedAttackAction : AIAction
    {
        [Header("Attack Data")]
        public int AttackDamage = 0;
        public float AttackCooldown = 0;

        [Header("Targeting Data")]
        public MoveToTargetAction TargetAction = null;

        private float currentCooldown = 0;
        private Health targetHealth = null;

        public override void InitialiseAction(AIAgent agent)
        {
            if (TargetAction)
            {
                TargetAction = Instantiate(TargetAction);
                TargetAction.InitialiseAction(agent);
            }
        }

        public override bool HasActionCompleted(AIAgent agent)
        {
            if (targetHealth)
            {
                return TargetAction.HasActionCompleted(agent) || targetHealth == null;
            }

            return targetHealth == null || targetHealth.Equals(null);
        }
        public override float UpdateAction(AIAgent agent)
        {
            if (TargetAction)
            {
                TargetAction.UpdateAction(agent);

                AttemptAttack(agent);
            }

            return 0.0f;
        }

        void AttemptAttack(AIAgent agent)
        {
            if ((Time.time >= currentCooldown) && targetHealth)
            {
                if (TargetAction && TargetAction.MoveAction.HasActionCompleted(agent))
                {
                    targetHealth.TakeDamage(AttackDamage);

                    currentCooldown = Time.time + AttackCooldown;
                }
            }
        }

        public override void CancelAction(AIAgent agent)
        {
            if (TargetAction)
            {
                TargetAction.CancelAction(agent);
            }
        }

        public override void EnterAction(AIAgent agent)
        {
            if (TargetAction)
            {
                TargetAction.EnterAction(agent);
            }
        }
        public override void ExitAction(AIAgent agent)
        {
            if (TargetAction)
            {
                TargetAction.ExitAction(agent);
            }
        }

        public override bool ExecuteAction(AIAgent agent)
        {
            if (TargetAction)
            {
                return TargetAction.ExecuteAction(agent);
            }

            return false;
        }

        public override bool SelectionAction(AIAgent agent)
        {
            if (TargetAction)
            {
                bool validTarget = TargetAction.SelectionAction(agent);

                if (TargetAction.CurrentTarget)
                {
                    targetHealth = TargetAction.CurrentTarget.GetComponentInChildren<Health>();

                    if (targetHealth)
                    {
                        targetHealth.OnDeathEvent.AddListener(() => { targetHealth = null; TargetAction.CurrentTarget = null; });
                    }
                }

                return targetHealth != null && validTarget;
            }

            return false;
        }

        public override bool SetVariables(AIAgent agent, GameObject go, Vector3 vec)
        {
            if (TargetAction)
            {
                bool validTarget = TargetAction.SetVariables(agent, go, vec);

                if (TargetAction.CurrentTarget)
                {
                    targetHealth = TargetAction.CurrentTarget.GetComponentInChildren<Health>();

                    if (targetHealth)
                    {
                        targetHealth.OnDeathEvent.AddListener(() => { targetHealth = null; TargetAction.CurrentTarget = null; });
                    }
                }

                return targetHealth != null && validTarget;
            }

            return false;
        }
    }
}