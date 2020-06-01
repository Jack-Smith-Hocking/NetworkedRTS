using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTS_System.AI
{
    [CreateAssetMenu(fileName = "New PatrolAction", menuName = "ScriptableObject/RTS/AI/Patrol")]
    public class PatrolAction : AIAction
    {
        [Header("Patrol Data")]
        [Tooltip("Action that moves a unit around")] public MoveToPointAction MoveAction = null;
        public List<Vector3> PatrolPath = new List<Vector3>();

        public override void InitialiseAction(AIAgent agent)
        {
            // Instantiate and initialise MoveAction
            if (MoveAction)
            {
                MoveAction = Instantiate(MoveAction);
                MoveAction.InitialiseAction(agent);
            }
        }

        public override float UpdateAction(AIAgent agent)
        {
            if (!agent) return 0.0f;

            if (MoveAction)
            {
                MoveAction.UpdateAction(agent);

                // If the MoveAction has reached its target it will be set to the next point in the path
                if (MoveAction.HasActionCompleted(agent))
                {
                    MoveAction.CurrentTarget = PatrolPath[0];
                    MoveAction.ExecuteAction(agent);

                    PatrolPath.Reverse();
                }
            }

            return 0.0f;
        }

        public override bool ExecuteAction(AIAgent agent)
        {
            return (MoveAction && MoveAction.ExecuteAction(agent));
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

        public override bool SelectionAction(AIAgent agent)
        {
            RaycastHit rayHit;

            // Get the hit point, and the current position and add them to the path
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out rayHit))
            {
                PatrolPath.Clear();

                PatrolPath.Add(rayHit.point);
                PatrolPath.Add(agent.transform.position);

                if (MoveAction)
                {
                    MoveAction.CurrentTarget = PatrolPath[0];
                }

                return true;
            }

            return false;
        }

        public override bool SetVariables(AIAgent agent, GameObject go, Vector3 vec)
        {
            bool valid = false;

            if (agent)
            {
                PatrolPath.Clear();

                PatrolPath.Add(vec);
                PatrolPath.Add(agent.transform.position);

                valid = MoveAction.SetVariables(agent, go, vec);
            }

            return valid;
        }
    }
}

