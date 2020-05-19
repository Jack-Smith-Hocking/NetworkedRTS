using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTS_System
{
    [CreateAssetMenu(fileName = "New ResourceCost", menuName = "ScriptableObject/RTS/Resources/ResourceCost")]
    public class Mod_ResourceCost : ScriptableObject
    {
        public Mod_Resource ResourceType;
        [Min(0)] public int RawResourceCost = 0;
        public bool DecreaseResources = true;

        public int TrueResourceCost { get { return RawResourceCost * (DecreaseResources ? -1 : 1); } }
    }
}

