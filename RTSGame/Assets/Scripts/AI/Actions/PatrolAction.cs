using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTS_System.AI
{
    [CreateAssetMenu(fileName = "New PatrolAction", menuName = "ScriptableObject/RTS/AI/Patrol")]
    public class PatrolAction : AIAction
    {
        [Header("Patrol Action")]
        public PatrolPoints PatrolData = null;

        public override float UpdateAction(AIAgent agent)
        {
            PatrolData.Update(agent);
            return 0;
        }

        public override bool ExecuteAction(AIAgent agent)
        {
            PatrolData.Execute(agent);
            return true;
        }

        public override void EnterAction(AIAgent agent)
        {
            PatrolData.Enter(agent);
        }
        public override void ExitAction(AIAgent agent)
        {
            PatrolData.Exit(agent);
        }

        public override bool SelectionAction(AIAgent agent)
        {
            return false;
        }

        public override bool SetVariables(AIAgent agent, GameObject go, Vector3 vec)
        {
            return PatrolData.SetVariables(agent, go, vec);
        }
    }
}

