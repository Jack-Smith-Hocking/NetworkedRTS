using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTS_System
{
    [CreateAssetMenu(fileName = "New ResourceType", menuName = "ScriptableObject/RTS/Resources/ResourceType")]
    public class Mod_Resource : ScriptableObject
    {
        [Tooltip("The name to be displayed for this resource when appropriate")] public string ResourceName;
        [Tooltip("Used for things like health bars on Mod_ResourceCollections")] public Color ResourceColour = Color.white;
        [Tooltip("Icon to be displayed for this icon when appropriate")] public Sprite ResourceIcon;
        [Min(0)] [Tooltip("The starting amount for this resource")] public int ResourceStartCount = 0;

        public static Dictionary<string, Mod_Resource> ResourceTable = new Dictionary<string, Mod_Resource>();

        public void OnEnable()
        {
            // Create a look up table for all resources, filtering out clones
            if (!name.Contains("Clone") && !ResourceTable.ContainsKey(ResourceName))
            {
                ResourceTable.Add(ResourceName, this);
            }
        }
    }
}