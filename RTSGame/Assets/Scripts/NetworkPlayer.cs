using RTS_System.Selection;
using RTS_System.Resource;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS_System.AI;
using System.Linq;
using Mirror;

namespace RTS_System
{
    public class NetworkPlayer : NetworkBehaviour
    {

        public Selector PlayerSelector = null;
        public Mod_ResourceManager PlayerResourceManager = null;
        [Space]
        public string FriendlyLayer = "Unit";
        public string EnemyLayer = "EnemyUnit";

        IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();
            //if (!isLocalPlayer && !isServer) return;

            AIAgent agent = null;

            Collider[] cols = Physics.OverlapSphere(transform.position, 200);
            Helper.LoopList_ForEach<Collider>(cols.ToList<Collider>(), (Collider col) =>
            // Loop action
            {
                agent = Helper.GetComponent<AIAgent>(col.gameObject);

                if (agent && !agent.AgentOwner)
                {
                    Selector.ClientInstance.CmdSetAgentOwner(col.gameObject, gameObject);

                    Helper.ListAdd<GameObject>(ref PlayerSelector.SceneSelectables, agent.gameObject);
                }
            });
        }
    }
}