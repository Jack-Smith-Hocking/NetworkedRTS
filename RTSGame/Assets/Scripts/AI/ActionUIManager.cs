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
        [Tooltip("The prefab that will display the current AIAction")] public GameObject CurrentActionDisplayPrefab = null;

        [Header("Queued Action UI")]
        [Tooltip("This will be the UI GameObject that will be the parent for the ActionQueue of the AIAgent (using the ActionDisplayPrefab)")] public GameObject ActionQueueDisplayParent = null;
        [Tooltip("The prefab that will display the queued AIActions")] public GameObject ActionQueueDisplayPrefab = null;

        [Header("Available Actions UI")]
        [Tooltip("The GameObject that will show all of the current actions of an AIAgent")] public GameObject ActionDisplayParent = null;
        [Tooltip("The prefab that will display the possible AIActions")] public GameObject ActionDisplayPrefab = null;

        [Header("Other")]
        [Tooltip("List of GameObjects to turn on/off when the ActionUIManager detects no selected GameObjects")] public List<GameObject> ObjectsToToggle = new List<GameObject>();

        private List<GameObject> DisplayedActions = new List<GameObject>();
        private UnitHandler DisplayedUnit = null;

        private IEnumerator Start()
        {
            yield return new WaitWhile(() => { return Selector.ClientInstance == null; });

            Instance = this;

            Selector.ClientInstance.OnSelectedChange += UpdateUI;

            UpdateUI();
        }

        /// <summary>
        /// Destroy the old UI prefabs and make new ones to display
        /// </summary>
        public void UpdateUI()
        {
            DisplayAgentActions();

            Helper.LoopList_ForEach<GameObject>(ObjectsToToggle, (GameObject obj) =>
            {
                obj.SetActive(DisplayedUnit != null);
            });
        }

        /// <summary>
        /// Get the AIAgent to display the actions of
        /// </summary>
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

        /// <summary>
        /// Create and set up a UI prefab for displaying an AIAction
        /// </summary>
        /// <param name="parent">The parent GameObject to bind to</param>
        /// <param name="prefab">The prefab to instantiate</param>
        /// <param name="action">The AIAction to display</param>
        /// <param name="text">The text to display</param>
        void CreateActionUI(GameObject parent, GameObject prefab, AIAction action, string text = "")
        {
            if (Helper.IsNullOrDestroyed<GameObject>(parent) || Helper.IsNullOrDestroyed<GameObject>(prefab) || Helper.IsNullOrDestroyed<AIAction>(action)) return;

            // Create prefab instance
            prefab = Instantiate(prefab, parent.transform);
            // Get the ActionUI component from the prefab instance
            UIDisplay actionUI = Helper.GetComponent<UIDisplay>(prefab);

            // Add the prefab instance to the list to display
            DisplayedActions.Add(prefab);

            if (actionUI)
            {
                actionUI.InitialiseUI(action.ActionIcon, action.ActionName, text);
            }
        }
        
        /// <summary>
        /// Create all the ActionUIs for a list of AIAction 
        /// </summary>
        /// <param name="parent">The parent to bind to</param>
        /// <param name="prefab">The prefab to instantiate</param>
        /// <param name="actions">List of actions to display</param>
        void CreateActionUIs(GameObject parent, GameObject prefab, List<AIAction> actions)
        {
            if (Helper.IsNullOrDestroyed<GameObject>(parent) || Helper.IsNullOrDestroyed<GameObject>(prefab)) return;

            // Loop through and create an ActionUI for each AIAction
            Helper.LoopList_ForEach<AIAction>(actions, (AIAction action) =>
            {
                CreateActionUI(parent, prefab, action);
            });
        }

        /// <summary>
        /// Display the CurrentAction, PossibleActions and QueuedActions of the currently selected AIAgent
        /// </summary>
        void DisplayAgentActions()
        {
            // Clear the currently displayed AIActions
            ClearDisplayedActions();

            if (ActionDisplayPrefab)
            {
                // Get the current AIAgent to display
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
                    if (DisplayedUnit.Agent.HasCurrentActionRef)
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

        /// <summary>
        /// Clear all of the currently displayed ActionUIs
        /// </summary>
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