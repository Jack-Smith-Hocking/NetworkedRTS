﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unit_System
{
    public abstract class AIAction : UtilityAction
    {
        public override float UpdateAction(AIAgent agent)
        {
            return EvaluateAction(agent);
        }

        public virtual bool ExecuteAction(AIAgent agent)
        {
            return (agent != null);
        }
        public virtual void EnterAction(AIAgent agent)
        {

        }
        public virtual void ExitAction(AIAgent agent)
        {

        }

        public virtual void CancelAction(AIAgent agent)
        {

        }

        public virtual void SelectionAction(AIAgent agent)
        {

        }
    }
}