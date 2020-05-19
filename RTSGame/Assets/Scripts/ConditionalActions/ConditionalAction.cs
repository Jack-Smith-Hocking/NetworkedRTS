using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionalAction : ScriptableObject
{
    public virtual bool EvaluateConditional()
    {
        return false;
    }
}
