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
        [Space]
        public MoveToPoint MoveAction = null;
        [Space]
        public List<AIAction> PossibleActions = new List<AIAction>();

        public Transform CurrentTarget /*{ get; set; } */= null;

        private AIAction currentAction = null;

        // Start is called before the first frame update
        IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();

            for (int i = 0; i < PossibleActions.Count; i++)
            {
                PossibleActions[i] = Instantiate(PossibleActions[i]);
            }

            if (MoveAction)
            {
                MoveAction = Instantiate(MoveAction);
            }

            if (AIManager.Instance)
            {
                AIManager.Instance.SceneAI.Add(this);
            }
        }

        public void UpdateAction()
        {
            if (currentAction)
            {
                currentAction.UpdateAction(this);
            }
        }
        public void EvaluateActions()
        {
            SetCurrentAction(GetBestAction());
        }

        public AIAction GetBestAction()
        {
            AIAction highestAction = null;
            float highestEvaluation = 0;

            if (currentAction)
            {
                highestAction = currentAction;
                highestEvaluation = currentAction.EvaluateAction(this);
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

        public void SetCurrentAction(AIAction action, bool overrideAction = false)
        {
            if (!action)
            {
                return;
            }
            if (currentAction && currentAction.Equals(action))
            {
                return;
            }

            if (!currentAction)
            {
                currentAction = action;
            }
            else
            {
                currentAction.ExitAction(this);
                currentAction = action;
            }

            currentAction.EnterAction(this);
            currentAction.ExecuteAction(this);
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
            if (Selector.Instance && MoveAction)
            {
                if (!Selector.Instance.AddToPath)
                {
                    MoveAction.CurrentTargets.Clear();
                }
                
                MoveAction.CurrentTargets.Add(Selector.Instance.SelectedPoint);

                if (currentAction)
                {
                    currentAction.ExitAction(this);
                }

                currentAction = MoveAction;
                currentAction.EnterAction(this);
                currentAction.ExecuteAction(this);
            }
        }
    }
    #endregion
}
