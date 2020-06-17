using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTS_System.Resource
{
    public class Mod_ResourceUI : MonoBehaviour
    {
        public static Mod_ResourceUI Instance = null;

        [Tooltip("The prefab to display resources with, should have an Image and a TextMeshProUGUI")] public GameObject ResourceUIPrefab = null;
        [Tooltip("The canvas object that will manage all of the ResourceUIPrefabs spawned")] public GameObject ResourceUIOwner = null;

        private Dictionary<string, UIDisplay> resourceUIDict = new Dictionary<string, UIDisplay>();

        private GameObject resourcePrefab = null;
        private UIDisplay resourceDisplay = null;

        private void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// Add a new entry into the dictionary and create a new ResourceUIPrefab to manage
        /// </summary>
        /// <param name="resource">Resource to add</param>
        public void AddResourceUI(Mod_Resource resource)
        {
            if (!resource) return;

            // Check if already in the dictionary 
            if (!resourceUIDict.ContainsKey(resource.ResourceName))
            {
                // Check that the parent and prefab being used are valid
                if (ResourceUIPrefab && ResourceUIOwner)
                {
                    // Create a new prefab and set the parent
                    resourcePrefab = Instantiate(ResourceUIPrefab, ResourceUIOwner.transform);

                    resourceDisplay = Helper.GetComponent<UIDisplay>(resourcePrefab);

                    // Add the display to the dictionary
                    if (resourceDisplay)
                    {
                        resourceDisplay.InitialiseUI(resource.ResourceIcon, "");
                        resourceUIDict.Add(resource.ResourceName, resourceDisplay);
                    }
                }
            }
        }

        public void UpdateResourceUI(Mod_Resource resource, int resourceCount)
        {
            if (!resource) return;

            UpdateResourceUI(resource.ResourceName, resourceCount);
        }

        /// <summary>
        /// Update the text of an entry in the dictionary
        /// </summary>
        /// <param name="resource">Name of the resource to update</param>
        /// <param name="resourceCount">New value to display</param>
        public void UpdateResourceUI(string resource, int resourceCount)
        {
            // Check if in the dictionary
            if (resourceUIDict.ContainsKey(resource))
            {
                resourceUIDict[resource].UpdateText($"{resource}: {resourceCount}");
            }
        }

        /// <summary>
        /// Would attempt to update the display of all values in the SyncedResources dictionary
        /// </summary>
        /// <param name="resourceManager"></param>
        public void UpdateUI(Mod_ResourceManager resourceManager)
        {
            foreach (var elem in resourceManager.SyncedResources)
            {
                UpdateResourceUI(elem.Key, elem.Value.CurrentResourceValue);
            }
        }
    }
}