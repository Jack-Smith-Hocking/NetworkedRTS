using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AI_System
{
    [CreateAssetMenu(fileName = "New MoveToAction", menuName = "ScriptableObject/RTS/AI/MoveTo")]
    public class MoveToTargetAction : AIAction
    {
        public float DetectionDistance = 5;

        public override bool ExecuteAction(AIAgent agent)
        {
            if (!base.ExecuteAction(agent)) return false;

            if (agent.NavAgent && agent.CurrentTarget)
            {
                agent.NavAgent.SetDestination(agent.CurrentTarget.position);
            }

            return true;
        }

        public override float EvaluateAction(AIAgent agent)
        {
            if (!agent || !agent.CurrentTarget) return 0.0f;

            float dist = Helper.Distance(agent.transform, agent.CurrentTarget);
            float returnVal = 0;

            if (dist > DetectionDistance)
            {
                returnVal = DetectionDistance / dist;
            }
            else if (dist < DetectionDistance)
            {
                returnVal = 1 - (dist / DetectionDistance);
            }

            return returnVal;
        }

        public override void EnterAction(AIAgent agent)
        {
            if (!agent || !agent.NavAgent) return;

            agent.NavAgent.ResetPath();
        }
        public override void ExitAction(AIAgent agent)
        {
            if (!agent || !agent.NavAgent) return;

            agent.NavAgent.ResetPath();
        }
    }
}