using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Action_System
{
    [CreateAssetMenu(fileName = "New MoveToAction", menuName = "ScriptableObject/RTS/AI/MoveToPoint")]
    public class MoveToPointAction : AIAction
    {
        public List<Vector3> CurrentTargets = new List<Vector3>();
        public float StoppingAccuracy = 1; 
        public float EvaluationValue = 0;

        public override float UpdateAction(AIAgent agent)
        {
            if (agent.NavAgent && CurrentTargets.Count > 0)
            {
                if (Vector3.Distance(agent.transform.position, CurrentTargets[0]) <= StoppingAccuracy)
                {
                    CurrentTargets.RemoveAt(0);

                    ExecuteAction(agent);
                }
            }

            return 0.0f;
        }

        public override bool HasActionCompleted(AIAgent agent)
        {
            if (!agent) return true;

            if (CurrentTargets.Count == 0 || Vector3.Distance(agent.transform.position, CurrentTargets[0]) <= StoppingAccuracy)
            {
                return true;
            }

            return false;
        }

        public override bool ExecuteAction(AIAgent agent)
        {
            if (!base.ExecuteAction(agent)) return false;

            if (agent.NavAgent && CurrentTargets.Count > 0)
            {
                agent.NavAgent.SetDestination(CurrentTargets[0]);
            }

            return true;
        }

        public override float EvaluateAction(AIAgent agent)
        {
            if (!agent) return 0.0f;

            return EvaluationValue;
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

        public override void SelectionAction(AIAgent agent)
        {
            RaycastHit rayHit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out rayHit))
            {
                CurrentTargets.Clear();
                CurrentTargets.Add(rayHit.point);
            }
        }
    }
}