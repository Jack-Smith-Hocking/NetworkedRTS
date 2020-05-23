﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unit_System
{
    [CreateAssetMenu(fileName = "New PatrolAction", menuName = "ScriptableObject/RTS/AI/Patrol")]
    public class PatrolAction : AIAction
    {
        public MoveToPointAction MoveAction = null;
        public List<Vector3> PatrolPath = new List<Vector3>();

        public override void InitialiseAction(AIAgent agent)
        {
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

        public override void SelectionAction(AIAgent agent)
        {
            RaycastHit rayHit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out rayHit))
            {
                PatrolPath.Clear();

                PatrolPath.Add(rayHit.point);
                PatrolPath.Add(agent.transform.position);

                if (MoveAction)
                {
                    MoveAction.CurrentTarget = PatrolPath[0];
                }
            }
        }
    }
}

