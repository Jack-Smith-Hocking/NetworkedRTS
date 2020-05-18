using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

/// <summary>
/// Binds and unbinds actions to inputs
/// </summary>
public class BoundInput
{
    public enum BindCode
    {
        SUCCESS, FAILURE, INVALID_MAP, INVALID_ACTION, RE_BIND_SUCCEEDED
    }

    /// <summary>
    /// The currently bound input action
    /// </summary>
    public InputAction InputAction { get; private set; } = null;

    public List<InputAction> otherBoundInputs { get; private set; } = new List<InputAction>();

    /// <summary>
    /// The actions to be added to the 'performed' callback of the input
    /// </summary>
    public Action<InputAction.CallbackContext> PerformedActions = (InputAction.CallbackContext cc) => { return; };
    /// <summary>
    /// The actions to be add to the 'cancelled' callback of the input
    /// </summary>
    public Action<InputAction.CallbackContext> CancelledActions = (InputAction.CallbackContext cc) => { return; };

    #region CurrentValues
    public float CurrentFloatVal
    {
        get
        {
            float val = 0;
            if (InputAction != null) val = InputAction.ReadValue<float>();

            float tempVal = 0;
            Action<InputAction> loopAction = (InputAction action) =>
            {
                tempVal = action.ReadValue<float>();

                if (tempVal > val) val = tempVal;
            };

            Helper.LoopListForEach<InputAction>(otherBoundInputs, loopAction);

            return val;
        }
    }
    public bool CurrentBoolVal
    {
        get
        {
            bool val = false;
            if (InputAction != null) val = InputAction.ReadValue<float>() >= 0.01f;

            Helper.LoopListForEach<InputAction>(otherBoundInputs, (InputAction action) => { if (!val) val = action.ReadValue<float>() >= 0.01f; }, () => { return val; });

            return val;
        }
    }
    public Vector2 CurrentVec2Val
    {
        get
        {
            Vector2 val = Vector2.zero;
            if (InputAction != null) val = InputAction.ReadValue<Vector2>();

            Vector2 tempVal = Vector2.zero;
            Action<InputAction> loopAction = (InputAction action) =>
            {
                tempVal = action.ReadValue<Vector2>();

                if (tempVal.x > val.x) val.x = tempVal.x;
                if (tempVal.y > val.y) val.y = tempVal.y;
            };

            Helper.LoopListForEach<InputAction>(otherBoundInputs, loopAction);

            return val;
        }
    }
    #endregion

    /// <summary>
    /// Bind an action to an InputAction on the passed in PlayerInput with the name 'actionName', remember to bind actions to "PerformedActions" and "CancelledActions" before calling this
    /// </summary>
    /// <param name="playerInput">The PlayerInput to get input for</param>
    /// <param name="actionName">The name of the action in the control map</param>
    /// <param name="deleteOldBindings">Whether or not to keep previous bindings for this action</param>
    /// <returns></returns>
    public BindCode Bind(PlayerInput playerInput, string actionName, bool deleteOldBindings = true)
    {
        BindCode bindCode = BindCode.SUCCESS;

        if (playerInput == null)
        {
            return BindCode.INVALID_MAP;
        }
        else
        {
            // Get the InputAction of name 'actionName' from the playerInput
            InputAction tempAction = playerInput.currentActionMap.FindAction(actionName, false);
            if (tempAction == null)
            {
                DebugManager.WarningMessage($"There was an issue attempting to bind input '{actionName}', Error Code: {BindCode.INVALID_ACTION.ToString()}");
                return BindCode.INVALID_ACTION;
            }

            // If the InputAction was successfully obtained continue
            if (InputAction != null && deleteOldBindings)
            {
                UnbindAll();

                otherBoundInputs.Clear();

                bindCode = BindCode.RE_BIND_SUCCEEDED;
            }
            else if (InputAction != null && !deleteOldBindings)
            {
                // If we don't want to delete the old bindings, this will add an alternate binding 
                otherBoundInputs.Add(InputAction);

                bindCode = BindCode.RE_BIND_SUCCEEDED;
            }

            InputAction = tempAction;

            if (InputAction != null)
            {
                InputAction.performed += PerformedActions;
                InputAction.canceled += CancelledActions;
            }
        }


        if (bindCode != BindCode.SUCCESS && bindCode != BindCode.RE_BIND_SUCCEEDED)
        {
            DebugManager.WarningMessage($"There was an error attempting to rebind action '{actionName}', error code: {bindCode}");
        }

        return bindCode;
    }

    public void Unbind(InputAction inputAction)
    {
        if (inputAction != null)
        {
            inputAction.performed -= PerformedActions;
            inputAction.canceled -= CancelledActions;
        }
    }
    public void UnbindAll()
    {
        // Remove all of the actions from any previous bindings
        Unbind(InputAction);
        Helper.LoopListForEach<InputAction>(otherBoundInputs, (InputAction action) => { Unbind(action); });
    }
}
public static class Helper
{
    /// <summary>
    /// Loop through a list of type T (For Each Loop) and execute an action on each element
    /// </summary>
    /// <typeparam name="T">The type of data being worked with</typeparam>
    /// <param name="loopList">The list to affect</param>
    /// <param name="loopAction">The action to perform on each element of the list</param>
    /// <param name="breakOut">An optional Func<bool> that will determine any break conditions for the loop</bool></param>
    public static void LoopListForEach<T>(List<T> loopList, Action<T> loopAction, Func<bool> breakOut = null)
    {
        if (loopList != null && loopAction != null)
        {
            foreach (T loopVal in loopList)
            {
                if (loopVal == null) continue;

                if (breakOut != null && breakOut()) break;

                loopAction?.Invoke(loopVal);
            }
        }
    }
    /// <summary>
    /// Loop through a list of type T (For Loop) and execute an action on each element
    /// </summary>
    /// <typeparam name="T">The type of data being worked with</typeparam>
    /// <param name="loopList">The list to affect</param>
    /// <param name="loopAction">The action to perform on each element of the list</param>
    /// <param name="breakOut">An optional Func<bool> that will determine any break conditions for the loop</param>
    public static void LoopListFor<T>(List<T> loopList, Action<T> loopAction, Func<bool> breakOut = null)
    {
        if (loopList != null && loopAction != null)
        {
            T loopVal = default;

            for (int i = 0; i < loopList.Count; i++)
            {
                loopVal = loopList[i];

                if (loopVal == null) continue;

                if (breakOut != null && breakOut()) break;

                loopAction?.Invoke(loopVal);
            }
        }
    }

    /// <summary>
    /// Get a random point on a NavMesh
    /// </summary>
    /// <param name="origin">The original point of the object</param>
    /// <param name="dist">The max distance away from the origin to test for</param>
    /// <param name="areaMask">The area to test</param>
    /// <returns>A valid position on the NavMesh</returns>
    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int areaMask)
    {
        Vector3 rand_direction = UnityEngine.Random.insideUnitSphere * dist;

        rand_direction += origin;

        if (rand_direction.magnitude > 100000)
        {
            Debug.LogError("thing");
        }

        NavMeshHit navHit;

        bool foundPos = NavMesh.SamplePosition(rand_direction, out navHit, dist, areaMask);
        if (foundPos == false)
        {
            foundPos = NavMesh.SamplePosition(origin, out navHit, dist, areaMask);

            if (foundPos == false)
            {
                return RandomNavSphere(origin, dist * 1.5f, areaMask);
            }
            else
            {
                return origin;
            }
        }

        return navHit.position;
    }

    /// <summary>
    /// Will loop (For Each) through a list and see if the GameObject is in it. Ensure the list is a list of MonoBehavioours, checks are in place to reject anything else
    /// </summary>
    /// <typeparam name="T">The type of list, will need to be a MonoBehaviour or child of a MonoBehaviour</typeparam>
    /// <param name="monoList">TThe list to check in</param>
    /// <param name="obj">The GameObject to check for</param>
    /// <returns>Returns true if the GameObject is in the list</returns>
    public static bool ObjectInMonoList<T>(List<T> monoList, GameObject obj)
    {
        if (obj == null) return false;

        bool inList = false;
        MonoBehaviour mb = null;

        if (monoList != null && monoList.Count > 0)
        {
            mb = monoList[0] as MonoBehaviour;
            if (!mb)
            { 
                return false;
            }
        }
        else
        {
            return false;
        }

        Helper.LoopListForEach<T>(monoList, 
        (T mono) => // LoopAction
        {
            mb = mono as MonoBehaviour;
            if (mb)
            {
                if(mb.gameObject.Equals(obj))
                {
                    inList = true;
                }
            }
        }, 
        () => { return inList; }); // BreakOut action

        return inList;
    }

    /// <summary>
    /// Get the distance between two GameObjects
    /// </summary>
    /// <param name="objOne">First object</param>
    /// <param name="objTwo">Second object</param>
    /// <returns>Returns -1 if either object is null</returns>
    public static float Distance(GameObject objOne, GameObject objTwo)
    {
        if (!objOne || !objTwo) return -1;

        return Distance(objOne.transform, objTwo.transform);
    }
    /// <summary>
    /// Get the distance between two Transforms
    /// </summary>
    /// <param name="transOne">First object</param>
    /// <param name="transTwo">Second object</param>
    /// <returns>Returns -1 if either object is null</returns>
    public static float Distance(Transform transOne, Transform transTwo)
    {
        if (!transOne || !transTwo) return -1;

        return Vector3.Distance(transOne.position, transTwo.position);
    }
}
