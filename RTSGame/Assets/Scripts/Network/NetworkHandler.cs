using Mirror;
using UnityEngine;
using RTS_System.AI;
using System.Linq;

namespace RTS_System
{
    public class NetworkHandler : NetworkBehaviour
    {
        public static NetworkHandler ClientInstance = null;

        public void Start()
        {
            if (isLocalPlayer)
            {
                ClientInstance = this;
            }
        }

        #region NetworkFunctions
        /// <summary>
        /// Send a command to the server to add an AIAction to an AIAgent's queue, the parameters to be used in the SetVariables(...) call 
        /// </summary>
        /// <param name="agent">The AIAgent to add the AIAction to</param>
        /// <param name="actionName">The name of the AIAction to lookup in the AIAgent's 'PossibleActions' dictionary</param>
        /// <param name="target">The GameObject that the client is telling the AIAction to target</param>
        /// <param name="targetPos">The Vector3 that the client is telling the AIAction to use</param>
        /// <param name="layer">The layer index that the client is telling the AIAction to use (this is used due to layer discrepancies from Server->Client)</param>
        /// <param name="addToList">Whether the AIAction should be added to the queue or nots</param>
        [Command]
        public void CmdAddAction(GameObject agent, string actionName, GameObject target, Vector3 targetPos, int layer, bool addToList)
        {
            if (Helper.IsNullOrDestroyed(agent))
            {
                DebugManager.WarningMessage("Attempting to add an AIAction to an AIAgent Server side!");
                return;
            }

            AIAgent ai = agent.GetComponent<AIAgent>();

            if (ai)
            {
                ai.AddAction(actionName, addToList, target, targetPos, layer);
            }
        }
        /// <summary>
        /// Send a command to the server to clear the current action and queue of an AIAgent
        /// </summary>
        /// <param name="agent">AIAgent to clear</param>
        [Command]
        public void CmdClearActions(GameObject agent)
        {
            if (Helper.IsNullOrDestroyed(agent))
            {
                DebugManager.WarningMessage("Attempting to add an AIAction to an AIAgent Server side!");
                return;
            }

            AIAgent ai = agent.GetComponent<AIAgent>();

            if (ai)
            {
                ai.ClearAllActions();
            }
        }

        /// <summary>
        /// Refresh the CurrentActionRef, and ActionQueueRef for an AIAgent
        /// </summary>
        /// <param name="agent">The AIAgent to refresh</param>
        /// <param name="currentAction">CurrentActionRef to set</param>
        /// <param name="actionQueue">ActionQueueRef to set</param>
        [Command]
        public void CmdRefreshAgentActions(GameObject agent, string currentAction, string[] actionQueue)
        {
            if (Helper.IsNullOrDestroyed(agent))
            {
                DebugManager.WarningMessage("Attempting to refresh actions on an AIAgent Server side!");
                return;
            }

            RpcRefreshAgentActions(agent, currentAction, actionQueue);
        }

        /// <summary>
        /// Refresh the CurrentActionRef, and ActionQueueRef for an AIAgent
        /// </summary>
        /// <param name="agent">The AIAgent to refresh</param>
        /// <param name="currentAction">CurrentActionRef to set</param>
        /// <param name="actionQueue">ActionQueueRef to set</param>
        [ClientRpc]
        public void RpcRefreshAgentActions(GameObject agent, string currentAction, string[] actionQueue)
        {
            if (Helper.IsNullOrDestroyed(agent))
            {
                DebugManager.WarningMessage("Attempting to refresh actions on an AIAgent Client side!");
                return;
            }

            AIAgent ai = agent.GetComponent<AIAgent>();

            if (ai)
            {
                ai.SetCurrentActionRef(currentAction);
                ai.SetActionQueueRef(actionQueue.ToList());

                ai.RefreshActionRefs(false);
            }
        }

        /// <summary>
        /// Spawn a GameObject on the server, propagates to all clients
        /// </summary>
        /// <param name="objToSpawn">GameObject to spawn, needs a NetworkIdentity and be apart of the SpawnablePrefabs list</param>
        [Server]
        public void ServSpawnObject(GameObject objToSpawn)
        {
            if (Helper.IsNullOrDestroyed(objToSpawn))
            {
                DebugManager.WarningMessage("Attempting to spawn a null GameObject Server side!");
                return;
            }

            NetworkServer.Spawn(objToSpawn);
        }
        /// <summary>
        /// Destroy a GameObject on the server, propagates to all clients
        /// </summary>
        /// <param name="objToDestroy">GameObject to destroy, needs a NetworkIdentity</param>
        [Server]
        public void ServDestroyObject(GameObject objToDestroy)
        {
            if (Helper.IsNullOrDestroyed(objToDestroy))
            {
                DebugManager.WarningMessage("Attempting to destroy a null GameObject Server side!");
            }

            NetworkServer.Destroy(objToDestroy);
        }

        /// <summary>
        /// Send a command to the server to inform an AIAgent that it has an owner
        /// </summary>
        /// <param name="agent">The AIAgent to update</param>
        /// <param name="owner">The new owner of the AIAgent</param>
        [Command]
        public void CmdSetAgentOwner(GameObject agent, GameObject owner)
        {
            if (Helper.IsNullOrDestroyed(agent) || Helper.IsNullOrDestroyed(owner))
            {
                DebugManager.WarningMessage("Failed to set the owner of an AIAgent Server side!");
                return;
            }

            // Tell all clients that the AIAgent has a new owner
            RpcSetAgentOwner(agent, owner);
        }
        /// <summary>
        /// Inform all clients that an AIAgent has been assigned an owner
        /// </summary>
        /// <param name="agent">THe AIAgent to update</param>
        /// <param name="owner">The new owner of the AIAgent</param>
        [ClientRpc]
        public void RpcSetAgentOwner(GameObject agent, GameObject owner)
        {
            if (Helper.IsNullOrDestroyed(agent) || Helper.IsNullOrDestroyed(owner))
            {
                DebugManager.WarningMessage("Failed to set the owner of an AIAgent Client side!");
                return;
            }

            AIAgent ai = Helper.GetComponent<AIAgent>(agent);
            NetworkPlayer player = Helper.GetComponent<NetworkPlayer>(owner);

            if (ai && player)
            {
                // Tell the AIAgent of its new owner
                ai.AgentOwner = player;

                // If the player that owns the AIAgent is the local player, then the AIAgent will be assigned to a 'friendly' layer
                if (player.isLocalPlayer)
                {
                    ai.gameObject.layer = LayerMask.NameToLayer(player.FriendlyLayer);
                }
                // If the player that owns the AIAgent is not the local player, then the AIAgent will be assigned to an 'enemy' layer
                else
                {
                    ai.gameObject.layer = LayerMask.NameToLayer(player.EnemyLayer);
                }
            }
        }
        #endregion
    }
}