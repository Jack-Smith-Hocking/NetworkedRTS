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
        public int ResourceStartCount = 0;
    }
}