using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTS_System
{
    [CreateAssetMenu(fileName = "New ResourceCost", menuName = "ScriptableObject/RTS/Resources/ResourceCost")]
    public class Mod_ResourceCost : ScriptableObject
    {
        public Mod_Resource ResourceType;
        public int ResourceCost = 0;
    }
}

