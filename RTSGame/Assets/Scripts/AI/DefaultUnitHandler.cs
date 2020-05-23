using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Selector_System;

namespace Unit_System
{
    [System.Serializable]
    public class SelectorInput
    {
        public string SelectorInputName;
        public InputActionReference InputAction = null;
        public AIAction Action = null;

        public BoundInput BoundInput = new BoundInput();
    }

    public class DefaultUnitHandler : MonoBehaviour
    {
        public static DefaultUnitHandler Instance = null;

        public List<AIAgent> CurrentAgents = new List<AIAgent>();
        public List<SelectorInput> SelectionInputs = new List<SelectorInput>();

        private bool isBound = false;

        // Start is called before the first frame update
        void Start()
        {
            Instance = this;

            Helper.LoopList_ForEach<SelectorInput>(SelectionInputs, (SelectorInput s) =>
            {
                AddPerformedAction(s, OnActionPerformed);
            });

            BindAllDefaults();
        }
        public static void AddPerformedAction(SelectorInput s, Action<SelectorInput> performedAction)
        {
            if (s == null || performedAction == null) return;

            s.BoundInput.PerformedActions += (InputAction.CallbackContext cc) =>
            {
                performedAction?.Invoke(s);
            };
        }

        void OnActionPerformed(SelectorInput s)
        {
            Helper.LoopList_ForEach<AIAgent>(CurrentAgents, (AIAgent a) =>
            {
                a.AddAction(s.Action, Selector.Instance.AddToActionList);
            });
        }

        public void BindAllDefaults()
        {
            if (!isBound)
            {
                BindAllInputs(SelectionInputs);
                isBound = true;
            }
        }
        public void UnbindAllDefaults()
        {
            if (isBound)
            {
                UnbindAllInputs(SelectionInputs);
                isBound = false;
            }
        }

        public static void BindInput(SelectorInput s)
        {
            if (s != null)
            {
                s.BoundInput.Bind(s.InputAction);
            }
        }
        public static void BindAllInputs(List<SelectorInput> inputs)
        {
            Helper.LoopList_ForEach<SelectorInput>(inputs, BindInput);
        }

        public static void UnbindInput(SelectorInput s)
        {
            if (s != null)
            {
                s.BoundInput.Unbind(s.InputAction);
            }
        }
        public static void UnbindAllInputs(List<SelectorInput> inputs)
        {
            Helper.LoopList_ForEach<SelectorInput>(inputs, UnbindInput);
        }
    }
}