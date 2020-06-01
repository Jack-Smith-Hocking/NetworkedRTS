using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace RTS_System.AI
{
    [CreateAssetMenu(fileName = "New MoveToAction", menuName = "ScriptableObject/RTS/AI/MoveToTarget")]
    public class MoveToTargetAction : AIAction
    {
        [Header("MoveToTarget Action")]
        public MoveToTarget MoveToTarget = null;

        public override bool HasActionCompleted(AIAgent agent)
        {
            return MoveToTarget.HasCompleted(agent);
        }

        public override float UpdateAction(AIAgent agent)
        {
            MoveToTarget.Update(agent);

            return 0.0f;
        }

        public override bool ExecuteAction(AIAgent agent)
        {
            MoveToTarget.Execute(agent);

            return false;
        }

        public override void EnterAction(AIAgent agent)
        {
            MoveToTarget.Enter(agent);
        }
        public override void ExitAction(AIAgent agent)
        {
            MoveToTarget.Exit(agent);
        }

        public override bool SelectionAction(AIAgent agent)
        {
            return false;
        }

        public override bool SetVariables(AIAgent agent, GameObject go, Vector3 vec)
        {
            return MoveToTarget.SetVariables(agent, go, vec);
        }
    }
}