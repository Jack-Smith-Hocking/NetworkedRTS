using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Selector_System;
using System;

namespace Unit_System
{
    [RequireComponent(typeof(AIAgent))]
    public class UnitHandler : MonoBehaviour, ISelectable
    {
        public AIAgent Agent = null;
        public bool UseDefaultActions = true;
        public List<SelectorInput> SelectionInputs = new List<SelectorInput>();

        // Start is called before the first frame update
        void Start()
        {
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
        public void OnHover()
        {
        }

        public void OnSelect()
        {
            DefaultUnitHandler.BindAllInputs(SelectionInputs);

            if (UseDefaultActions)
            {
                DefaultUnitHandler.Instance.CurrentUnits.Add(this);
            }
        }

        public void OnDeselect()
        {
            DefaultUnitHandler.UnbindAllInputs(SelectionInputs);

            if (UseDefaultActions)
            {
                DefaultUnitHandler.Instance.CurrentUnits.Remove(this);
            }
        }

        public void OnExecute()
        {
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