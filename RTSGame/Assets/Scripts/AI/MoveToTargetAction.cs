using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Action_System
{
    [CreateAssetMenu(fileName = "New MoveToAction", menuName = "ScriptableObject/RTS/AI/MoveToTarget")]
    public class MoveToTargetAction : AIAction
    {
        public MoveToPointAction MoveAction;
        public float MaxFollowDistance = 0;
        public float TargetFollowDistance = 0;
        public Transform CurrentTarget = null;

        public override void InitialiseAction(AIAgent agent)
        {
            if (MoveAction)
            {
                MoveAction = Instantiate(MoveAction);
                MoveAction.InitialiseAction(agent);
            }
        }
        public override bool HasActionCompleted(AIAgent agent)
        {
            return (CurrentTarget && Vector3.Distance(CurrentTarget.transform.position, agent.transform.position) >= MaxFollowDistance);
        }

        public override float UpdateAction(AIAgent agent)
        {
            bool outsideTargetRange = Vector3.Distance(agent.transform.position, CurrentTarget.transform.position) >= TargetFollowDistance;

            if (MoveAction && CurrentTarget && outsideTargetRange)
            { 
                MoveAction.CurrentTargets.Clear();
                MoveAction.CurrentTargets.Add(CurrentTarget.position);

                MoveAction.ExecuteAction(agent);
                MoveAction.UpdateAction(agent);
            }
            
            if (!outsideTargetRange)
            {
                agent.NavAgent.ResetPath();
            }

            return 0.0f;
        }

        public override bool ExecuteAction(AIAgent agent)
        {
            return (MoveAction && MoveAction.ExecuteAction(agent));
        }

        public override float EvaluateAction(AIAgent agent)
        {
            if (!agent || !CurrentTarget) return 0.0f;

            if (MoveAction)
            {
                MoveAction.CurrentTargets.Clear();
                MoveAction.CurrentTargets.Add(CurrentTarget.position);
            }

            float dist = Helper.Distance(agent.transform, CurrentTarget);
            float returnVal = 0;

            return returnVal;
        }

        public override void EnterAction(AIAgent agent)
        {
            if (!agent || !agent.NavAgent) return;

            agent.NavAgent.ResetPath();

            if (MoveAction && CurrentTarget)
            {
                MoveAction.CurrentTargets.Clear();
                MoveAction.CurrentTargets.Add(CurrentTarget.position);
            }
        }
        public override void ExitAction(AIAgent agent)
        {
            if (!agent || !agent.NavAgent) return;

            agent.NavAgent.ResetPath();
            CurrentTarget = null;
        }

        public override void SelectionAction(AIAgent agent)
        {
            RaycastHit rayHit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out rayHit))
            {
                CurrentTarget = rayHit.collider.transform;
            }
        }
    }
}