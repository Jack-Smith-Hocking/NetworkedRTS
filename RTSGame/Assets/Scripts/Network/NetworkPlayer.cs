using Mirror;
using RTS_System.AI;
using RTS_System.Resource;
using RTS_System.Selection;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace RTS_System
{
    [RequireComponent(typeof(NetworkHandler))]
    public class NetworkPlayer : NetworkBehaviour
    {
        [Tooltip("The Selector associated with this NetworkPlayer")] public Selector PlayerSelector = null;
        [Tooltip("The ResourceManager associated with this NetworkPlayer")] public Mod_ResourceManager PlayerResourceManager = null;
        [Space]
        [Tooltip("The layer that units should be in if they are client owned")] public string FriendlyLayer = "Unit";
        [Tooltip("The layer that units should be in if they are not client owned")] public string EnemyLayer = "EnemyUnit";

        IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();
            //if (!isLocalPlayer && !isServer) return;

            AIAgent agent = null;

            // Collect all the starting units and tell them that they belong to this NetworkPlayer
            // Probably a bad way of doing it but it will work for now
            Collider[] cols = Physics.OverlapSphere(transform.position, 200);
            Helper.LoopList_ForEach<Collider>(cols.ToList<Collider>(), (Collider col) =>
            // Loop action
            {
                agent = Helper.GetComponent<AIAgent>(col.gameObject);

                if (agent && !agent.AgentOwner)
                {
                    // Tell all clients that this AIAgent belongs to this NetworkPlayer
                    NetworkHandler.ClientInstance.CmdSetAgentOwner(col.gameObject, gameObject);

                    // Add the AIAgent to the list of selectables for this player
                    PlayerSelector.AddSelectable(agent.gameObject);
                }
            });
        }
    }
}