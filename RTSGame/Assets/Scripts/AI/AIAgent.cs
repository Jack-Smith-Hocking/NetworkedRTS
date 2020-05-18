using Pixelplacement;
using Selector_Systen;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AI_System
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class AIAgent : MonoBehaviour, Selector_Systen.ISelectable
    {
        public NavMeshAgent NavAgent = null;
        public List<AIAction> PossibleActions = new List<AIAction>();

        public List<AIAction> currentActions = new List<AIAction>();

        // Start is called before the first frame update
        IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();

            for (int i = 0; i < PossibleActions.Count; i++)
            {
                PossibleActions[i] = Instantiate(PossibleActions[i]);
            }

            if (AIManager.Instance)
            {
                AIManager.Instance.SceneAI.Add(this);
            }
        }

        public void UpdateAction()
        {
            if (currentActions.Count > 0)
            {
                currentActions[0].UpdateAction(this);

                if (currentActions[0].HasActionCompleted(this))
                {
                    currentActions[0].ExitAction(this);
                    currentActions.RemoveAt(0);
                 
                    if (currentActions.Count > 0)
                    {
                        currentActions[0].EnterAction(this);
                        currentActions[0].ExecuteAction(this);
                    }
                }
            }
        }
        public void EvaluateActions()
        {
            SetCurrentAction(GetBestAction(), false);
        }

        public AIAction GetBestAction()
        {
            AIAction highestAction = null;
            float highestEvaluation = 0;

            if (currentActions.Count > 0)
            {
                highestAction = currentActions[0];
                highestEvaluation = currentActions[0].EvaluateAction(this);
            }

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

        public void SetCurrentAction(AIAction action, bool addToList)
        {
            if (!action)
            {
                return;
            }
            if (currentActions.Count > 0 && currentActions[0].Equals(action))
            {
                return;
            }

            if (currentActions.Count == 0)
            {
                currentActions.Add(action);
                addToList = false;
            }
            else if (addToList)
            {
                currentActions.Add(action);
            }
            else
            {
                currentActions[0].ExitAction(this);
                currentActions.Clear();

                currentActions.Add(action);
            }

            if (!addToList)
            {
                currentActions[0].EnterAction(this);
                currentActions[0].ExecuteAction(this);
            }
        }

        #region ISelectable
        public void OnHover()
        {
        }

        public void OnSelect()
        {
        }

        public void OnDeselect()
        {
        }

        public void OnExecute()
        {
            if (Selector.Instance)
            {
                AIAction newAction = Selector.Instance.CurrentAction;
                if (newAction)
                {
                    newAction = Instantiate(newAction);
                    newAction.InitialiseAction(this);
                    newAction.SelectionAction(this);

                    SetCurrentAction(newAction, Selector.Instance.AddToActionList);
                }
            }
        }
    }
    #endregion
}
