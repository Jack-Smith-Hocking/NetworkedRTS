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
        [Tooltip("The type of resource.ResourceName that this relates to")] public RTS_System.Mod_Resource ResourceType;
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
        public bool CanAfford()
        {
            return Mod_ResourceManager.Instance.CanAfford(ResourceType, TrueValue);
        }
    }

    public class Mod_ResourceCache
    {
        /// <summary>
        /// The current resource.ResourceName value, can't be externally set
        /// </summary>
        public int CurrentResourceValue { get; private set; } = 0;
        
        /// <summary>
        /// Will check if amount can be added, if result is less than zero it will fail otherwise amount will be added
        /// </summary>
        /// <param name="amount">The amount to add</param>
        /// <returns>Whether or not the amount was successfully added</returns>
        public bool IncreaseValue(Mod_Resource resource, int amount)
        {
            // Checks whether the amount can be afforded
            if (CanAfford(amount))
            {
                CurrentResourceValue += amount;

                Mod_ResourceUI.Instance.UpdateResourceUI(resource, CurrentResourceValue);

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

    [Serializable]
    public class SyncResourceDictionary : SyncDictionary<string, Mod_ResourceCache> { }

    public class Mod_ResourceManager : NetworkBehaviour
    {
        public static Mod_ResourceManager Instance = null;

        [Tooltip("All of the resources in the game")] public List<Mod_Resource> Resources = new List<Mod_Resource>();

        //private Dictionary<Mod_Resource, Mod_ResourceCache> syncedResources = new Dictionary<Mod_Resource, Mod_ResourceCache>();

        private SyncResourceDictionary syncedResources = new SyncResourceDictionary();

        // Start is called before the first frame update
        void Start()
        {
            // Create singleton
            if (!Instance) Instance = this;

            // If this is the singleton then set up resources
            if (Instance.Equals(this))
            {
                Mod_ResourceCache cache = null;

                // Loop through all the resources in the game and spawn prefabs for them
                Helper.LoopList_ForEach<Mod_Resource>(Resources, (Mod_Resource resource) =>
                {
                    if (syncedResources.ContainsKey(resource.ResourceName))
                    {
                        DebugManager.WarningMessage($"Resource of name '{resource.ResourceName}' already exists!");
                    }
                    // Add the resource.ResourceName to the dictionary if it isn't there already
                    else
                    {
                        cache = new Mod_ResourceCache();
                        cache.IncreaseValue(resource, resource.ResourceStartCount);

                        Mod_ResourceUI.Instance.AddResourceUI(resource);

                        // Add resource.ResourceName cache to dictionary
                        syncedResources.Add(resource.ResourceName, cache);
                    }
                });
            }
        }

        /// <summary>
        /// Adds an amount of resources to a resource.ResourceName type
        /// </summary>
        /// <param name="resource">The type of resource.ResourceName to add to</param>
        /// <param name="amount">The amount to add</param>
        /// <returns>Whether the amount was successfully added</returns>
        public bool AddResource(Mod_Resource resource, int amount)
        {
            if (!resource) return false;

            if (syncedResources.ContainsKey(resource.ResourceName))
            {
                return syncedResources[resource.ResourceName].IncreaseValue(resource, amount);
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
            if (!resourceValue.ResourceType) return false;

            if (syncedResources.ContainsKey(resourceValue.ResourceType.ResourceName))
            {
                return syncedResources[resourceValue.ResourceType.ResourceName].IncreaseValue(resourceValue.ResourceType, resourceValue.TrueValue);
            }

            return false;
        }

        /// <summary>
        /// Adds a list of Mod_ResourceValues to their respective caches
        /// </summary>
        /// <param name="resourceValues">Mod_ResourceValue to add</param>
        /// <returns></returns>
        public bool AddResources(List<Mod_ResourceValue> resourceValues)
        {
            bool canAfford = false;
            Helper.LoopList_ForEach<Mod_ResourceValue>(resourceValues, (Mod_ResourceValue rc) => { canAfford = CanAfford(rc); }, () => { return !canAfford; });

            if (canAfford)
            {
                Helper.LoopList_ForEach<Mod_ResourceValue>(resourceValues, (Mod_ResourceValue rc) => { AddResource(rc); });

                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if the player can afford something of type resource.ResourceName and cost value
        /// </summary>
        /// <param name="resource.ResourceName">Resource to check if can afford</param>
        /// <param name="value">The value to see if the player can afford</param>
        /// <returns></returns>
        public bool CanAfford(Mod_Resource resource, int value)
        {
            if (!resource) return false;

            // Checks if the resource.ResourceName is valid
            if (syncedResources.ContainsKey(resource.ResourceName))
            {
                return syncedResources[resource.ResourceName].CanAfford(value);
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
    }
}
