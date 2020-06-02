using RTS_System.Resource;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace RTS_System.AI
{
    [Serializable]
    public abstract class BaseAction
    {
        public virtual bool HasCompleted(AIAgent agent) { return true; }

        public virtual float Evaluate(AIAgent agent) { return 0.0f; }
        public virtual void Execute(AIAgent agent) { }
        public virtual void Update(AIAgent agent) { }

        public virtual void Enter(AIAgent agent) { }
        public virtual void Exit(AIAgent agent) { }
        public virtual void Cancel(AIAgent agent) { }

        public virtual bool SetVariables(AIAgent agent, GameObject obj, Vector3 vec3) { return false; }
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
        public bool ValidTarget = false;

        public override bool HasCompleted(AIAgent agent)
        {
            if (agent.NavAgent)
            {
                return agent.NavAgent.remainingDistance <= StoppingDistance || !ValidTarget;
            }

            return Vector3.Distance(CurrentTarget, agent.transform.position) <= StoppingDistance || !ValidTarget;
        }

        public override void Execute(AIAgent agent)
        {
            if (agent && agent.NavAgent)
            {
                agent.NavAgent.SetDestination(CurrentTarget);
            }
        }

        public override void Enter(AIAgent agent)
        {
            if (agent && agent.NavAgent && agent.NavAgent.enabled)
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

        public override bool SetVariables(AIAgent agent, GameObject obj, Vector3 vec3)
        {
            if (obj)
            {
                if (Helper.IsInLayerMask(MovementLayers, obj.layer))
                {
                    CurrentTarget = vec3;
                    ValidTarget = true;
                }
            }
            else
            {
                CurrentTarget = vec3;
                ValidTarget = true;
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
            if (!Helper.IsNullOrDestroyed<Transform>(CurrentTarget))
            {
                return Helper.Distance(agent.transform, CurrentTarget) >= MaxFollowDistance;
            }

            return true;
        }

        public override void Update(AIAgent agent)
        {
            if (Helper.IsNullOrDestroyed<Transform>(CurrentTarget)) return;

            if (Time.time > CurrentRepositionTime)
            {
                CurrentRepositionTime = Time.time + RepositionTime;

                MoveToPoint.CurrentTarget = CurrentTarget.position;
                MoveToPoint.Execute(agent);
            }

            MoveToPoint.Update(agent);

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

        public override bool SetVariables(AIAgent agent, GameObject obj, Vector3 vec3)
        {
            bool valid = false;

            valid = MoveToPoint.SetVariables(agent, obj, vec3);

            if (valid)
            {
                CurrentTarget = obj.transform;
            }

            return valid;
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

            if (MoveToPoint.HasCompleted(agent) && PatrolPath.Count > 0)
            {
                if (CurrentPathIndex >= PatrolPath.Count)
                {
                    CurrentPathIndex = 0;
                    PatrolPath.Reverse();
                }

                MoveToPoint.CurrentTarget = PatrolPath[CurrentPathIndex];
                MoveToPoint.Execute(agent);

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

        public override bool SetVariables(AIAgent agent, GameObject obj, Vector3 vec3)
        {
            bool valid = false;

            if (agent)
            {
                valid = MoveToPoint.SetVariables(agent, obj, vec3);

                if (valid)
                {
                    PatrolPath.Clear();

                    PatrolPath.Add(vec3);
                    PatrolPath.Add(agent.transform.position);
                }
            }

            return valid;
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
        public float CurrentAttackDelay = 0;
        public Health TargetHealth = null;

        public override bool HasCompleted(AIAgent agent)
        {
            return !TargetHealth;
        }

        public override void Execute(AIAgent agent)
        {
            MoveToTarget.Execute(agent);
        }
        public override void Update(AIAgent agent)
        {
            MoveToTarget.Update(agent);
            AttemptAttack(agent);
        }
        void AttemptAttack(AIAgent agent)
        {
            if (Time.time >= CurrentAttackDelay && TargetHealth)
            {
                if (MoveToTarget.MoveToPoint.HasCompleted(agent))
                {
                    TargetHealth.TakeDamage(AttackDamage);

                    CurrentAttackDelay = Time.time + AttackDelay;
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
        }

        public override bool SetVariables(AIAgent agent, GameObject obj, Vector3 vec3)
        {
            bool validTarget = MoveToTarget.SetVariables(agent, obj, vec3);

            if (MoveToTarget.CurrentTarget)
            {
                TargetHealth = MoveToTarget.CurrentTarget.GetComponentInChildren<Health>();

                if (TargetHealth)
                {
                    TargetHealth.OnDeathEvent.AddListener(() => { TargetHealth = null; MoveToTarget.Exit(agent); });
                }
            }

            return TargetHealth != null && validTarget;
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
            return ((Building != null) && CanAffordBuilding) || CanAffordBuilding == false;
        }

        public override void Execute(AIAgent agent)
        {
            MoveToPoint.Execute(agent);
        }

        public override void Update(AIAgent agent)
        {
            // Start the building timer
            {
                if (MoveToPoint.HasCompleted(agent) && !AttemptBuild)
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
                    if (TempIndicator)
                    {
                        GameObject.Destroy(TempIndicator);
                    }

                    Building = GameObject.Instantiate(BuildingPrefab, MoveToPoint.CurrentTarget, Quaternion.identity);
                    agent.AgentOwner.PlayerSelector.ServSpawnObject(Building);

                    agent.AgentOwner.PlayerSelector.RpcSetAgentOwner(Building, agent.AgentOwner.gameObject);
                    //AIAgent buildingAgent = Helper.GetComponent<AIAgent>(Building);
                    //if (buildingAgent)
                    //{
                    //    buildingAgent.AgentOwner = agent.AgentOwner;
                    //}

                    AttemptBuild = false;
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
                Helper.LoopList_ForEach<Mod_ResourceValue>(BuildingCost.ResourceCosts, (Mod_ResourceValue rv) =>
                {
                    agent.AgentOwner.PlayerResourceManager.ServAddToSynceDict(rv.ResourceType.ResourceName, rv.TrueValue * -1);
                });
            }

            if (TempIndicator)
            {
                agent.AgentOwner.PlayerSelector.ServDestroyObject(TempIndicator);
                //GameObject.Destroy(TempIndicator);
            }

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
                    Helper.LoopList_ForEach<Mod_ResourceValue>(BuildingCost.ResourceCosts, (Mod_ResourceValue rv) =>
                    {
                        agent.AgentOwner.PlayerResourceManager.ServAddToSynceDict(rv.ResourceType.ResourceName, rv.TrueValue);
                    });

                    // Create a temporary object at the location to indicate a building will be made there
                    if (BuildingIndicator)
                    {
                        TempIndicator = GameObject.Instantiate(BuildingIndicator, MoveToPoint.CurrentTarget, Quaternion.identity);
                        agent.AgentOwner.PlayerSelector.ServSpawnObject(TempIndicator);
                    }
                }
            }
        }

        public override bool SetVariables(AIAgent agent, GameObject obj, Vector3 vec3)
        {
            bool valid = MoveToPoint.SetVariables(agent, obj, vec3);

            if (valid && (!StopDistanceAsSpawnDist || MoveToPoint.HasCompleted(agent)))
            {
                BuyBuilding(agent);
            }

            return valid;
        }
    }
    #endregion
}