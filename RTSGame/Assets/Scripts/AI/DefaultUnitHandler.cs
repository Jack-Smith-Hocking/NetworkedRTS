using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using RTS_System;

namespace RTS_System.AI
{
    [System.Serializable]
    public class ActionInput
    {
        [Tooltip("Identifier for long inspector lists")] public string SelectorInputName;
        [Tooltip("Input to bind to")] public InputActionReference InputActionRef = null;

        [SerializeField]
        [Tooltip("Action to perform on key press")] private AIAction Action = null;

        public BoundInput BoundInput = new BoundInput();

        public string ActionName
        {
            get
            {
                if (Action)
                {
                    return Action.ActionName;
                }
                return string.Empty;
            }
        }
        public AIAction GetActionClone
        {
            get
            {
                if (Action)
                {
                    return GameObject.Instantiate(Action);
                }

                return null;    
            }
        }

        public ActionInput() { }

        // Make a copy
        public ActionInput(ActionInput selectorInput)
        {
            if (selectorInput == null) return;

            SelectorInputName = selectorInput.SelectorInputName;
            InputActionRef = selectorInput.InputActionRef;
            Action = selectorInput.Action;
        }
    }

    public class DefaultUnitHandler : MonoBehaviour
    {
        public static DefaultUnitHandler Instance = null;

        public InputActionReference ClearActionQueueInput = null;

        [Tooltip("List of actions that can be performed by all Units if they choose")] public List<ActionInput> SelectionInputs = new List<ActionInput>();

        private bool isBound = false;

        // Start is called before the first frame update
        void Start()
        {
            Instance = this;
        }
        public static void AddPerformedAction(ActionInput s, Action<ActionInput> performedAction)
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
        public static void BindInput(ActionInput s)
        {
            if (s != null)
            {
                s.BoundInput.Bind(s.InputActionRef);
            }
        }

        /// <summary>
        /// Bind the PerformedActions of the SelectorInputs to their InputAction
        /// </summary>
        /// <param name="inputs">SelectorInput list to bind</param>
        public static void BindAllInputs(List<ActionInput> inputs)
        {
            Helper.LoopList_ForEach<ActionInput>(inputs, BindInput);
        }


        /// <summary>
        /// Unbind the PerformedActions of the SelectorInput to their InputAction
        /// </summary>
        /// <param name="s">SelectorInput to unbind</param>
        public static void UnbindInput(ActionInput s)
        {
            if (s != null)
            {
                s.BoundInput.Unbind(s.InputActionRef);
            }
        }
        /// <summary>
        /// Unbind the PerformedActions of the SelectorInputs to their InputAction
        /// </summary>
        /// <param name="inputs">SelectorInput list to unbind</param>
        public static void UnbindAllInputs(List<ActionInput> inputs)
        {
            Helper.LoopList_ForEach<ActionInput>(inputs, UnbindInput);
        }
    }
}