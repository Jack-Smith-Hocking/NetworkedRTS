using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS_System.Selection;
using UnityEngine.InputSystem;

namespace RTS_System.AI
{
    [RequireComponent(typeof(AIAgent))]
    public class UnitHandler : DefaultSelectable
    {
        [Header("Unit Data")]
        [Tooltip("The AIAgent that this unit is managing")] public AIAgent Agent = null;
        [Tooltip("Whether or not this unit will implement the DefaultUnitHandler's actions")] public bool UseDefaultActions = true;
        [Tooltip("List of actions that this unit can perform")] public List<ActionInput> ActionInputs = new List<ActionInput>();

        public BoundInput ClearActionInput = new BoundInput();

        // Start is called before the first frame update
        protected override IEnumerator Start()
        {
            yield return base.Start();

            if (!Agent) Agent = gameObject.GetComponent<AIAgent>();

            // If this unit is using the set of default actions then add them to this unit's list of available actions
            if (UseDefaultActions)
            {
                Helper.LoopList_ForEach<ActionInput>(DefaultUnitHandler.Instance.SelectionInputs, (ActionInput s) =>
                {
                    ActionInputs.Add(new ActionInput(s));
                });
            }

            AIAction tempAction = null;
            Helper.LoopList_ForEach<ActionInput>(ActionInputs, (ActionInput s) =>
            {
                // Get a copy of the action associated and add it to the AIAgent's dictionary of possible actions
                tempAction = s.GetActionClone;
                if (tempAction)
                {
                    Agent.SetActionInDict(s.ActionName, tempAction, true);
                    Agent.PossibleActions.Add(tempAction);
                }

                // Set up the input callback for this action
                DefaultUnitHandler.AddPerformedAction(s, PerformedAction);
            });

            if (DefaultUnitHandler.Instance.ClearActionQueueInput)
            {
                ClearActionInput.PerformedActions += (InputAction.CallbackContext cc) =>
                {
                    NetworkHandler.ClientInstance.CmdClearActions(Agent.gameObject);
                };
            }
        }

        public void PerformedAction(ActionInput s)
        {
            if (s != null && s.ActionName.Length > 0)
            {
                // Raycast into the world on the client to get the data needed for the action
                RaycastHit rayHit;
                if (Physics.Raycast(Selector.ClientInstance.SelectorCam.ScreenPointToRay(Input.mousePosition), out rayHit))
                {
                    GameObject go = rayHit.collider.gameObject;

                    // Tell the server to add the action to this AIAgent with all the passed data
                   NetworkHandler.ClientInstance.CmdAddAction(Agent.gameObject, s.ActionName, go, rayHit.point, go.layer, Selector.ClientInstance.AddToActionList);
                }
            }
        }

        #region ISelectable
        public override void OnSelect()
        {
            base.OnSelect();

            // Bind all the inputs on this UnitHandler (meaning that callbacks will now be invoked for this Unit on input)
            DefaultUnitHandler.BindAllInputs(ActionInputs);
            ClearActionInput.Bind(DefaultUnitHandler.Instance.ClearActionQueueInput);
        }

        public override void OnDeselect()
        {
            base.OnDeselect();

            // Unbind all inputs on this UnitHandler (meaning that callbacks will now longer be invoked for this unit on input)
            DefaultUnitHandler.UnbindAllInputs(ActionInputs);
            ClearActionInput.Unbind(DefaultUnitHandler.Instance.ClearActionQueueInput);
        }
        #endregion

        private void OnDestroy()
        {
            // Safely unbind all the associated inputs of this UnitHandler
            DefaultUnitHandler.UnbindAllInputs(ActionInputs);
        }
    }
}