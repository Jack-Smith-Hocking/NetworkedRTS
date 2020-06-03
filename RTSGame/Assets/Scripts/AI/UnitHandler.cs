using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS_System.Selection;

namespace RTS_System.AI
{
    [RequireComponent(typeof(AIAgent))]
    public class UnitHandler : DefaultSelectable
    {
        [Header("Unit Data")]
        [Tooltip("The AIAgent that this unit is managing")] public AIAgent Agent = null;
        [Tooltip("Whether or not this unit will implement the DefaultUnitHandler's actions")] public bool UseDefaultActions = true;
        [Tooltip("List of actions that this unit can perform")] public List<ActionInput> SelectionInputs = new List<ActionInput>();

        // Start is called before the first frame update
        protected override IEnumerator Start()
        {
            yield return base.Start();

            if (!Agent) Agent = gameObject.GetComponent<AIAgent>();

            if (UseDefaultActions)
            {
                Helper.LoopList_ForEach<ActionInput>(DefaultUnitHandler.Instance.SelectionInputs, (ActionInput s) =>
                {
                    SelectionInputs.Add(new ActionInput(s));
                });
            }
 
            Helper.LoopList_ForEach<ActionInput>(SelectionInputs, (ActionInput s) => 
            { 
                DefaultUnitHandler.AddPerformedAction(s, PerformedAction); 
            });
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

            DefaultUnitHandler.BindAllInputs(SelectionInputs);
        }

        public override void OnDeselect()
        {
            base.OnDeselect();

            DefaultUnitHandler.UnbindAllInputs(SelectionInputs);
        }
        #endregion

        private void OnDestroy()
        {
            DefaultUnitHandler.UnbindAllInputs(SelectionInputs);
        }
    }
}