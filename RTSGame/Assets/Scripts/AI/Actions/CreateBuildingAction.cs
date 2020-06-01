using RTS_System.Resource;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTS_System.AI
{
    [CreateAssetMenu(fileName = "New CreateBuldingAction", menuName = "ScriptableObject/RTS/AI/CreateBuilding")]
    public class CreateBuildingAction : AIAction
    {
        [Header("CreateBuilding Action")]
        public CreateBuilding CreateBuilding = null;

        public override bool HasActionCompleted(AIAgent agent)
        {
            return CreateBuilding.HasCompleted(agent);
        }

        public override float UpdateAction(AIAgent agent)
        {
            CreateBuilding.Update(agent);

            return 0.0f;
        }

        public override bool ExecuteAction(AIAgent agent)
        {
            CreateBuilding.Execute(agent);

            return false;   
        }

        public override void EnterAction(AIAgent agent)
        {
            CreateBuilding.Enter(agent);
        }
        public override void ExitAction(AIAgent agent)
        {
            CreateBuilding.Exit(agent);
        }

        public override void CancelAction(AIAgent agent)
        {
            CreateBuilding.Cancel(agent);
        }

        public override bool SetVariables(AIAgent agent, GameObject go, Vector3 vec)
        {
            return CreateBuilding.SetVariables(agent, go, vec);
        }
    }
}