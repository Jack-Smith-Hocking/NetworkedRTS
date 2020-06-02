using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTS_System.AI
{
    [CreateAssetMenu(fileName = "New CollectResourceAction", menuName = "ScriptableObject/RTS/AI/CollectResource")]
    public class CollectResourceAction : AIAction
    {
        [Header("CollectResource Action")]
        public CollectResource CollectResource = null;

        public override bool HasActionCompleted(AIAgent agent)
        {
            return CollectResource.HasCompleted(agent);
        }

        public override bool ExecuteAction(AIAgent agent)
        {
            CollectResource.Execute(agent);
            return true;
        }
        public override float UpdateAction(AIAgent agent)
        {
            CollectResource.Update(agent);
            return 0;
        }

        public override void EnterAction(AIAgent agent)
        {
            CollectResource.Enter(agent);
        }
        public override void ExitAction(AIAgent agent)
        {
            CollectResource.Execute(agent);
        }
        public override bool SetVariables(AIAgent agent, GameObject go, Vector3 vec, int num)
        {
            return CollectResource.SetVariables(agent, go, vec, num);
        }
    }
}