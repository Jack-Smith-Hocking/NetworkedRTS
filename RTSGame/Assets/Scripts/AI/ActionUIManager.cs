using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS_System.Selection;
using System;
using UnityEditor;

namespace RTS_System.AI
{
    public class ActionUIManager : MonoBehaviour
    {
        public static ActionUIManager Instance = null;

        public GameObject CurrentActionDisplayParent = null;
        public GameObject ActionDisplayParent = null;
        public GameObject ActionDisplayPrefab = null;

        public Action<Selector> UpdateCallback = null;

        [Space]
        public List<GameObject> DisplayedActions = new List<GameObject>();
        public UnitHandler DisplayedUnit = null;
        public Selector DisplayedSelector = null;

        private void Start()
        {
            Instance = this;

            UpdateCallback += (Selector selector) =>
            {
                if (selector)
                {
                    selector.OnSelectedChange += UpdateUI;

                    DisplayedSelector = selector;
                }
            };

            UpdateCallback.Invoke(Selector.ClientInstance);
        }

        public void UpdateUI()
        {
            DisplayAgentActions();
        }

        void GetDisplayAgent()
        {
            DisplayedUnit = Helper.GetComponent<UnitHandler>(DisplayedSelector.GetFirstSelected);
            if (DisplayedUnit == null)
            {
                Helper.LoopList_ForEach<GameObject>(DisplayedSelector.SelectedObjects, 
                // Loop Action
                (GameObject go) =>
                {
                    DisplayedUnit = Helper.GetComponent<UnitHandler>(go);
                }, 
                // BreakOut Action
                () =>
                {
                    return DisplayedUnit;
                });
            }
        }

        void DisplayAgentActions()
        {
            ClearDisplayedActions();

            if (ActionDisplayPrefab)
            {
                GetDisplayAgent();

                if (DisplayedUnit)
                {
                    GameObject displayPrefab = null;
                    ActionUI actionUI = null;

                    if (ActionDisplayParent)
                    {
                        Helper.LoopList_ForEach<ActionInput>(DisplayedUnit.ActionInputs, (ActionInput actionInput) =>
                        {
                            displayPrefab = Instantiate(ActionDisplayPrefab, ActionDisplayParent.transform);
                            actionUI = Helper.GetComponent<ActionUI>(displayPrefab);

                            DisplayedActions.Add(displayPrefab);

                            if (actionUI)
                            {
                                string inputButton = "\n( " + actionInput.InputActionRef.ToInputAction().name + " )";
                                actionUI.UpdateActionUI(actionInput.GetActionClone, inputButton);
                            }
                        });
                    }

                    if (CurrentActionDisplayParent && DisplayedUnit.Agent.CurrentAction)
                    {
                        displayPrefab = Instantiate(ActionDisplayPrefab, CurrentActionDisplayParent.transform);
                        actionUI = Helper.GetComponent<ActionUI>(displayPrefab);

                        DisplayedActions.Add(displayPrefab);

                        if (actionUI)
                        {
                            actionUI.UpdateActionUI(DisplayedUnit.Agent.CurrentAction);
                        }
                    }
                }
            }
        }

        void ClearDisplayedActions()
        {
            Helper.LoopList_ForEach<GameObject>(DisplayedActions, (GameObject da) =>
            {
                Destroy(da);
            });

            DisplayedActions.Clear();
            DisplayedUnit = null;
        }
    }
}