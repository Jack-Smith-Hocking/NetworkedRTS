using Pixelplacement;
using Selector_System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Unit_System
{
    //[RequireComponent(typeof(NavMeshAgent))]
    public class AIAgent : SelectableDefault
    {
        [Header("AIAgent Data")]
        [Tooltip("The NavMeshAgent that will move this AIAgent")] public NavMeshAgent NavAgent = null;
        [Tooltip("List of possible actions")] public List<AIAction> PossibleActions = new List<AIAction>();
        [Space]
        [Tooltip("Current actions")] public List<AIAction> actionQueue = new List<AIAction>();
        public AIAction currentAction = null;

        private AIAction cachedAction = null;

        // Start is called before the first frame update
        protected override IEnumerator Start()
        {
            yield return base.Start();

            // Get copies of the PossibleActions list
            for (int i = 0; i < PossibleActions.Count; i++)
            {
                PossibleActions[i] = Instantiate(PossibleActions[i]);
            }

            // Give management of this AIAgent to the AIManager instance
            if (AIManager.Instance)
            {
                AIManager.Instance.SceneAI.Add(this);
            }
        }

        public void UpdateAction()
        {
            // If there is a current action, update it
            if (currentAction)
            {
                currentAction.UpdateAction(this);

                // If the current action has completed, move on to the next action in the queue
                if (currentAction.HasActionCompleted(this))
                {
                    currentAction.ExitAction(this);

                    // Get the next action from the queue if there is one
                    if (actionQueue.Count > 0)
                    {
                        currentAction = actionQueue[0];
                        currentAction.EnterAction(this);
                        currentAction.ExecuteAction(this);

                        actionQueue.RemoveAt(0);
                    }
                    else
                    {
                        currentAction = null;
                    }
                }
            }
        }
        public void EvaluateActions()
        {
            SetAction(GetBestAction(), false);
        }

        /// <summary>
        /// Determines the best action to perform
        /// </summary>
        /// <returns></returns>
        public AIAction GetBestAction()
        {
            AIAction highestAction = null;
            float highestEvaluation = 0;

            // If there is a current action, it is by default the most valuable action to start
            if (currentAction)
            {
                highestAction = currentAction;
                highestEvaluation = currentAction.EvaluateAction(this);
            }

            // Loop through possible actions and evaluate them
            foreach (AIAction action in PossibleActions)
            {
                float evaluateVal = action.EvaluateAction(this);
                DebugManager.LogMessage($"Action {action.name} evaluated with value: {evaluateVal}");

                if (evaluateVal > highestEvaluation)
                {
                    highestEvaluation = evaluateVal;
                    highestAction = action;
                }
            }

            return highestAction;
        }

        /// <summary>
        /// THis will take an action and either set the current action or add it to the queue
        /// </summary>
        /// <param name="action">Action to add</param>
        /// <param name="addToList">If set to false, will set the current action. Otherwise will add to queue</param>
        public void SetAction(AIAction action, bool addToList)
        {
            if (!action)
            {
                return;
            }
            if (currentAction && currentAction.Equals(action))
            {
                return;
            }

            // If there is no current action or queued action, then just et the current action
            if (!currentAction && actionQueue.Count == 0)
            {
                currentAction = action;
                addToList = false;
            }
            // Else if there is a current action, or a queued action, add to the queue
            else if (addToList)
            {
                actionQueue.Add(action);
            }
            // Otherwise, reset the queue and set the current action
            else
            {
                // Exit out of current action if there is one
                if (currentAction)
                {
                    currentAction.ExitAction(this);
                }

                // Cancel queued actions
                Helper.LoopList_ForEach<AIAction>(actionQueue, (AIAction a) =>
                {
                    a.CancelAction(this);
                });
                
                actionQueue.Clear();
                currentAction = action;
            }

            // Enter and execute the new current action, if there is one
            if (!addToList && currentAction)
            {
                currentAction.EnterAction(this);
                currentAction.ExecuteAction(this);
            }
        }

        /// <summary>
        /// Instantiate, initialise and 'select' an action the add to the queue or set as current, will not be added if it's selection fails
        /// </summary>
        /// <param name="action">Action to add</param>
        /// <param name="addToList">Whether to add to the queue or not</param>
        /// <param name="selectAction">Whether to call the SelectAction method on the action or not</param>
        /// <param name="createInstance">Whether or not to instantiate the action</param>
        public void AddAction(AIAction action, bool addToList, bool selectAction = true, bool createInstance = true)
        {
            cachedAction = action;
            if (cachedAction)
            {
                bool selectedPass = false;

                if (createInstance)
                {
                    // Instantiate and initialise the new action
                    cachedAction = Instantiate(cachedAction);
                    cachedAction.InitialiseAction(this);
                }

                if (selectAction)
                {
                    selectedPass = cachedAction.SelectionAction(this);

                    if (!selectedPass)
                    {
                        cachedAction.CancelAction(this);
                    }
                }

                if (selectedPass || !selectAction)
                {
                    SetAction(cachedAction, addToList);
                }
            }
        }
        private void OnDestroy()
        {
            Helper.LoopList_ForEach<AIAction>(actionQueue, (AIAction a) => { a.CancelAction(this); });

            if (AIManager.Instance)
            {
                AIManager.Instance.SceneAI.Remove(this);
            }
        }
    }
}
