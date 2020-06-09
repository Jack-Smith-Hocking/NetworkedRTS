using RTS_System.Resource;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;

namespace RTS_System.AI
{
    [Serializable]
    public abstract class BaseAction
    {
        [Header("BaseAction DebugInfo")]
        public bool ValidTarget = false;

        /// <summary>
        /// Whether the action has completed itself yet or not
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        public virtual bool HasCompleted(AIAgent agent) { return true; }

        /// <summary>
        /// How important this action is given its own context
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        public virtual float Evaluate(AIAgent agent) { return 0.0f; }
        /// <summary>
        /// Forces the action to execute itself
        /// </summary>
        /// <param name="agent"></param>
        public virtual void Execute(AIAgent agent) { }
        /// <summary>
        /// Updates the action and its variables
        /// </summary>
        /// <param name="agent"></param>
        public virtual void Update(AIAgent agent) { }

        /// <summary>
        /// Do any necessary operations before executing the action
        /// </summary>
        /// <param name="agent"></param>
        public virtual void Enter(AIAgent agent) { }
        /// <summary>
        /// Do any necessary operations before exiting out of the action
        /// </summary>
        /// <param name="agent"></param>
        public virtual void Exit(AIAgent agent) { }
        /// <summary>
        /// Do any necessary operations before removing action from any list
        /// </summary>
        /// <param name="agent"></param>
        public virtual void Cancel(AIAgent agent) { }

        /// <summary>
        /// Set up all necessary data
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="obj"></param>
        /// <param name="vec3"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public virtual bool SetVariables(AIAgent agent, GameObject obj, Vector3 vec3, int num) { return false; }
    }

    #region MoveToPoint
    [Serializable]
    public class MoveToPoint : BaseAction
    {
        [Header("MoveToPoint Data")]
        [Tooltip("Valid layers that can be moved on")] public LayerMask MovementLayers;
        [Tooltip("How far away from target the target before stopping")] public float StoppingDistance = 10;

        [Header("MoveToPoint Debug Info")]
        public Vector3 CurrentTarget;

        public override bool HasCompleted(AIAgent agent)
        {
            // Check if the AIAgent has a NavMeshAgent
            if (agent.NavAgent)
            {
                // Check if the AIAgent is close enough to its target, or if there is no ValidTarget
                return agent.NavAgent.remainingDistance <= StoppingDistance || !ValidTarget;
            }

            // If there is no NavMeshAgent then a manual check between the target and AIAgent position
            return Vector3.Distance(CurrentTarget, agent.transform.position) <= StoppingDistance || !ValidTarget;
        }

        public override void Execute(AIAgent agent)
        {
            // Update the NavMeshAgent's destination 
            if (agent.NavAgent)
            {
                agent.NavAgent.SetDestination(CurrentTarget);
            }
        }

        public override void Enter(AIAgent agent)
        {
            // Reset the current path and update the stopping distance of the NavMeshAgent
            if (agent.NavAgent && agent.NavAgent.isActiveAndEnabled)
            {
                agent.NavAgent.ResetPath();
                agent.NavAgent.stoppingDistance = StoppingDistance;
            }
        }
        public override void Exit(AIAgent agent)
        {
            Enter(agent);

            ValidTarget = false;
        }

        public override bool SetVariables(AIAgent agent, GameObject obj, Vector3 vec3, int num)
        {
            ValidTarget = false;

            // If a GameObject was passed through, then check if it's in the right layer
            if (obj)
            {
                if (Helper.IsInLayerMask(MovementLayers, num))
                {
                    CurrentTarget = vec3;
                    ValidTarget = true;
                }
            }

            return ValidTarget;
        }
    }
    #endregion

    #region MoveToTarget
    [Serializable]
    public class MoveToTarget : BaseAction
    {
        [Header("MoveToPoint")]
        public MoveToPoint MoveToPoint = null;

        [Header("MoveToTarget Data")]
        [Tooltip("The maximum distance that the target will be followed")] public float MaxFollowDistance = 0;
        [Tooltip("The amount of time between each time the target position will be updated")] public float RepositionTime = 1;

        [Header("MoveToTarget Debug Info")]
        public Transform CurrentTarget = null;
        public float CurrentRepositionTime = 0;

        public override bool HasCompleted(AIAgent agent)
        {
            // If there is a current target, then check if it is too far away from it
            if (!Helper.IsNullOrDestroyed<Transform>(CurrentTarget))
            {
                return Helper.Distance(agent.transform, CurrentTarget) >= MaxFollowDistance || !ValidTarget;
            }

            return !ValidTarget;
        }

        public override void Update(AIAgent agent)
        {
            if (Helper.IsNullOrDestroyed<Transform>(CurrentTarget)) return;

            // Check if it is time to update the NavMeshAgent's destination
            if (Time.time > CurrentRepositionTime)
            {
                CurrentRepositionTime = Time.time + RepositionTime;

                // Reset target position
                MoveToPoint.CurrentTarget = CurrentTarget.position;
                MoveToPoint.Execute(agent);
            }

            MoveToPoint.Update(agent);

            // If the AIAgent has reached it's destination and the NavMeshAgent is not calculating a path
            // Then the current path will be reset
            if (!agent.NavAgent.pathPending && MoveToPoint.HasCompleted(agent))
            {
                agent.NavAgent.ResetPath();
            }
        }

        public override void Execute(AIAgent agent)
        {
            MoveToPoint.Execute(agent);
        }

        public override void Enter(AIAgent agent)
        {
            MoveToPoint.Enter(agent);

            // Update the current target position
            if (!Helper.IsNullOrDestroyed<Transform>(CurrentTarget))
            {
                MoveToPoint.CurrentTarget = CurrentTarget.position;
            }
        }

        public override void Exit(AIAgent agent)
        {
            MoveToPoint.Exit(agent);

            CurrentTarget = null;
        }

        public override bool SetVariables(AIAgent agent, GameObject obj, Vector3 vec3, int num)
        {
            ValidTarget = false;

            // Check if a valid target was found
            ValidTarget = MoveToPoint.SetVariables(agent, obj, vec3, num);
            ValidTarget = obj && ValidTarget;

            if (ValidTarget)
            {
                CurrentTarget = obj.transform;
            }

            return ValidTarget;
        }
    }
    #endregion

    #region PatrolPoints
    [Serializable]
    public class PatrolPoints : BaseAction
    {
        [Header("MoveToPoint")]
        public MoveToPoint MoveToPoint = null;

        [Header("PatrolPoints Debug Info")]
        public List<Vector3> PatrolPath = new List<Vector3>();
        public int CurrentPathIndex = 0;

        public override void Update(AIAgent agent)
        {
            MoveToPoint.Update(agent);

            // If the current target has been reached then update the path
            if (MoveToPoint.HasCompleted(agent) && PatrolPath.Count > 0)
            {
                // If the CurrentPathIndex is less then the total path count, use the next position in the path
                // Else, reverse the path and use the first position in the path
                if (CurrentPathIndex >= PatrolPath.Count)
                {
                    CurrentPathIndex = 0;
                    PatrolPath.Reverse();
                }

                // Update the current target
                MoveToPoint.CurrentTarget = PatrolPath[CurrentPathIndex];
                MoveToPoint.Execute(agent);

                // Update path index for next target
                CurrentPathIndex++;
            }
        }

        public override void Execute(AIAgent agent)
        {
            MoveToPoint.Execute(agent);
        }

        public override void Enter(AIAgent agent)
        {
            MoveToPoint.Enter(agent);
        }
        public override void Exit(AIAgent agent)
        {
            MoveToPoint.Exit(agent);
        }

        public override bool SetVariables(AIAgent agent, GameObject obj, Vector3 vec3, int num)
        {
            ValidTarget = false;

            if (agent)
            {
                // Check if a valid target was found
                ValidTarget = MoveToPoint.SetVariables(agent, obj, vec3, num);

                // If a valid target was found, then add vec3 and the position of 
                // The AIAgent to the PatrolPath
                if (ValidTarget)
                {
                    PatrolPath.Clear();

                    PatrolPath.Add(vec3);
                    PatrolPath.Add(agent.transform.position);
                }
            }

            return ValidTarget;
        }
    }
    #endregion

    #region DirectedAttack
    [Serializable]
    public class DirectedAttack : BaseAction
    {
        [Header("MoveToTarget")]
        public MoveToTarget MoveToTarget = null;

        [Header("DirectedAttack Data")]
        [Tooltip("How much damage will be done per attack")] public float AttackDamage = 0;
        [Tooltip("Delay between attacks")] public float AttackDelay = 0;

        [Header("DirectedAttack Debug Info")]
        //public float CurrentAttackDelay = 0;
        public Health TargetHealth = null;

        public override bool HasCompleted(AIAgent agent)
        {
            return !TargetHealth || !ValidTarget;
        }

        public override void Execute(AIAgent agent)
        {
            MoveToTarget.Execute(agent);

            agent.ActionTimer.SetTimer(agent.name + "_DirectedAttack", 0);
        }
        public override void Update(AIAgent agent)
        {
            MoveToTarget.Update(agent);
            AttemptAttack(agent);
        }

        /// <summary>
        /// Attempt an attack on the current target
        /// </summary>
        /// <param name="agent"></param>
        void AttemptAttack(AIAgent agent)
        {
            if (agent.ActionTimer.CheckTimer(agent.name + "_DirectedAttack", Time.time) && TargetHealth)
            {
                // If close enough to the current target, make an attack
                if (MoveToTarget.MoveToPoint.HasCompleted(agent))
                {
                    TargetHealth.RpcTakeDamageSimple(AttackDamage);

                    agent.ActionTimer.SetTimer(agent.name + "_DirectedAttack", Time.time + AttackDelay, true);
                }
            }
        }

        public override void Enter(AIAgent agent)
        {
            MoveToTarget.Enter(agent);
        }
        public override void Exit(AIAgent agent)
        {
            MoveToTarget.Exit(agent);

            Cancel(agent);
        }

        public override void Cancel(AIAgent agent)
        {
            // Safely remove listeners from the TargetHealth's OnDeathEvent
            if (TargetHealth)
            {
                TargetHealth.OnDeathEvent.RemoveListener(() => { TargetHealth = null; MoveToTarget.Exit(agent); });
            }
        }

        public override bool SetVariables(AIAgent agent, GameObject obj, Vector3 vec3, int num)
        {
            // Check if a valid target was detected
            ValidTarget = MoveToTarget.SetVariables(agent, obj, vec3, num);

            // If there was a valid target then see if the target has a Health script attached to it
            if (MoveToTarget.CurrentTarget)
            {
                TargetHealth = Helper.GetComponent<Health>(MoveToTarget.CurrentTarget.gameObject);

                // When the target dies then exit this action
                if (TargetHealth)
                {
                    TargetHealth.OnDeathEvent.AddListener(() => { TargetHealth = null; MoveToTarget.Exit(agent); });
                }
            }

            return TargetHealth != null && ValidTarget;
        }
    }
    #endregion

    #region CreateBuilding
    [Serializable]
    public class CreateBuilding : BaseAction
    {
        [Header("MoveToPoint")]
        public MoveToPoint MoveToPoint = null;

        [Header("CreateBuilding Data")]
        [Tooltip("The building that will be created")] public GameObject BuildingPrefab = null;
        [Tooltip("The object to display at the building location")] public GameObject BuildingIndicator = null;
        [Space]
        [Tooltip("The cost of the building")] public ResourceConditional BuildingCost = null;
        [Tooltip("How long the building will take to build")] public float BuildCompletionTime = 0;
        [Space]
        [Tooltip("Whether to use the 'MoveToPoint.StoppingDistance' as a max spawn distance")] public bool StopDistanceAsSpawnDist = false;

        [Header("CreateBuilding Debug Info")]
        public GameObject Building = null;
        public GameObject TempIndicator = null;
        public float CurrentBuildTime = 0;
        public bool CanAffordBuilding = false;
        public bool AttemptBuild = false;

        public override bool HasCompleted(AIAgent agent)
        {
            // Return false if there is no Building
            // Return false if the Building couldn't be afforded
            // Return false if there is no ValidTarget
            // Otherwise, return true
            return ((Building != null) && CanAffordBuilding) || CanAffordBuilding == false || !ValidTarget;
        }

        public override void Execute(AIAgent agent)
        {
            MoveToPoint.Execute(agent);
        }

        public override void Update(AIAgent agent)
        {
            // Start the building timer
            {
                if ((MoveToPoint.HasCompleted(agent) || StopDistanceAsSpawnDist) && !AttemptBuild)
                {
                    CurrentBuildTime = Time.time + BuildCompletionTime;

                    AttemptBuild = CanAffordBuilding;

                    MoveToPoint.Exit(agent);
                }
            }

            // Build the building
            {
                if (BuildingPrefab && Time.time >= CurrentBuildTime && AttemptBuild)
                {
                    // Destroy the TempIndicator across the server
                    if (TempIndicator)
                    {
                        agent.AgentOwner.PlayerSelector.ServDestroyObject(TempIndicator);
                    }
                    // Instantiate Building then spawn it across the server
                    Building = GameObject.Instantiate(BuildingPrefab, MoveToPoint.CurrentTarget, Quaternion.identity);
                    agent.AgentOwner.PlayerSelector.ServSpawnObject(Building);

                    // Tell the Building who its owner is
                    agent.AgentOwner.PlayerSelector.RpcSetAgentOwner(Building, agent.AgentOwner.gameObject);

                    AttemptBuild = false;
                }
                else if (!BuildingPrefab)
                {
                    DebugManager.WarningMessage($"Attempting to build a null building from agent: {agent.gameObject.name}");
                }

            }

            MoveToPoint.Update(agent);
        }

        public override void Enter(AIAgent agent)
        {
            MoveToPoint.Enter(agent);
        }
        public override void Exit(AIAgent agent)
        {
            MoveToPoint.Exit(agent);

            Cancel(agent);
        }
        public override void Cancel(AIAgent agent)
        {
            // If the action was cancelled before the building was built then resources will be refunded 
            if (CanAffordBuilding && BuildingCost && Helper.IsNullOrDestroyed(Building))
            {
                Helper.LoopList_ForEach<Mod_ResourceValue>(BuildingCost.ResourceCosts,
                // Loop action
                (Mod_ResourceValue rv) =>
                {
                    agent.AgentOwner.PlayerResourceManager.ServAddToSynceDict(rv.ResourceType.ResourceName, rv.TrueValue * -1);
                });
            }

            // Destroy the TempIndicator across the server
            if (TempIndicator)
            {
                agent.AgentOwner.PlayerSelector.ServDestroyObject(TempIndicator);
            }

            // Reset data
            TempIndicator = null;
            Building = null;
            CurrentBuildTime = 0;
            AttemptBuild = false;
            CanAffordBuilding = false;
        }

        void BuyBuilding(AIAgent agent)
        {
            // Check if the building is affordable
            if (BuildingCost)
            {
                BuildingCost = GameObject.Instantiate(BuildingCost);
                BuildingCost.ResourceManager = agent.AgentOwner.PlayerResourceManager;

                CanAffordBuilding = BuildingCost.EvaluateConditional();

                if (CanAffordBuilding)
                {
                    Helper.LoopList_ForEach<Mod_ResourceValue>(BuildingCost.ResourceCosts,
                    // Loop action
                    (Mod_ResourceValue rv) =>
                    {
                        agent.AgentOwner.PlayerResourceManager.ServAddToSynceDict(rv.ResourceType.ResourceName, rv.TrueValue);
                    });

                    // Create a temporary object at the location to indicate a building will be made there and then spawn it on the server
                    if (BuildingIndicator)
                    {
                        TempIndicator = GameObject.Instantiate(BuildingIndicator, MoveToPoint.CurrentTarget, Quaternion.identity);
                        agent.AgentOwner.PlayerSelector.ServSpawnObject(TempIndicator);
                    }
                }
                else
                {
                    ValidTarget = false;
                }
            }
        }

        public override bool SetVariables(AIAgent agent, GameObject obj, Vector3 vec3, int num)
        {
            // Check if a valid target was detected
            ValidTarget = MoveToPoint.SetVariables(agent, obj, vec3, num);

            // if there was a valid target and the spawn location is in range, attempt to buy it
            ValidTarget = ValidTarget && (!StopDistanceAsSpawnDist || MoveToPoint.HasCompleted(agent));
            
            if (ValidTarget)
            {
                BuyBuilding(agent);
            }

            return ValidTarget;
        }
    }
    #endregion

    #region CollectResource
    [Serializable]
    public class CollectResource : BaseAction
    {
        [Header("MoveToTarget")]
        public MoveToTarget MoveToTarget = null;

        [Header("CollectResource Data")]
        [Tooltip("How much of the resource will be collected each cycle")] public int CollectionAmount = 50;
        [Tooltip("The time before resource collection")] public float CollectionTime = 1;
        [Tooltip("Whether the action will be marked as complete when the resource pile is empty")] public bool CompleteOnEmpty = true;

        [Header("CollectionResource Debug Info")]
        public Mod_ResourceCollection ResourcePile = null;
        public Mod_ResourceValue CollectedResource;

        public override bool HasCompleted(AIAgent agent)
        {
            if (ResourcePile)
            {
                return (ResourcePile.IsEmpty && CompleteOnEmpty) || !ValidTarget;
            }

            return !ValidTarget;
        }

        public override void Execute(AIAgent agent)
        {
            MoveToTarget.Execute(agent);

            agent.ActionTimer.SetTimer(agent.name + "_CollectResource", 0);
        }

        public override void Update(AIAgent agent)
        {
            // If close enough to the ResourcePile and it's time to collect, then attempt to collect
            if (MoveToTarget.MoveToPoint.HasCompleted(agent) && agent.ActionTimer.CheckTimer(agent.name + "_CollectResource", Time.time))
            {
                if (ResourcePile)
                {
                    // Collect resources from the ResourcePile
                    CollectedResource = ResourcePile.CollectResources(CollectionAmount);

                    // Sync the owner's ResourceManager
                    agent.AgentOwner.PlayerResourceManager.ServAddToSynceDict(CollectedResource.ResourceType.ResourceName, CollectedResource.RawValue);

                    agent.ActionTimer.SetTimer(agent.name + "_CollectResource", Time.time + CollectionTime, true);
                }
            }

            MoveToTarget.Update(agent);
        }

        public override void Enter(AIAgent agent)
        {
            MoveToTarget.Enter(agent);
        }
        public override void Exit(AIAgent agent)
        {
            MoveToTarget.Exit(agent);
        }

        public override bool SetVariables(AIAgent agent, GameObject obj, Vector3 vec3, int num)
        {
            // Check if a valid target was detected
            ValidTarget = MoveToTarget.SetVariables(agent, obj, vec3, num);

            if (ValidTarget)
            {
                // Check if the target has a Mod_ResourceCollection
                ResourcePile = Helper.GetComponent<Mod_ResourceCollection>(obj);

                // If there is no Mod_ResourcePile then the action is not valid
                ValidTarget = ResourcePile;
            }

            return ValidTarget;
        }
    }
    #endregion
}