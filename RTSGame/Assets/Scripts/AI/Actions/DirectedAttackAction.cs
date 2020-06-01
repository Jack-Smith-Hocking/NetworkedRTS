using System.Collections;
using System.Collections.Generic;
using Unity.XR.OpenVR;
using UnityEngine;

namespace RTS_System.AI
{
    [CreateAssetMenu(fileName = "New DirectedAttack", menuName = "ScriptableObject/RTS/AI/DirectedAttack")]
    public class DirectedAttackAction : AIAction
    {
        [Header("DirectedAttack Action")]
        public DirectedAttack DirectedAttack = null;

        public override bool HasActionCompleted(AIAgent agent)
        {
            return DirectedAttack.HasCompleted(agent);
        }
        public override float UpdateAction(AIAgent agent)
        {
            DirectedAttack.Update(agent);

            return 0.0f;
        }

        public override void EnterAction(AIAgent agent)
        {
            DirectedAttack.Enter(agent);
        }
        public override void ExitAction(AIAgent agent)
        {
            DirectedAttack.Exit(agent);
        }

        public override bool ExecuteAction(AIAgent agent)
        {
            DirectedAttack.Execute(agent);

            return false;
        }

        public override bool SetVariables(AIAgent agent, GameObject go, Vector3 vec)
        {
            return DirectedAttack.SetVariables(agent, go, vec);
        }
    }
}