using System.Collections;
using System.Collections.Generic;
using System.Security;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

namespace RTS_System
{
    [Serializable]
    public struct Mod_ResourceValue
    {
        [Tooltip("Ease of use in inspector")] public string ListIdentifier;
        [Tooltip("The type of resource that this relates to")] public RTS_System.Mod_Resource ResourceType;
        [Tooltip("The value of the resource, whether it adds or subtracts depends on DecreaseResource")] [Min(0)] public int RawValue;

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
        /// Gets invoked when the value of this Cache is changed
        /// </summary>
        public Action<int> OnValueChanged = null;

        /// <summary>
        /// The current resource value, can't be externally set
        /// </summary>
        public int CurrentResourceValue { get; private set; } = 0;
        
        private TextMeshProUGUI resourceText = null;

        public Mod_ResourceCache() { }

        /// <summary>
        /// Initialises a Mod_ResourcesCache with a text item
        /// </summary>
        /// <param name="text">The text to display the caches</param>
        /// <param name="resourceName">Name of the resource to display</param>
        public Mod_ResourceCache(TextMeshProUGUI text, string resourceName)
        {
            resourceText = text;

            if (resourceText)
            {
                OnValueChanged += (int val) =>
                {
                    resourceText.text = resourceName + ": " + val;
                };
            }
        }

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

                OnValueChanged?.Invoke(CurrentResourceValue);

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
            if (amount > 0)
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

    public class Mod_ResourceManager : MonoBehaviour
    {
        public static Mod_ResourceManager Instance = null;

        [Tooltip("The prefab to display resources with, should have an Image and a TextMeshProUGUI")] public GameObject ResourceUIPrefab = null;
        [Tooltip("The canvas object that will manage all of the ResourceUIPrefabs spawned")] public GameObject ResourceUIOwner = null;

        [Tooltip("All of the resources in the game")] public List<Mod_Resource> Resources = new List<Mod_Resource>();

        private GameObject resourcePrefab = null;
        private Image resourceImage = null;
        private TextMeshProUGUI resourceText = null;

        private Dictionary<Mod_Resource, Mod_ResourceCache> resourceCaches = new Dictionary<Mod_Resource, Mod_ResourceCache>();
        
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
                foreach (Mod_Resource resource in Resources)
                {
                    if (resourceCaches.ContainsKey(resource))
                    {
                        DebugManager.WarningMessage($"Resource of name '{resource.ResourceName}' already exists!");
                    }
                    // Add the resource to the dictionary if it isn't there already
                    else
                    {
                        cache = new Mod_ResourceCache();

                        // Instantiate prefab and get the Text and Image components from it
                        if (ResourceUIPrefab && ResourceUIOwner)
                        {
                            resourcePrefab = Instantiate(ResourceUIPrefab, ResourceUIOwner.transform);
                            resourcePrefab.name = $"Resource ({resource.ResourceName})";

                            resourceText = resourcePrefab.GetComponentInChildren<TextMeshProUGUI>();
                            resourceImage = resourcePrefab.GetComponentInChildren<Image>();

                            if (resourceText)
                            {
                                cache = new Mod_ResourceCache(resourceText, resource.ResourceName);
                            }
                            if (resourceImage)
                            {
                                resourceImage.sprite = resource.ResourceIcon;
                            }
                        }

                        // Set the initial value of the cache
                        cache.IncreaseValue(Mathf.Abs(resource.ResourceStartCount));

                        // Add resource cache to dictionary
                        resourceCaches.Add(resource, cache);
                    }
                }

                resourceImage = null;
                resourceText = null;
                resourcePrefab = null;
            }
        }

        /// <summary>
        /// Adds an amount of resources to a resource type
        /// </summary>
        /// <param name="resource">The type of resource to add to</param>
        /// <param name="amount">The amount to add</param>
        /// <returns>Whether the amount was successfully added</returns>
        public bool AddResource(Mod_Resource resource, int amount)
        {
            if (!resource) return false;

            if (resourceCaches.ContainsKey(resource))
            {
                return resourceCaches[resource].IncreaseValue(amount);
            }

            return false;
        }
        /// <summary>
        /// Add a Mod_ResourceValue to its respective resource type
        /// </summary>
        /// <param name="resourceValue">Mod_ResourceValue to add</param>
        /// <returns></returns>
        public bool AddResource(Mod_ResourceValue resourceValue)
        {
            if (!resourceValue.ResourceType) return false;

            if (resourceCaches.ContainsKey(resourceValue.ResourceType))
            {
                return resourceCaches[resourceValue.ResourceType].IncreaseValue(resourceValue.TrueValue);
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
        /// Checks if the player can afford something of type resource and cost value
        /// </summary>
        /// <param name="resource">Resource to check if can afford</param>
        /// <param name="value">The value to see if the player can afford</param>
        /// <returns></returns>
        public bool CanAfford(Mod_Resource resource, int value)
        {
            if (!resource) return false;

            // Checks if the resource is valid
            if (resourceCaches.ContainsKey(resource))
            {
                return resourceCaches[resource].CanAfford(value);
            }

            return false;
        }
        /// <summary>
        /// Checks if the player can afford a resource of a certain value
        /// </summary>
        /// <param name="resourceValue">Holds data for the resource and the value to add</param>
        /// <param name="trueCost">Whether to use the raw cost (>= 0) or the true cost (can be less than 0)</param>
        /// <returns></returns>
        public bool CanAfford(Mod_ResourceValue resourceValue, bool trueCost = true)
        {
            return CanAfford(resourceValue.ResourceType, (trueCost ? resourceValue.TrueValue : resourceValue.RawValue));
        }
    }
}
