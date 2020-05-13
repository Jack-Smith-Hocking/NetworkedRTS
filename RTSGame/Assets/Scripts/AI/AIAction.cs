using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI_System
{
    public class AIAction : UtilityAction
    {
        public override float UpdateAction(AIAgent agent)
        {
            return EvaluateAction(agent);
        }
        public virtual float EvaluateAction(AIAgent agent)
        {
            return 0.0f;
        }

        public virtual void EnterAction(AIAgent agent)
        {

        }
        public virtual void ExitAction(AIAgent agent)
        {

        }
    }
}