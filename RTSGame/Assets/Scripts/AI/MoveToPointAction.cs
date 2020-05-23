﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Unit_System
{
    [CreateAssetMenu(fileName = "New MoveToAction", menuName = "ScriptableObject/RTS/AI/MoveToPoint")]
    public class MoveToPointAction : AIAction
    {
        public Vector3 CurrentTarget;
        public float StoppingAccuracy = 1;
        public float EvaluationValue = 0;

        private Vector3 currentPos;

        public override float UpdateAction(AIAgent agent)
        {
            return 0.0f;
        }

        public override bool HasActionCompleted(AIAgent agent)
        {
            if (!agent) return true;

            if (Vector3.Distance(agent.transform.position, CurrentTarget) <= StoppingAccuracy)
            {
                return true;
            }

            return false;
        }

        public override bool ExecuteAction(AIAgent agent)
        {
            if (!base.ExecuteAction(agent)) return false;

            if (agent.NavAgent)
            {
                agent.NavAgent.SetDestination(CurrentTarget);
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
                CurrentTarget = rayHit.point;
            }
        }
    }
}