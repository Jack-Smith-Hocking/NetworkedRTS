using RTS_System.Selection;
using RTS_System.Resource;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS_System.AI;
using System.Linq;

namespace RTS_System
{
    public class Player : MonoBehaviour
    {
        public Selector PlayerSelector = null;
        public Mod_ResourceManager PlayerResourceManager = null;

        private void Start()
        {
            AIAgent agent = null;

            Collider[] cols = Physics.OverlapSphere(transform.position, 200);
            Helper.LoopList_ForEach<Collider>(cols.ToList<Collider>(), (Collider col) =>
            // Loop action
            {
                agent = Helper.GetComponent<AIAgent>(col.gameObject);

                if (agent)
                {
                    agent.AgentOwner = this;
                    Helper.ListAdd<GameObject>(ref PlayerSelector.SceneSelectables, agent.gameObject);
                }
            });
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawSphere(transform.position, 200);
        }
    }
}