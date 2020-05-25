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
        [Tooltip("The action that will control moving, without this the unit won't move")] public MoveToPointAction MoveAction = null;
        [Tooltip("The building to attempt to create")] public GameObject BuildingPrefab = null;
        [Tooltip("Cost of the building")] public ResourceConditional BuildingCost = null;
        [Tooltip("How long it will take to build")] public float BuildTime = 0;

        private bool canAfford = true;
        private bool attemptBuild = false;
        private float currentBuildTime = 0;
        private GameObject building = null;

        public override void InitialiseAction(AIAgent agent)
        {
            // Initialise and instantiate the MoveAction
            if (MoveAction)
            {
                MoveAction = Instantiate(MoveAction);
                MoveAction.InitialiseAction(agent);
            }
        }
        public override bool HasActionCompleted(AIAgent agent)
        {
            return (building != null) && canAfford; // If the building was built, and the building was affordable this action is done
            // Returns false if it wasn't affordable
        }

        public override float UpdateAction(AIAgent agent)
        {
            if (MoveAction)
            {
                // Check if has reached target
                if (MoveAction.HasActionCompleted(agent) && !attemptBuild)
                {
                    // If the target point is reached, set a timer that will wait for the building to build
                    currentBuildTime = Time.time + BuildTime;

                    attemptBuild = true;

                    MoveAction.ExitAction(agent);
                }

                // If the building is ready to build, then create one at the target point
                if (BuildingPrefab && Time.time >= currentBuildTime && attemptBuild)
                {
                    building = Instantiate(BuildingPrefab, MoveAction.CurrentTarget, Quaternion.identity);

                    attemptBuild = false;
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

            CancelAction(agent);
        }

        public override void CancelAction(AIAgent agent)
        {
            if (!agent) return;

            // If the action was cancelled before the building was built then resources will be refunded 
            if (canAfford && BuildingCost && building == null)
            {
                Helper.LoopList_ForEach<Mod_ResourceCost>(BuildingCost.ResourceCosts, (Mod_ResourceCost rc) =>
                {
                    Mod_ResourceManager.Instance.AddResource(rc.ResourceType, rc.TrueResourceCost * -1);
                });
            }
        }

        public override void SelectionAction(AIAgent agent)
        {
            // Check if the building is affordable
            if (BuildingCost)
            {
                canAfford = BuildingCost.EvaluateConditional();

                if (canAfford)
                {
                    Mod_ResourceManager.Instance.AddResources(BuildingCost.ResourceCosts);
                }
            }

            if (MoveAction)
            {
                MoveAction.SelectionAction(agent);
            }
        }
    }
}