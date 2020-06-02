using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace RTS_System.AI
{
    [CreateAssetMenu(fileName = "New MoveToAction", menuName = "ScriptableObject/RTS/AI/MoveToPoint")]
    public class MoveToPointAction : AIAction
    {
        [Header("MoveToPoint Action")]
        public MoveToPoint MoveToPoint = null;

        public override float UpdateAction(AIAgent agent)
        {
            MoveToPoint.Update(agent);
            return 0.0f;
        }

        public override bool HasActionCompleted(AIAgent agent)
        {
            return MoveToPoint.HasCompleted(agent);
        }

        public override bool ExecuteAction(AIAgent agent)
        {
            MoveToPoint.Execute(agent);
            return true;
        }

        public override void EnterAction(AIAgent agent)
        {
            MoveToPoint.Enter(agent);
        }
        public override void ExitAction(AIAgent agent)
        {
            MoveToPoint.Exit(agent);
        }

        public override bool SetVariables(AIAgent agent, GameObject go, Vector3 vec, int num)
        {
            return MoveToPoint.SetVariables(agent, go, vec, num);
        }
    }
}