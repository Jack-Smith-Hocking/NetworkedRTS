using RTS_System.Resource;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTS_System.AI
{
    [CreateAssetMenu(fileName = "New CreateBuldingAction", menuName = "ScriptableObject/RTS/AI/CreateBuilding")]
    public class CreateBuildingAction : AIAction
    {
        [Header("Building Data")]
        [Tooltip("The action that will control moving, without this the unit won't move")] public MoveToPointAction MoveAction = null;
        [Space]
        [Tooltip("The building to attempt to create")] public GameObject BuildingPrefab = null;
        [Tooltip("The prefab to show that a building is meant to be built here")] public GameObject DisplayBuilding = null;
        [Space]
        [Tooltip("Cost of the building")] public ResourceConditional BuildingCost = null;
        [Tooltip("How long it will take to build")] public float BuildTime = 0;

        /// <summary>
        /// Whether this building can be afforded
        /// </summary>
        public bool CanAfford { get; private set; } = false;
        public bool AttemptBuild { get; private set; } = false;

        private float currentBuildTime = 0;
        private GameObject building = null;
        private GameObject tempBuilding = null;

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
            return ((building != null) && CanAfford) || CanAfford == false; // If the building was built, and the building was affordable this action is done
            // Returns false if it wasn't affordable
        }

        public void StartTimer(AIAgent agent)
        {
            // If the target point is reached, set a timer that will wait for the building to build
            currentBuildTime = Time.time + BuildTime;

            AttemptBuild = CanAfford;

            if (MoveAction)
            {
                MoveAction.ExitAction(agent);
            }
        }
        public void Build()
        {
            // If the building is ready to build, then create one at the target point
            if (BuildingPrefab && Time.time >= currentBuildTime && AttemptBuild)
            {
                if (tempBuilding)
                {
                    Destroy(tempBuilding);
                }

                building = Instantiate(BuildingPrefab, MoveAction.CurrentTarget, Quaternion.identity);

                AttemptBuild = false;
            }
        }

        public override float UpdateAction(AIAgent agent)
        {
            if (MoveAction)
            {
                // Check if has reached target
                if (MoveAction.HasActionCompleted(agent) && !AttemptBuild)
                {
                    StartTimer(agent);
                }

                Build();

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

            if (agent.NavAgent.enabled)
            {
                agent.NavAgent.ResetPath();
            }
        }
        public override void ExitAction(AIAgent agent)
        {
            CancelAction(agent);

            if (!agent || !agent.NavAgent) return;

            if (MoveAction)
            {
                MoveAction.ExitAction(agent);
            }

            if (agent.NavAgent.enabled)
            {
                agent.NavAgent.ResetPath();
            }
        }

        public override void CancelAction(AIAgent agent)
        {
            if (!agent) return;

            // If the action was cancelled before the building was built then resources will be refunded 
            if (CanAfford && BuildingCost && building == null)
            {
                Helper.LoopList_ForEach<Mod_ResourceValue>(BuildingCost.ResourceCosts, (Mod_ResourceValue rc) =>
                {
                    Mod_ResourceManager.Instance.AddResource(rc.ResourceType, rc.TrueValue * -1);
                });
            }

            if (tempBuilding)
            {
                Destroy(tempBuilding);
            }
        }

        public void BuyBuilding()
        {
            // Check if the building is affordable
            if (BuildingCost)
            {
                CanAfford = BuildingCost.EvaluateConditional();

                if (CanAfford)
                {
                    Mod_ResourceManager.Instance.AddResources(BuildingCost.ResourceCosts);

                    if (MoveAction)
                    {
                        // Create a temporary object at the location to indicate a building will be made there
                        if (DisplayBuilding)
                        {
                            tempBuilding = Instantiate(DisplayBuilding, MoveAction.CurrentTarget, Quaternion.identity);
                        }
                    }
                }
            }
        }

        public override bool SelectionAction(AIAgent agent)
        {
            bool validTarget = false;

            if (MoveAction)
            {
                validTarget = MoveAction.SelectionAction(agent);
            }

            if (validTarget)
            {
                BuyBuilding();
            }
            else
            {
                CanAfford = false;
            }

            return validTarget;
        }

        public override bool SetVariables(AIAgent agent, GameObject go, Vector3 vec)
        {
            bool valid = false;

            if (MoveAction)
            {
                valid = MoveAction.SetVariables(agent, go, vec);
            }

            if (valid)
            {
                BuyBuilding();
            }
            else
            {
                CanAfford = false;
            }

            return valid;
        }
    }
}