using System.Collections;
using System.Collections.Generic;
using System.Security;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using Mirror;

namespace RTS_System.Resource
{
    [Serializable]
    public struct Mod_ResourceValue
    {
        [Tooltip("Ease of use in inspector")] public string ListIdentifier;
        [Tooltip("The type of resource.ResourceName that this relates to")] public Mod_Resource ResourceType;
        [Tooltip("The value of the resource.ResourceName, whether it adds or subtracts depends on DecreaseResource")] [Min(0)] public int RawValue;

        [Tooltip("True = Value will be subtracted from player total, false = add to player total")] public bool DecreaseResource;

        public int TrueValue
        {
            get
            {
                return (RawValue * (DecreaseResource ? -1 : 1));
            }
        }

        /// <summary>
        /// Whether or not the player can afford this
        /// </summary>
        /// <returns></returns>
        public bool CanAfford(Mod_ResourceManager manager)
        {
            return manager.CanAfford(ResourceType, TrueValue);
        }
    }

    public class Mod_ResourceCache
    {
        /// <summary>
        /// The current resource.ResourceName value, can't be externally set
        /// </summary>
        [SyncVar]
        public int CurrentResourceValue = 0;

        /// <summary>
        /// Will check if amount can be added, if result is less than zero it will fail otherwise amount will be added
        /// </summary>
        /// <param name="amount">The amount to add</param>
        /// <returns>Whether or not the amount was successfully added</returns>
        public bool IncreaseValue(int amount)
        {
            // Checks whether the amount can be afforded
            if (CanAfford(amount))
            {
                CurrentResourceValue += amount;

                // Mod_ResourceUI.Instance.UpdateResourceUI(resource, CurrentResourceValue);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Whether the amount can be afforded for this cache
        /// </summary>
        /// <param name="amount">The amount to check</param>
        /// <returns></returns>
        public bool CanAfford(int amount)
        {
            if (amount >= 0)
            {
                return true;
            }

            if (amount < 0)
            {
                return ((amount + CurrentResourceValue) >= 0);
            }

            return false;
        }
    }

    public class Mod_ResourceManager : NetworkBehaviour
    {
        [Tooltip("All of the resources in the game")] public List<Mod_Resource> Resources = new List<Mod_Resource>();

        public Dictionary<string, Mod_ResourceCache> SyncedResources = new Dictionary<string, Mod_ResourceCache>();

        // Start is called before the first frame update
        void Start()
        {
            if (isLocalPlayer)
            {
                // Loop through all the resources in the game and spawn prefabs for them
                Helper.LoopList_ForEach<Mod_Resource>(Resources, (Mod_Resource resource) =>
                {
                    if (SyncedResources.ContainsKey(resource.ResourceName))
                    {
                        DebugManager.WarningMessage($"Resource of name '{resource.ResourceName}' already exists!");
                    }
                    // Add the resource.ResourceName to the dictionary if it isn't there already
                    else
                    {
                        Mod_ResourceUI.Instance.AddResourceUI(resource);

                        // Add resource cache to dictionary
                        AddResource(resource, resource.ResourceStartCount, true);
                        //CmdAddToSyncDict(resource.ResourceName, resource.ResourceStartCount);
                    }
                });
            }
        }

        public void UpdateUI(string key, int value)
        {
            Mod_ResourceUI.Instance.UpdateResourceUI(key, value);
        }

        /// <summary>
        /// Adds an amount of resources to a resource.ResourceName type
        /// </summary>
        /// <param name="resource">The type of resource.ResourceName to add to</param>
        /// <param name="amount">The amount to add</param>
        /// <returns>Whether the amount was successfully added</returns>
        public bool AddResource(Mod_Resource resource, int amount, bool overwrite = false)
        {
            if (!resource) return false;

            if (SyncedResources.ContainsKey(resource.ResourceName) || overwrite)
            {
                if (!isServer)
                {
                    CmdAddToSyncDict(resource.ResourceName, amount);
                }
                else
                { 
                    ServAddToSynceDict(resource.ResourceName, amount);
                }
            }

            return false;
        }
        /// <summary>
        /// Add a Mod_ResourceValue to its respective resource.ResourceName type
        /// </summary>
        /// <param name="resourceValue">Mod_ResourceValue to add</param>
        /// <returns></returns>
        public bool AddResource(Mod_ResourceValue resourceValue)
        {
            return AddResource(resourceValue.ResourceType, resourceValue.TrueValue);
        }

        /// <summary>
        /// Checks if the player can afford something of type resource.ResourceName and cost value
        /// </summary>
        /// <param name="resource">Resource to check if can afford</param>
        /// <param name="value">The value to see if the player can afford</param>
        /// <returns></returns>
        public bool CanAfford(Mod_Resource resource, int value)
        {
            if (!resource) return false;

            // Checks if the resource.ResourceName is valid
            if (SyncedResources.ContainsKey(resource.ResourceName))
            {
                return SyncedResources[resource.ResourceName].CanAfford(value);
            }

            return false;
        }
        /// <summary>
        /// Checks if the player can afford a resource.ResourceName of a certain value
        /// </summary>
        /// <param name="resourceValue">Holds data for the resource.ResourceName and the value to add</param>
        /// <param name="trueCost">Whether to use the raw cost (>= 0) or the true cost (can be less than 0)</param>
        /// <returns></returns>
        public bool CanAfford(Mod_ResourceValue resourceValue, bool trueCost = true)
        {
            return CanAfford(resourceValue.ResourceType, (trueCost ? resourceValue.TrueValue : resourceValue.RawValue));
        }

        #region NetworkFunctions
        /// <summary>
        /// Tell all clients to update their resource count
        /// </summary>
        /// <param name="resourceName">Name of resource to set</param>
        /// <param name="resourceAmount">Amount to set to</param>
        [ClientRpc]
        public void RpcSetResource(string resourceName, int resourceAmount)
        {
            if (isLocalPlayer)
            {
                // If the resource is in the dictionary then set it to the new value
                if (SyncedResources.ContainsKey(resourceName))
                {
                    SyncedResources[resourceName].CurrentResourceValue = resourceAmount;
                }
                // Else just add a new entry to the dictionary
                else
                {
                    Mod_ResourceCache cache = new Mod_ResourceCache();
                    cache.CurrentResourceValue = resourceAmount;

                }

                // Uodate the UI
                UpdateUI(resourceName, resourceAmount);
            }
        }

        /// <summary>
        /// Tell the server to add a new entry to the synced dictionary
        /// </summary>
        /// <param name="resourceName">The name of the resource to add</param>
        /// <param name="resourceAmount"></param>
        [Command]
        public void CmdAddToSyncDict(string resourceName, int resourceAmount)
        {
            AddToSynceDict(resourceName, resourceAmount);
        }

        /// <summary>
        /// Have the server (this) add a new entry to the resource dictionary
        /// </summary>
        /// <param name="resourceName">Name of the resource to add</param>
        /// <param name="resourceAmount">Amount of resource to make it</param>
        [Server]
        public void ServAddToSynceDict(string resourceName, int resourceAmount)
        {
            AddToSynceDict(resourceName, resourceAmount);
        }

        /// <summary>
        /// Add a new entry to the resource dictionary
        /// </summary>
        /// <param name="resourceName">Name of resource to add</param>
        /// <param name="resourceAmount">Amount of resource to add</param>
        public void AddToSynceDict(string resourceName, int resourceAmount)
        {
            bool canAfford = false;

            Mod_ResourceCache cache = new Mod_ResourceCache();
            // Check if the resource is in the dictionary
            if (!SyncedResources.ContainsKey(resourceName))
            {
                canAfford = cache.IncreaseValue(resourceAmount);
            }
            // Check if the increase/decrease can be afforded 
            else
            {
                canAfford = SyncedResources[resourceName].IncreaseValue(resourceAmount);
                cache.CurrentResourceValue = SyncedResources[resourceName].CurrentResourceValue;
            }

            // Tell all clients to update their dictionaries
            if (canAfford)
            {
                SyncedResources[resourceName] = cache;
                RpcSetResource(resourceName, cache.CurrentResourceValue);
            }
        }
        #endregion
    }
}
