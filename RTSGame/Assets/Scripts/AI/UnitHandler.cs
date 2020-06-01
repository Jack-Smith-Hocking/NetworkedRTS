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
        [Tooltip("List of actions that this unit can perform")] public List<SelectorInput> SelectionInputs = new List<SelectorInput>();

        // Start is called before the first frame update
        protected override IEnumerator Start()
        {
            yield return base.Start();

            if (!Agent) Agent = gameObject.GetComponent<AIAgent>();

            if (UseDefaultActions)
            {
                Helper.LoopList_ForEach<SelectorInput>(DefaultUnitHandler.Instance.SelectionInputs, (SelectorInput s) =>
                {
                    SelectionInputs.Add(new SelectorInput(s));
                });
            }
 
            Helper.LoopList_ForEach<SelectorInput>(SelectionInputs, (SelectorInput s) => 
            {
                if (s.Action != null)
                {
                    s.Action = Instantiate(s.Action);
                }
            });

            Helper.LoopList_ForEach<SelectorInput>(SelectionInputs, (SelectorInput s) => 
            { 
                DefaultUnitHandler.AddPerformedAction(s, PerformedAction); 
            });
        }

        public void PerformedAction(SelectorInput s)
        {
            if (s != null && s.Action != null)
            {
                RaycastHit rayHit;
                if (Physics.Raycast(Agent.AgentOwner.PlayerSelector.SelectorCam.ScreenPointToRay(Input.mousePosition), out rayHit))
                {
                    Agent.AgentOwner.PlayerSelector.CmdAddAction(Agent.gameObject, s.Action.ActionName, rayHit.collider.gameObject, rayHit.point, Agent.AgentOwner.PlayerSelector.AddToActionList);
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
        }
    }
}