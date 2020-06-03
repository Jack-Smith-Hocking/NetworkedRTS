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
        public AIAgent DisplayedAgent = null;
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
            DisplayedAgent = Helper.GetComponent<AIAgent>(DisplayedSelector.GetFirstSelected);
            if (DisplayedAgent == null)
            {
                Helper.LoopList_ForEach<GameObject>(DisplayedSelector.SelectedObjects, 
                // Loop Action
                (GameObject go) =>
                {
                    DisplayedAgent = Helper.GetComponent<AIAgent>(go);
                }, 
                // BreakOut Action
                () =>
                {
                    return DisplayedAgent;
                });
            }
        }

        void DisplayAgentActions()
        {
            ClearDisplayedActions();

            if (ActionDisplayPrefab)
            {
                GetDisplayAgent();

                if (DisplayedAgent)
                {
                    GameObject displayPrefab = null;
                    ActionUI actionUI = null;

                    if (ActionDisplayParent)
                    {
                        Helper.LoopList_ForEach<AIAction>(DisplayedAgent.PossibleActions, (AIAction action) =>
                        {
                            displayPrefab = Instantiate(ActionDisplayPrefab, ActionDisplayParent.transform);
                            actionUI = Helper.GetComponent<ActionUI>(displayPrefab);

                            DisplayedActions.Add(displayPrefab);

                            if (actionUI)
                            {
                                actionUI.ActionToDisplay = action;
                                actionUI.UpdateActionUI();
                            }
                        });
                    }

                    if (CurrentActionDisplayParent && DisplayedAgent.CurrentAction)
                    {
                        displayPrefab = Instantiate(ActionDisplayPrefab, CurrentActionDisplayParent.transform);
                        actionUI = Helper.GetComponent<ActionUI>(displayPrefab);

                        DisplayedActions.Add(displayPrefab);

                        if (actionUI)
                        {
                            actionUI.ActionToDisplay = DisplayedAgent.CurrentAction;
                            actionUI.UpdateActionUI();
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
            DisplayedAgent = null;
        }
    }
}