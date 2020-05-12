using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

/// <summary>
/// Since Triggers and Collisions have no callback functionality (to my knowledge) i made a simple class to do just that
/// Has a set of UnityEvents with no arguments and Actions that take the appropriate arguments 
/// </summary>
public class CollisionCallback : MonoBehaviour
{
    #region Collisions
    [Header("Collisions")]
    [Tooltip("Callback for when this object enters a collision")] public UnityEvent OnCollisionEnterEvent = null;
    [Tooltip("Callback for when this object exits a collision")] public UnityEvent OnCollisionExitEvent = null;
    [Tooltip("Callback for when this object stays in a collision")] public UnityEvent OnCollisionStayEvent = null;

    public Action<Collision> CollisionEnterAction = null;
    public Action<Collision> CollisionExitAction = null;
    public Action<Collision> CollisionStayAction = null;

    public void OnCollisionEnter(Collision collision)
    {
        if (OnCollisionEnterEvent != null) OnCollisionEnterEvent.Invoke();
        if (CollisionEnterAction != null) CollisionEnterAction.Invoke(collision);
    }
    public void OnCollisionStay(Collision collision)
    {
        if (OnCollisionStayEvent != null) OnCollisionStayEvent.Invoke();
        if (CollisionStayAction != null) CollisionStayAction.Invoke(collision);
    }
    public void OnCollisionExit(Collision collision)
    {
        if (OnCollisionExitEvent != null) OnCollisionExitEvent.Invoke();
        if (CollisionExitAction != null) CollisionExitAction.Invoke(collision);
    }
    #endregion

    #region Triggers
    [Header("Triggers")]
    [Tooltip("Callback for when this object enters a trigger")] public UnityEvent OnTriggerEnterEvent = null;
    [Tooltip("Callback for when this object exits a trigger")] public UnityEvent OnTriggerExitEvent = null;
    [Tooltip("Callback for when this object stays in a trigger")] public UnityEvent OnTriggerStayEvent = null;

    public Action<Collider> TriggerEnterAction = null;
    public Action<Collider> TriggerExitAction = null;
    public Action<Collider> TriggerStayAction = null;

    public void OnTriggerEnter(Collider other)
    {
        if (OnTriggerEnterEvent != null) OnTriggerEnterEvent.Invoke();
        if (TriggerEnterAction != null) TriggerEnterAction.Invoke(other);
    }
    public void OnTriggerStay(Collider other)
    {
        if (OnTriggerStayEvent != null) OnTriggerStayEvent.Invoke();
        if (TriggerStayAction != null) TriggerStayAction.Invoke(other);
    }
    public void OnTriggerExit(Collider other)
    {
        if (OnTriggerExitEvent != null) OnTriggerExitEvent.Invoke();
        if (TriggerExitAction != null) TriggerExitAction.Invoke(other);
    }
    #endregion
}
