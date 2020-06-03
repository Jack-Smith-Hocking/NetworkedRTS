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

        [Tooltip("This will be the GameObject that is used to display the currently run action of an AI (using the ActionDisplayPrefab")] public GameObject CurrentActionDisplayParent = null;
        [Tooltip("The GameObject that will show all of the current actions of an AIAgent")] public GameObject ActionDisplayParent = null;
        [Tooltip("The prefab that will display AIActions")] public GameObject ActionDisplayPrefab = null;

        [Space]
        public List<GameObject> DisplayedActions = new List<GameObject>();
        public UnitHandler DisplayedUnit = null;

        private IEnumerator Start()
        {
            yield return new WaitWhile(() => { return Selector.ClientInstance == null; });

            Instance = this;

            Selector.ClientInstance.OnSelectedChange += UpdateUI;
        }

        public void UpdateUI()
        {
            DisplayAgentActions();
        }

        void GetDisplayAgent()
        {
            DisplayedUnit = Helper.GetComponent<UnitHandler>(Selector.ClientInstance.GetFirstSelected);
            if (DisplayedUnit == null)
            {
                Helper.LoopList_ForEach<GameObject>(Selector.ClientInstance.SelectedObjects,
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