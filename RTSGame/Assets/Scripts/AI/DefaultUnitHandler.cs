using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using RTS_System;

namespace RTS_System.AI
{
    [System.Serializable]
    public class SelectorInput
    {
        [Tooltip("Identifier for long inspector lists")] public string SelectorInputName;
        [Tooltip("Input to bind to")] public InputActionReference InputAction = null;
        [Tooltip("Action to perform on key press")] public AIAction Action = null;
        public BoundInput BoundInput = new BoundInput();

        public SelectorInput() { }
        public SelectorInput(SelectorInput selectorInput)
        {
            if (selectorInput == null) return;

            SelectorInputName = selectorInput.SelectorInputName;
            InputAction = selectorInput.InputAction;
            Action = selectorInput.Action;
        }
    }

    public class DefaultUnitHandler : MonoBehaviour
    {
        public static DefaultUnitHandler Instance = null;
        
        [Tooltip("List of actions that can be performed by all Units if they choose")] public List<SelectorInput> SelectionInputs = new List<SelectorInput>();

        private bool isBound = false;

        // Start is called before the first frame update
        void Start()
        {
            Instance = this;
        }
        public static void AddPerformedAction(SelectorInput s, Action<SelectorInput> performedAction)
        {
            if (s == null || performedAction == null) return;

            s.BoundInput.PerformedActions += (InputAction.CallbackContext cc) =>
            {
                performedAction.Invoke(s);
            };
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

        /// <summary>
        /// Bind the PerformedActions of the SelectorInput to their InputAction
        /// </summary>
        /// <param name="s">SelectorInput to bind</param>
        public static void BindInput(SelectorInput s)
        {
            if (s != null)
            {
                s.BoundInput.Bind(s.InputAction);
            }
        }

        /// <summary>
        /// Bind the PerformedActions of the SelectorInputs to their InputAction
        /// </summary>
        /// <param name="inputs">SelectorInput list to bind</param>
        public static void BindAllInputs(List<SelectorInput> inputs)
        {
            Helper.LoopList_ForEach<SelectorInput>(inputs, BindInput);
        }


        /// <summary>
        /// Unbind the PerformedActions of the SelectorInput to their InputAction
        /// </summary>
        /// <param name="s">SelectorInput to unbind</param>
        public static void UnbindInput(SelectorInput s)
        {
            if (s != null)
            {
                s.BoundInput.Unbind(s.InputAction);
            }
        }
        /// <summary>
        /// Unbind the PerformedActions of the SelectorInputs to their InputAction
        /// </summary>
        /// <param name="inputs">SelectorInput list to unbind</param>
        public static void UnbindAllInputs(List<SelectorInput> inputs)
        {
            Helper.LoopList_ForEach<SelectorInput>(inputs, UnbindInput);
        }
    }
}