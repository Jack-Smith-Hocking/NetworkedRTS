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
    [Space]
    public bool ContainName = false;
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

    /// <summary>
    /// Turn all of the Renderers on this GameObject and children on/off
    /// </summary>
    /// <param name="state">Active state of the Renderers</param>
    public void ToggleRenderer(bool state)
    {
        List<Renderer> renderers = new List<Renderer>(Helper.GetComponents<Renderer>(gameObject));

        Helper.LoopList_ForEach<Renderer>(renderers, (Renderer rend) =>
        {
            rend.enabled = state;
        });
    }

    /// <summary>
    /// Make a change to a GameObject and all the children of this GameObject recursively 
    /// </summary>
    /// <param name="obj">GameObject to start with</param>
    /// <param name="action">The change to make</param>
    public void MakeChange(GameObject obj, Action<GameObject> action)
    {
        if (!Helper.IsNullOrDestroyed<GameObject>(obj)) return;

        MakeSingleChange(obj, action);

        for (int i = 0; i < obj.transform.childCount; i++)
        {
            MakeChange(obj.transform.GetChild(i).gameObject, action);
        }
    }
    /// <summary>
    /// Make a change to a GameObject
    /// </summary>
    /// <param name="obj">GameObject to change</param>
    /// <param name="action">The change to make</param>
    public void MakeSingleChange(GameObject obj, Action<GameObject> action)
    {
        if (!Helper.IsNullOrDestroyed<GameObject>(obj)) return;

        if ((obj.name.Contains(Name) && ContainName) || !ContainName || ChangeAll)
        {
            action?.Invoke(obj);
        }
    }
}
