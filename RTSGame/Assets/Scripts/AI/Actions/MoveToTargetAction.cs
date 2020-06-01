using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace RTS_System.AI
{
    [CreateAssetMenu(fileName = "New MoveToAction", menuName = "ScriptableObject/RTS/AI/MoveToTarget")]
    public class MoveToTargetAction : AIAction
    {
        [Header("Movement Data")]
        public MoveToPointAction MoveAction;
        public float MaxFollowDistance = 0;
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
            if (!CurrentTarget || !MoveAction) return 0.0f;

            bool outsideTargetRange = Vector3.Distance(agent.transform.position, CurrentTarget.transform.position) >= MoveAction.StoppingAccuracy;

            if (MoveAction && CurrentTarget && outsideTargetRange)
            { 
                MoveAction.CurrentTarget = CurrentTarget.position;

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
                MoveAction.CurrentTarget = CurrentTarget.position;
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
                MoveAction.CurrentTarget = CurrentTarget.position;
            }
        }
        public override void ExitAction(AIAgent agent)
        {
            if (!agent || !agent.NavAgent) return;

            agent.NavAgent.ResetPath();
            CurrentTarget = null;
        }

        public override bool SelectionAction(AIAgent agent)
        {
            RaycastHit rayHit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out rayHit))
            {
                CurrentTarget = rayHit.collider.transform;
                return MoveAction.SelectionAction(agent);
            }

            return false;
        }

        public override bool SetVariables(AIAgent agent, GameObject go, Vector3 vec)
        {
            bool valid = false;
            if (MoveAction)
            {
                valid = MoveAction.SetVariables(agent, go, vec);

                if (valid)
                {
                    CurrentTarget = go.transform;
                }
            }

            return valid;
        }
    }
}