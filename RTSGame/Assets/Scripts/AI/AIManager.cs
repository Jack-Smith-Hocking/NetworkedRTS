using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AI_System
{
    public class AIManager : MonoBehaviour
    {
        public static AIManager Instance = null;

        public bool EvalInUpdate = false;
        public float EvalUpdateIntervals = 0.1f;
        [Range(0, 100)] public int EvalUpdatePercentage = 25;
        [Space]

        public List<AIAgent> SceneAI = new List<AIAgent>();

        private bool doingUpdate = false;
        private float currentCount = 0;

        // Start is called before the first frame update
        void Start()
        {
            if (!Instance)
            {
                Instance = this;
            }

            //InvokeRepeating("DoUpdate", 0.1f, Mathf.Abs(EvalUpdateIntervals));
        }

        private void Update()
        {
            Helper.LoopListForEach<AIAgent>(SceneAI, (AIAgent agent) => { agent.UpdateAction(); });

            if (EvalInUpdate)
            {
                UpdateEvaluations();
            }
            else
            {
                if (!doingUpdate)
                {
                    StartCoroutine(DoUpdate(Mathf.Abs(EvalUpdateIntervals)));
                }
            }
        }

        IEnumerator DoUpdate(float delay = 0)
        {
            if (EvalInUpdate)
            {
                doingUpdate = false;
                yield return null;
            }

            doingUpdate = true;

            yield return new WaitForSeconds(delay);
            UpdateEvaluations();

            doingUpdate = false;
        }

        void UpdateEvaluations()
        {
            if (currentCount >= SceneAI.Count) currentCount = 0;

            float mod = ((float)EvalUpdatePercentage / 100f);
            currentCount += SceneAI.Count * mod;

            if (currentCount == 0) currentCount = SceneAI.Count;

            // Loop through and update all the AI
            for (int i = 0; i < currentCount; i++)
            {
                SceneAI[i].EvaluateActions();
            }
        }
    }
}