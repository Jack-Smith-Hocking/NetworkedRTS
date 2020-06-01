using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTS_System
{
    [CreateAssetMenu(fileName = "New ResourceType", menuName = "ScriptableObject/RTS/Resources/ResourceType")]
    public class Mod_Resource : ScriptableObject
    {
        public string ResourceName;
        public Sprite ResourceIcon;
        [Min(0)] public int ResourceStartCount = 0;

        public static Dictionary<string, Mod_Resource> ResourceTable = new Dictionary<string, Mod_Resource>();

        public void OnEnable()
        {
            if (!name.Contains("Clone") && !ResourceTable.ContainsKey(ResourceName))
            {
                ResourceTable.Add(ResourceName, this);
            }
        }
    }
}