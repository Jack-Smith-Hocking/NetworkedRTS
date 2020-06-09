using Pixelplacement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using RTS_System.Selection;
using System.Linq;
using System.Linq.Expressions;
using System.Configuration;

namespace RTS_System.AI
{
    public class AIAgent : DefaultSelectable
    {
        [Header("AIAgent Data")]
        [Tooltip("The NavMeshAgent that will move this AIAgent")] public NavMeshAgent NavAgent = null;
        [Tooltip("List of possible actions")] public List<AIAction> PossibleActions = new List<AIAction>();
        [Space]
        [Tooltip("Current actions")] public List<AIAction> ActionQueue = new List<AIAction>();
        public AIAction CurrentAction = null;

        public NetworkPlayer AgentOwner = null;

        private AIAction cachedAction = null;

        public Dictionary<string, AIAction> PossibleActionsDict = new Dictionary<string, AIAction>();

        public TimerDict ActionTimer = new TimerDict();

        [Space]
        public string CurrentActionRef;
        public List<string> ActionQueueRef = new List<string>();

        // Start is called before the first frame update
        protected override IEnumerator Start()
        {
            yield return base.Start();

            // Get copies of the PossibleActions list
            //for (int i = 0; i < PossibleActions.Count; i++)
            //{
            //    PossibleActions[i] = Instantiate(PossibleActions[i]);

            //    PossibleActionsDict[PossibleActions[i].ActionName] = PossibleActions[i];
            //}

            if (AgentOwner)
            {
                Helper.ListAdd<GameObject>(ref AgentOwner.PlayerSelector.SceneSelectables, gameObject);
            }

            // Give management of this AIAgent to the AIManager instance
            if (AIManager.Instance)
            {
                Helper.ListAdd<AIAgent>(ref AIManager.Instance.SceneAI, this);
            }
        }

        public void SetCurrentActionRef(string actionName)
        {
            CurrentActionRef = actionName;
        }
        public void SetActionQueueRef(List<string> actionNames)
        {
            ActionQueueRef = actionNames;
        }

        public AIAction GetActionFromDict(string actionName)
        {
            if (PossibleActionsDict.ContainsKey(actionName))
            {
                return PossibleActionsDict[actionName];
            }

            return null;
        }

        public AIAction GetCurrentAction()
        {
            return GetActionFromDict(CurrentActionRef);
        }
        public List<AIAction> GetActionQueue()
        {
            List<AIAction> actionQueue = new List<AIAction>(ActionQueueRef.Count);

            Helper.LoopList_ForEach<string>(ActionQueueRef, (string actionName) =>
            {
                actionQueue.Add(GetActionFromDict(actionName));
            });

            return actionQueue;
        }

        public void RefreshActionRefs(bool sendToClients = true)
        {
            if (Selector.ClientInstance)
            {
                string currentAction = "";
                List<string> actionQueue = new List<string>(ActionQueue.Count);

                if (CurrentAction)
                {
                    currentAction = CurrentAction.ActionName;
                }

                Helper.LoopList_ForEach<AIAction>(ActionQueue, (AIAction action) =>
                {
                    actionQueue.Add(action.ActionName);
                });

                if (sendToClients)
                {
                    Selector.ClientInstance.CmdRefreshAgentActions(gameObject, currentAction, actionQueue.ToArray());
                }

                if (Selector.ClientInstance.Equals(AgentOwner.PlayerSelector))
                {
                    StartCoroutine(RefreshUI());
                }
            }
        }
        IEnumerator RefreshUI()
        {
            yield return new WaitWhile(() => { return CurrentActionRef.Length == 0; });

            ActionUIManager.Instance.UpdateUI();
        }

        public void UpdateAction()
        {
            // If there is a current action, update it
            if (CurrentAction)
            {
                CurrentAction.UpdateAction(this);

                // If the current action has completed, move on to the next action in the queue
                if (CurrentAction.HasActionCompleted(this))
                {
                    CurrentAction.ExitAction(this);

                    // Get the next action from the queue if there is one
                    if (ActionQueue.Count > 0)
                    {
                        CurrentAction = ActionQueue[0];
                        CurrentAction.EnterAction(this);
                        CurrentAction.ExecuteAction(this);

                        ActionQueue.RemoveAt(0);
                    }
                    else
                    {
                        CurrentAction = null;
                    }

                    RefreshActionRefs();
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
            if (CurrentAction)
            {
                highestAction = CurrentAction;
                highestEvaluation = CurrentAction.EvaluateAction(this);
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
        public void SetAction(AIAction action, bool addToList, bool checkInList = true)
        {
            if (!action)
            {
                return;
            }
            if (CurrentAction && CurrentAction.Equals(action) && checkInList)
            {
                return;
            }

            // If there is no current action or queued action, then just et the current action
            if (!CurrentAction && ActionQueue.Count == 0)
            {
                CurrentAction = action;
                addToList = false;
            }
            // Else if there is a current action, or a queued action, add to the queue
            else if (addToList)
            {
                ActionQueue.Add(action);
            }
            // Otherwise, reset the queue and set the current action
            else
            {
                // Exit out of current action if there is one
                if (CurrentAction)
                {
                    CurrentAction.ExitAction(this);
                }

                // Cancel queued actions
                Helper.LoopList_ForEach<AIAction>(ActionQueue, (AIAction a) =>
                {
                    a.CancelAction(this);
                });

                ActionQueue.Clear();
                CurrentAction = action;
            }

            // Enter and execute the new current action, if there is one
            if (!addToList && CurrentAction)
            {
                CurrentAction.EnterAction(this);
                CurrentAction.ExecuteAction(this);
            }

            RefreshActionRefs();
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

        public void AddAction(string actionName, bool addToList, GameObject go, Vector3 vec3, int integer)
        {
            if (PossibleActionsDict.ContainsKey(actionName))
            {
                AIAction action = PossibleActionsDict[actionName];

                if (action)
                {
                    action = Instantiate(action);
                    action.InitialiseAction(this);

                    if (action.SetVariables(this, go, vec3, integer))
                    {
                        SetAction(action, addToList, false);
                    }
                    else
                    {
                        action.CancelAction(this);
                    }
                }
            }
        }

        public void ClearAllActions(bool refresh = true)
        {
            if (CurrentAction)
            {
                CurrentAction.ExitAction(this);
                CurrentAction = null;
            }

            Helper.LoopList_ForEach<AIAction>(ActionQueue, (AIAction a) => { a.CancelAction(this); });
            ActionQueue.Clear();

            if (refresh)
            {
                RefreshActionRefs();
            }
        }

        private void OnDestroy()
        {
            ClearAllActions(false);

            if (AIManager.Instance)
            {
                AIManager.Instance.SceneAI.Remove(this);
            }
        }

        public void ServerDestroy(float delay)
        {
            ServerDestroyDelayed(delay);
        }
        IEnumerator ServerDestroyDelayed(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);

            AgentOwner.PlayerSelector.ServDestroyObject(gameObject);
        }
    }
}
