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
                tempAction = s.GetActionClone;
                if (tempAction)
                {
                    Agent.PossibleActionsDict[s.ActionName] = tempAction;
                    Agent.PossibleActions.Add(tempAction);
                }

                DefaultUnitHandler.AddPerformedAction(s, PerformedAction); 
            });

            if (DefaultUnitHandler.Instance.ClearActionQueueInput)
            {
                ClearActionInput.PerformedActions += (InputAction.CallbackContext cc) =>
                {
                    Agent.AgentOwner.PlayerSelector.CmdClearActions(Agent.gameObject);
                };
            }
        }

        public void PerformedAction(ActionInput s)
        {
            if (s != null && s.ActionName.Length > 0)
            {
                RaycastHit rayHit;
                if (Physics.Raycast(Agent.AgentOwner.PlayerSelector.SelectorCam.ScreenPointToRay(Input.mousePosition), out rayHit))
                {
                    Agent.AgentOwner.PlayerSelector.CmdAddAction(Agent.gameObject, s.ActionName, rayHit.collider.gameObject, rayHit.point, rayHit.collider.gameObject.layer, Agent.AgentOwner.PlayerSelector.AddToActionList);
                }
            }
        }

        #region ISelectable
        public override void OnSelect()
        {
            base.OnSelect();

            DefaultUnitHandler.BindAllInputs(ActionInputs);
            ClearActionInput.Bind(DefaultUnitHandler.Instance.ClearActionQueueInput);
        }

        public override void OnDeselect()
        {
            base.OnDeselect();

            DefaultUnitHandler.UnbindAllInputs(ActionInputs);
            ClearActionInput.Unbind(DefaultUnitHandler.Instance.ClearActionQueueInput);
        }
        #endregion

        private void OnDestroy()
        {
            DefaultUnitHandler.UnbindAllInputs(ActionInputs);
        }
    }
}