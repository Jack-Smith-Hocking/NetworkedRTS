using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTS_System.AI
{
    [CreateAssetMenu(fileName = "New CreateUnitAction", menuName = "ScriptableObject/RTS/AI/CreateUnit")]
    public class CreateUnitAction : AIAction
    {
        [Header("Unit Data")]
        [Tooltip("Data for creating units")] public CreateBuildingAction BuildAction = null;
        [Tooltip("The max distance from the AIAgent this is working on that a unit can be spawned")] public float MaxSpawnDist = 10;

        private bool canCreateUnit = false;

        public override void InitialiseAction(AIAgent agent)
        {
            if (BuildAction)
            {
                BuildAction = Instantiate(BuildAction);
                BuildAction.InitialiseAction(agent);
            }
        }

        public override bool HasActionCompleted(AIAgent agent)
        {
            if (BuildAction)
            {
                return BuildAction.HasActionCompleted(agent) || canCreateUnit == false;
            }

            return canCreateUnit;
        }

        public override float UpdateAction(AIAgent agent)
        {
            if (BuildAction)
            {
                BuildAction.Build();
            }

            return 0.0f;
        }

        public override bool ExecuteAction(AIAgent agent)
        {
            if (BuildAction && canCreateUnit)
            {
                BuildAction.StartTimer(agent);
            }

            return true;
        }

        public override void EnterAction(AIAgent agent)
        {
            if (BuildAction)
            {
                BuildAction.EnterAction(agent);
            }
        }
        public override void ExitAction(AIAgent agent)
        {
            if (BuildAction)
            {
                BuildAction.ExitAction(agent);
            }
        }
        public override void CancelAction(AIAgent agent)
        {
            if (BuildAction)
            {
                BuildAction.CancelAction(agent);
            }
        }

        public override bool SelectionAction(AIAgent agent)
        {
            canCreateUnit = false;

            if (BuildAction)
            {
                if (BuildAction.MoveAction)
                {
                    canCreateUnit = BuildAction.MoveAction.SelectionAction(agent);

                    if (Vector3.Distance(BuildAction.MoveAction.CurrentTarget, agent.transform.position) > MaxSpawnDist)
                    {
                        canCreateUnit = false;
                    }
                }

                if (canCreateUnit)
                {
                    BuildAction.BuyBuilding(agent);
                }
            }

            return canCreateUnit;
        }

        public override bool SetVariables(AIAgent agent, GameObject go, Vector3 vec)
        {
            canCreateUnit = false;

            if (BuildAction)
            {
                if (BuildAction.MoveAction)
                {
                    canCreateUnit = BuildAction.MoveAction.SetVariables(agent, go, vec);

                    if (Vector3.Distance(BuildAction.MoveAction.CurrentTarget, agent.transform.position) > MaxSpawnDist)
                    {
                        canCreateUnit = false;
                    }
                }

                if (canCreateUnit)
                {
                    BuildAction.BuyBuilding(agent);
                }
            }

            return canCreateUnit;
        }
    }
}