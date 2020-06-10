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

        [Header("Current Action UI")]
        [Tooltip("This will be the GameObject that is used to display the currently run action of an AIAgent (using the ActionDisplayPrefab")] public GameObject CurrentActionDisplayParent = null;
        public GameObject CurrentActionDisplayPrefab = null;

        [Header("Queued Action UI")]
        [Tooltip("This will be the UI GameObject that will be the parent for the ActionQueue of the AIAgent (using the ActionDisplayPrefab)")] public GameObject ActionQueueDisplayParent = null;
        public GameObject ActionQueueDisplayPrefab = null;

        [Header("Available Actions UI")]
        [Tooltip("The GameObject that will show all of the current actions of an AIAgent")] public GameObject ActionDisplayParent = null;
        [Tooltip("The prefab that will display AIActions")] public GameObject ActionDisplayPrefab = null;

        [Header("Other")]
        public List<GameObject> ObjectsToToggle = new List<GameObject>();

        [Space]
        public List<GameObject> DisplayedActions = new List<GameObject>();
        public UnitHandler DisplayedUnit = null;


        private IEnumerator Start()
        {
            yield return new WaitWhile(() => { return Selector.ClientInstance == null; });

            Instance = this;

            Selector.ClientInstance.OnSelectedChange += UpdateUI;

            UpdateUI();
        }

        public void UpdateUI()
        {
            DisplayAgentActions();

            Helper.LoopList_ForEach<GameObject>(ObjectsToToggle, (GameObject obj) =>
            {
                obj.SetActive(DisplayedUnit != null);
            });
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

        void CreateActionUI(GameObject parent, GameObject prefab, AIAction action, string text = "")
        {
            if (!parent || !prefab || !action) return;

            prefab = Instantiate(prefab, parent.transform);
            ActionUI actionUI = Helper.GetComponent<ActionUI>(prefab);

            DisplayedActions.Add(prefab);

            if (actionUI)
            {
                actionUI.UpdateActionUI(action, text);
            }
        }
        void CreateActionUIs(GameObject parent, GameObject prefab, List<AIAction> actions)
        {
            if (!parent || !prefab) return;

            Helper.LoopList_ForEach<AIAction>(actions, (AIAction action) =>
            {
                CreateActionUI(parent, prefab, action);
            });
        }

        void DisplayAgentActions()
        {
            ClearDisplayedActions();

            if (ActionDisplayPrefab)
            {
                GetDisplayAgent();

                if (DisplayedUnit)
                {
                    if (ActionDisplayParent)
                    {
                        Helper.LoopList_ForEach<ActionInput>(DisplayedUnit.ActionInputs, (ActionInput actionInput) =>
                        {
                            CreateActionUI(ActionDisplayParent, ActionDisplayPrefab, actionInput.GetActionClone, "\n( " + actionInput.InputActionRef.ToInputAction().name + " )");
                        });
                    }

                    // Update queue and current action UI
                    if (DisplayedUnit.Agent.CurrentActionRef.Length > 0)
                    {
                        if (CurrentActionDisplayParent)
                        {
                            CreateActionUI(CurrentActionDisplayParent, CurrentActionDisplayPrefab, DisplayedUnit.Agent.GetCurrentAction());
                        }
                        if (ActionQueueDisplayParent)
                        {
                            CreateActionUIs(ActionQueueDisplayParent, ActionQueueDisplayPrefab, DisplayedUnit.Agent.GetActionQueue());
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