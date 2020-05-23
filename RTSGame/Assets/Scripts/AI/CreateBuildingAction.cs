using RTS_System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

namespace Unit_System
{
    [CreateAssetMenu(fileName = "New CreateBuldingAction", menuName = "ScriptableObject/RTS/AI/CreateBuilding")]
    public class CreateBuildingAction : AIAction
    {
        public MoveToPointAction MoveAction = null;
        public GameObject BuildingPrefab = null;
        public ResourceConditional BuildingCost = null;
        public float BuildTime = 0;

        private bool attemptBuild = false;
        private float currentBuildTime = 0;
        private GameObject building = null;

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
            return (building != null);
        }

        public override float UpdateAction(AIAgent agent)
        {
            if (MoveAction)
            {
                if (MoveAction.HasActionCompleted(agent) && !attemptBuild)
                {
                    currentBuildTime = Time.time + BuildTime;

                    attemptBuild = true;

                    if (BuildingCost)
                    {
                        attemptBuild = BuildingCost.EvaluateConditional();
                    }

                    MoveAction.ExitAction(agent);
                }

                if (BuildingPrefab && Time.time >= currentBuildTime && attemptBuild)
                {
                    building = Instantiate(BuildingPrefab, MoveAction.CurrentTarget, Quaternion.identity);

                    if (BuildingCost)
                    {
                        BuildingCost.AddResources();
                    }
                }

                MoveAction.UpdateAction(agent);
            }

            return 0.0f;
        }

        public override bool ExecuteAction(AIAgent agent)
        {
            if (MoveAction)
            {
                return MoveAction.ExecuteAction(agent);
            }

            return false;
        }

        public override void EnterAction(AIAgent agent)
        {
            if (!agent || !agent.NavAgent) return;

            if (MoveAction)
            {
                MoveAction.EnterAction(agent);
            }

            agent.NavAgent.ResetPath();
        }
        public override void ExitAction(AIAgent agent)
        {
            if (!agent || !agent.NavAgent) return;

            if (MoveAction)
            {
                MoveAction.ExitAction(agent);
            }

            agent.NavAgent.ResetPath();
        }

        public override void SelectionAction(AIAgent agent)
        {
            if (MoveAction)
            {
                MoveAction.SelectionAction(agent);
            }
        }
    }
}