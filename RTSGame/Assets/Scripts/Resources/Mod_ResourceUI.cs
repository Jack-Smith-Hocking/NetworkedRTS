using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Security.Policy;

namespace RTS_System
{
    public class Mod_ResourceUI : MonoBehaviour
    {
        public static Mod_ResourceUI Instance = null;

        [Tooltip("The prefab to display resources with, should have an Image and a TextMeshProUGUI")] public GameObject ResourceUIPrefab = null;
        [Tooltip("The canvas object that will manage all of the ResourceUIPrefabs spawned")] public GameObject ResourceUIOwner = null;

        private Dictionary<string, TextMeshProUGUI> resourceUIDict = new Dictionary<string, TextMeshProUGUI>();

        private GameObject resourcePrefab = null;
        private TextMeshProUGUI resourceText = null;
        private Image resourceImage = null;

        private void Awake()
        {
            Instance = this;
        }

        public void AddResourceUI(Mod_Resource resource)
        {
            if (!resource) return;

            if (!resourceUIDict.ContainsKey(resource.ResourceName))
            {
                if (ResourceUIPrefab && ResourceUIOwner)
                {
                    resourcePrefab = Instantiate(ResourceUIPrefab, ResourceUIOwner.transform);

                    resourceText = resourcePrefab.GetComponentInChildren<TextMeshProUGUI>();
                    resourceImage = resourcePrefab.GetComponentInChildren<Image>();

                    if (resourceText)
                    {
                        resourceUIDict.Add(resource.ResourceName, resourceText);
                    }
                    if (resourceImage)
                    {
                        resourceImage.sprite = resource.ResourceIcon;
                    }
                }
            }
        }

        public void UpdateResourceUI(Mod_Resource resource, int resourceCount)
        {
            if (!resource) return;

            if (resourceUIDict.ContainsKey(resource.ResourceName))
            {
                resourceUIDict[resource.ResourceName].text = $"{resource.ResourceName}: {resourceCount}";
            }
        }    
    }
}