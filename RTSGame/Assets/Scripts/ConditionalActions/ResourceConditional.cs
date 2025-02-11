﻿using RTS_System.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace RTS_System.Resource
{
    [CreateAssetMenu(fileName = "New ResourceCon", menuName = "ScriptableObject/RTS/Conditionals/ResourceCon")]
    public class ResourceConditional : ConditionalAction
    {
        [Tooltip("Data for each resource that this conditional will evaluate")] public List<Mod_ResourceValue> ResourceCosts = new List<Mod_ResourceValue>();
        [HideInInspector] public Mod_ResourceManager ResourceManager = null;

        public override bool EvaluateConditional()
        {
            bool eval = false;

            Helper.LoopList_ForEach<Mod_ResourceValue>(ResourceCosts, (Mod_ResourceValue rc) => { eval = rc.CanAfford(ResourceManager); }, () => { return !eval; });

            return eval;
        }
    }
}