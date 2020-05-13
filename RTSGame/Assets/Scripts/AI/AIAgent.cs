using ScriptableActions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions.Must;

namespace AI_System
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class AIAgent : MonoBehaviour
    {
        public NavMeshAgent NavAgent = null;
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

            if (!currentAction)
            {
                currentAction = action;
                currentAction.EnterAction(this);
                currentAction.ExecuteAction(this);
            }
            else if (currentAction && !currentAction.Equals(action))
            {
                currentAction.ExitAction(this);
                currentAction = action;
                currentAction.EnterAction(this);
                currentAction.ExecuteAction(this);
            }
        }
    }
}
