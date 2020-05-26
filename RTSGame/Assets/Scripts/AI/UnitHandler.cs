using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Selector_System;
using System;

namespace Unit_System
{
    [RequireComponent(typeof(AIAgent))]
    public class UnitHandler : SelectableDefault
    {
        [Header("Unit Data")]
        [Tooltip("The AIAgent that this unit is managing")] public AIAgent Agent = null;
        [Tooltip("Whether or not this unit will implement the DefaultUnitHandler's actions")] public bool UseDefaultActions = true;
        [Tooltip("List of actions that this unit can perform")] public List<SelectorInput> SelectionInputs = new List<SelectorInput>();

        // Start is called before the first frame update
        protected override IEnumerator Start()
        {
            yield return base.Start();

            if (!Agent) Agent = gameObject.GetComponent<AIAgent>();

            Helper.LoopList_ForEach<SelectorInput>(SelectionInputs, (SelectorInput s) => 
            { 
                DefaultUnitHandler.AddPerformedAction(s, PerformedAction); 
            });
        }

        public void PerformedAction(SelectorInput s)
        {
            if (s != null && s.Action != null)
            {
                Agent.AddAction(s.Action, Selector.Instance.AddToActionList);
            }
        }

        #region ISelectable
        public override void OnSelect()
        {
            base.OnSelect();

            DefaultUnitHandler.BindAllInputs(SelectionInputs);

            if (UseDefaultActions)
            {
                DefaultUnitHandler.Instance.CurrentUnits.Add(this);
            }
        }

        public override void OnDeselect()
        {
            base.OnDeselect();

            DefaultUnitHandler.UnbindAllInputs(SelectionInputs);

            if (UseDefaultActions)
            {
                DefaultUnitHandler.Instance.CurrentUnits.Remove(this);
            }
        }
        #endregion

        private void OnDestroy()
        {
            if (UseDefaultActions)
            {
                DefaultUnitHandler.Instance.CurrentUnits.Remove(this);
            }
        }
    }
}