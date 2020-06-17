using Pixelplacement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using RTS_System.Selection;

namespace RTS_System.AI
{
    public class AIAgent : DefaultSelectable
    {
        [Header("AIAgent Data")]
        [Tooltip("The NavMeshAgent that will move this AIAgent")] public NavMeshAgent NavAgent = null;
        [Tooltip("List of possible actions")] public List<AIAction> PossibleActions = new List<AIAction>();
        [Space]
        [Tooltip("Current actions")] public List<AIAction> ActionQueue = new List<AIAction>();
        [Tooltip("Current action")] public AIAction CurrentAction = null;

        [Space]
        [HideInInspector] public NetworkPlayer AgentOwner = null;
        [HideInInspector] public TimerTracker ActionTimer = new TimerTracker();

        public bool HasCurrentActionRef { get { return CurrentActionRef.Length > 0; } }
        private string CurrentActionRef = "";
        
        private List<string> ActionQueueRef = new List<string>();
        private Dictionary<string, AIAction> PossibleActionsDict = new Dictionary<string, AIAction>();

        private AIAction cachedAction = null;

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
                AgentOwner.PlayerSelector.AddSelectable(gameObject);
            }

            // Give management of this AIAgent to the AIManager instance, only really matters if this is being run on the server
            if (AIManager.Instance)
            {
                Helper.ListAdd<AIAgent>(ref AIManager.Instance.SceneAI, this);
            }
        }

        #region ActionRefFunctions
        /// <summary>
        /// Set the name of the currently active action, useful for non-server clients
        /// </summary>
        /// <param name="actionName">The name of the current action</param>
        public void SetCurrentActionRef(string actionName)
        {
            CurrentActionRef = actionName;
        }
        /// <summary>
        /// Set the names of the actions in the queue
        /// </summary>
        /// <param name="actionNames">List of action names</param>
        public void SetActionQueueRef(List<string> actionNames)
        {
            ActionQueueRef = actionNames;
        }

        /// <summary>
        /// Gets an action from the list of possible actions
        /// </summary>
        /// <param name="actionName">The name of the action to get</param>
        /// <returns></returns>
        public AIAction GetActionFromDict(string actionName)
        {
            return Helper.GetFromDictionary<string, AIAction>(PossibleActionsDict, actionName);
        }
        /// <summary>
        /// Set an action in the PossibleActionsDict
        /// </summary>
        /// <param name="actionName">Name of the action</param>
        /// <param name="action">Action to set</param>
        /// <param name="overwrite">Whether or not to replace a value that is already in the dictionary</param>
        public void SetActionInDict(string actionName, AIAction action, bool overwrite = false)
        {
            Helper.SetInDictionary<string, AIAction>(ref PossibleActionsDict, actionName, action, overwrite);
        }

        /// <summary>
        /// Get the currently active action
        /// </summary>
        /// <returns>The currently active action</returns>
        public AIAction GetCurrentAction()
        {
            // Will get the currently active action based on what the server has said the name of the action is
            return GetActionFromDict(CurrentActionRef);
        }
        /// <summary>
        /// Get the list of actions that are currently active based on what the server has said is in the queue
        /// </summary>
        /// <returns>The queue of actions</returns>
        public List<AIAction> GetActionQueue()
        {
            List<AIAction> actionQueue = new List<AIAction>(ActionQueueRef.Count);

            Helper.LoopList_ForEach<string>(ActionQueueRef, (string actionName) =>
            {
                actionQueue.Add(GetActionFromDict(actionName));
            });

            return actionQueue;
        }

        /// <summary>
        /// Refreshes the names of the current action and action queue that is held by clients
        /// </summary>
        /// <param name="sendToClients">Whether to get clients to refresh, this is here to stop a stack overflow or worse</param>
        public void RefreshActionRefs(bool sendToClients = true)
        {
            // Get the client selector instance so that commands can be used
            if (Selector.ClientInstance)
            {
                string currentAction = "";
                List<string> actionQueue = new List<string>(ActionQueue.Count);

                // Get the name of the current action
                if (CurrentAction)
                {
                    currentAction = CurrentAction.ActionName;

                    // If there is no CurrentAction than it is safe to assume there is no queue
                    Helper.LoopList_ForEach<AIAction>(ActionQueue, (AIAction action) =>
                    {
                        actionQueue.Add(action.ActionName);
                    });
                }

                // Get the server to update all clients
                if (sendToClients)
                {
                    NetworkHandler.ClientInstance.CmdRefreshAgentActions(gameObject, currentAction, actionQueue.ToArray());
                }

                // If this is a client agent then refresh the UI instance 
                if (Selector.ClientInstance.Equals(AgentOwner.PlayerSelector))
                {
                    StartCoroutine(RefreshUI());
                }
            }
        }
        #endregion

        /// <summary>
        /// Refresh the UI instance 
        /// </summary>
        /// <returns>Will wait until there is a valid CurrentActionRef due to network lag</returns>
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
        /// This will take an action and either set the current action or add it to the queue
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

        /// <summary>
        /// Add am action to the queue or set it as the current action
        /// </summary>
        /// <param name="actionName">The name of the action in the PossiblActions queue</param>
        /// <param name="addToList">Whether to add it to the list or set as current action</param>
        /// <param name="go">GameObejct to be passed to the action</param>
        /// <param name="vec3">Vector3 to be passed to the action</param>
        /// <param name="integer">Integer to be passed to the action</param>
        public void AddAction(string actionName, bool addToList, GameObject go, Vector3 vec3, int integer)
        {
            // Check whether the action is valid
            if (PossibleActionsDict.ContainsKey(actionName))
            {
                AIAction action = PossibleActionsDict[actionName];

                if (action)
                {
                    // Get a copy
                    action = Instantiate(action);
                    // Initialise 
                    action.InitialiseAction(this);

                    // Set the internal data of the action and see if it can be performed
                    if (action.SetVariables(this, go, vec3, integer))
                    {
                        // If it can be performed then handle it appropriately 
                        SetAction(action, addToList, false);
                    }
                    else
                    {
                        // Else cancel the action and move on
                        action.CancelAction(this);
                    }
                }
            }
        }

        /// <summary>
        /// Clears the current action and the queue and then refreshes clients 
        /// </summary>
        /// <param name="refresh"></param>
        public void ClearAllActions(bool refresh = true)
        {
            // If there is a current action then exit it and set to null
            if (CurrentAction)
            {
                CurrentAction.ExitAction(this);
                CurrentAction = null;
            }

            // Loop through the queue and cancel each action
            Helper.LoopList_ForEach<AIAction>(ActionQueue, (AIAction a) => { a.CancelAction(this); });
            ActionQueue.Clear();

            // Refresh ActionRefs
            if (refresh)
            {
                RefreshActionRefs();
            }
        }

        /// <summary>
        /// Destroy this Agent on the server
        /// </summary>
        /// <param name="delay">Delay before destroying</param>
        public void ServerDestroy(float delay)
        {
            StartCoroutine(ServerDestroyDelayed(delay));
        }
        /// <summary>
        /// Server destroy after delay
        /// </summary>
        /// <param name="delay">Time until destroy</param>
        /// <returns></returns>
        IEnumerator ServerDestroyDelayed(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);

            NetworkHandler.ClientInstance.ServDestroyObject(gameObject);
        }

        /// <summary>
        /// Move the Agent to a position
        /// </summary>
        /// <param name="pos">Position to move to</param>
        public void MoveToPoint(Vector3 pos)
        {
            if (NavAgent && NavAgent.isActiveAndEnabled)
            {
                NavAgent.SetDestination(pos);
            }
        }
        /// <summary>
        /// Move the Agent to a transform
        /// </summary>
        /// <param name="target">Transform to move towards</param>
        public void MoveToTransform(Transform target)
        {
            if (!target) return;

            MoveToPoint(target.position);
        }

        private void OnDestroy()
        {
            // Safely clear all actions from the list when destroyed
            ClearAllActions(false);

            // Remove from the AIManager
            if (AIManager.Instance)
            {
                AIManager.Instance.SceneAI.Remove(this);
            }
        }
    }
}
