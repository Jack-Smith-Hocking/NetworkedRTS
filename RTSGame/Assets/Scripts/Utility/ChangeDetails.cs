using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeDetails : MonoBehaviour
{
    public string Name;
    public string LayerName;
    public bool InverseOperation = false;

    [ContextMenu("MakeStatic")]
    public void MakeStatic()
    {
        MakeChange(gameObject, (GameObject obj) => { obj.isStatic = true; });
    }

    [ContextMenu("ChangeLayer")]
    public void ChangeLayer()
    {
        MakeChange(gameObject, (GameObject obj) => { obj.layer = LayerMask.NameToLayer(LayerName); });
    }

    public void MakeChange(GameObject obj, Action<GameObject> action)
    {
        if (!obj) return;

        if ((obj.name.Contains(Name) && !InverseOperation) || InverseOperation)
        {
            action?.Invoke(obj);
        }

        for (int i = 0; i < obj.transform.childCount; i++)
        {
            MakeChange(obj.transform.GetChild(i).gameObject, action);
        }
    }
}
