using RTS_System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New ResourceCon", menuName = "ScriptableObject/RTS/Conditionals/ResourceCon")]
public class ResourceConditional : ConditionalAction
{
    public List<RTS_System.Mod_ResourceCost> ResourceCosts = new List<RTS_System.Mod_ResourceCost>();

    public override bool EvaluateConditional()
    {
        bool eval = false;

        Helper.LoopListForEach<Mod_ResourceCost>(ResourceCosts, (Mod_ResourceCost rc) => { eval = Mod_ResourceManager.Instance.CanAfford(rc); }, () => { return !eval; });

        return eval;
    }

    public bool AddResources()
    {
        bool eval = false;
        eval = EvaluateConditional();

        if (!eval)
        {
            return eval;
        }

        Helper.LoopListForEach<Mod_ResourceCost>(ResourceCosts, (Mod_ResourceCost rc) => { Mod_ResourceManager.Instance.AddResources(rc); });

        return eval;
    }
}
