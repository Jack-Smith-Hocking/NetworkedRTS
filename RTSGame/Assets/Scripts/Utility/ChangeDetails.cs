using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ChangeDetails : MonoBehaviour
{
    public string Name;
    public string LayerName;
    public Transform NewParent = null;
    public bool InverseOperation = false;
    public bool ChangeAll = false;

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

    [ContextMenu("AddNetworkIdentity")]
    public void AddNetworkIdentity()
    {
        MakeChange(gameObject, (GameObject obj) => { obj.AddComponent<NetworkIdentity>(); });
    }

    [ContextMenu("RemoveParent")]
    public void RemoveParent()
    {
        MakeChange(gameObject, (GameObject obj) => { obj.transform.parent = null; });
    }

    [ContextMenu("ChangeParent")]
    public void ChangeParent()
    {
        if (NewParent)
        {
            MakeChange(gameObject, (GameObject obj) => { obj.transform.parent = NewParent; });
        }
    }

    [ContextMenu("TurnOffRenderer")]
    public void TurnOffRenderer()
    {
        ToggleRenderer(false);
    }
    [ContextMenu("TurnOnRenderer")]
    public void TurnOnRenderer()
    {
        ToggleRenderer(true);
    }

    public void ToggleRenderer(bool state)
    {
        List<Renderer> Renderers = new List<Renderer>(Helper.GetComponents<Renderer>(gameObject));

        Helper.LoopList_ForEach<Renderer>(Renderers, (Renderer rend) =>
        {
            rend.enabled = state;
        });
    }

    public void MakeChange(GameObject obj, Action<GameObject> action)
    {
        if (!obj) return;

        if ((obj.name.Contains(Name) && !InverseOperation) || InverseOperation || ChangeAll)
        {
            action?.Invoke(obj);
        }

        for (int i = 0; i < obj.transform.childCount; i++)
        {
            MakeChange(obj.transform.GetChild(i).gameObject, action);
        }
    }
}
