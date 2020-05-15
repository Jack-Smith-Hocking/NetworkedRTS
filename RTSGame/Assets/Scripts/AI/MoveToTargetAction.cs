using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AI_System
{
    [CreateAssetMenu(fileName = "New MoveToAction", menuName = "ScriptableObject/RTS/AI/MoveToTarget")]
    public class MoveToTargetAction : AIAction
    {
        public Transform CurrentTarget = null;
        public float DetectionDistance = 5;

        private MoveToPoint moveAction;

        private void OnEnable()
        {
            moveAction = MoveToPoint.CreateInstance(typeof(MoveToPoint)) as MoveToPoint;
        }

        public override void InitialiseAction(AIAgent agent)
        {
            if (moveAction)
            {
                moveAction = Instantiate(moveAction);
                moveAction.InitialiseAction(agent);
            }
        }

        public override bool ExecuteAction(AIAgent agent)
        {
            return (moveAction && moveAction.ExecuteAction(agent));
        }

        public override float EvaluateAction(AIAgent agent)
        {
            if (!agent || !CurrentTarget) return 0.0f;

            if (moveAction)
            {
                moveAction.CurrentTargets.Clear();
                moveAction.CurrentTargets.Add(CurrentTarget.position);
            }

            float dist = Helper.Distance(agent.transform, CurrentTarget);
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

            if (moveAction && CurrentTarget)
            {
                moveAction.CurrentTargets.Clear();
                moveAction.CurrentTargets.Add(CurrentTarget.position);
            }
        }
        public override void ExitAction(AIAgent agent)
        {
            if (!agent || !agent.NavAgent) return;

            agent.NavAgent.ResetPath();
            CurrentTarget = null;
        }
    }
}