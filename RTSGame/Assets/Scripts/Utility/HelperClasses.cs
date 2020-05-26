using System;
using System.Collections.Generic;
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

            Helper.LoopList_ForEach<InputAction>(otherBoundInputs, loopAction);

            return val;
        }
    }
    public bool CurrentBoolVal
    {
        get
        {
            bool val = false;
            if (InputAction != null) val = InputAction.ReadValue<float>() >= 0.01f;

            Helper.LoopList_ForEach<InputAction>(otherBoundInputs, (InputAction action) => { if (!val) val = action.ReadValue<float>() >= 0.01f; }, () => { return val; });

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

            Helper.LoopList_ForEach<InputAction>(otherBoundInputs, loopAction);

            return val;
        }
    }
    #endregion

    #region Bind
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

            bindCode = Bind(tempAction, deleteOldBindings);
        }


        if (bindCode != BindCode.SUCCESS && bindCode != BindCode.RE_BIND_SUCCEEDED)
        {
            DebugManager.WarningMessage($"There was an error attempting to rebind action '{actionName}', error code: {bindCode}");
        }

        return bindCode;
    }

    public BindCode Bind(InputActionReference actionRef, bool deleteOldBindings = true)
    {
        if (actionRef == null)
        {
            DebugManager.WarningMessage($"There was an issue attempting to bind input, Error Code: {BindCode.INVALID_ACTION.ToString()}");

            return BindCode.INVALID_ACTION;
        }

        return Bind(actionRef.action, deleteOldBindings);
    }

    public BindCode Bind(InputAction action, bool deleteOldBindings = true)
    {
        BindCode bindCode = BindCode.SUCCESS;

        if (action == null)
        {
            DebugManager.WarningMessage($"There was an issue attempting to bind input, Error Code: {BindCode.INVALID_ACTION.ToString()}");
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

        InputAction = action;

        if (InputAction != null)
        {
            InputAction.performed += PerformedActions;
            InputAction.canceled += CancelledActions;
        }

        return bindCode;
    }
    #endregion 
    public void Unbind(InputActionReference inputAction)
    {
        if (inputAction)
        {
            Unbind(inputAction.action);
        }
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
        Helper.LoopList_ForEach<InputAction>(otherBoundInputs, (InputAction action) => { Unbind(action); });
    }
}
public static class Helper
{
    public static Renderer CachedRenderer = null;
    public static MonoBehaviour CachedMono = null;

    public static void ListAddRange<T>(ref List<T> originalList, List<T> addList)
    {
        foreach (T elem in addList)
        {
            Helper.ListAdd<T>(ref originalList, elem);
        }
    }

    public static void ListAdd<T>(ref List<T> originalList, T addObj)
    {
        if (addObj != null && originalList != null)
        {
            if (originalList.Count == 0)
            {
                originalList.Add(addObj);
            }
            else if (!originalList.Contains(addObj))
            {
                originalList.Add(addObj);
            }
        }
    }

    /// <summary>
    /// Loop through a list of type T (For Each Loop) and execute an action on each element
    /// </summary>
    /// <typeparam name="T">The type of data being worked with</typeparam>
    /// <param name="loopList">The list to affect</param>
    /// <param name="loopAction">The action to perform on each element of the list</param>
    /// <param name="breakOut">An optional Func<bool> that will determine any break conditions for the loop</bool></param>
    /// <returns>Will return a list of null objects in the loopList, is cut short if the breakOut is triggered</returns>
    public static List<T> LoopList_ForEach<T>(List<T> loopList, Action<T> loopAction, Func<bool> breakOut = null)
    {
        List<T> nullObjs = new List<T>(loopList.Count);

        if (loopList != null && loopAction != null)
        {
            foreach (T loopVal in loopList)
            {
                if (loopVal == null)
                {
                    nullObjs.Add(loopVal);
                    continue;
                }

                loopAction?.Invoke(loopVal);

                if (breakOut != null && breakOut()) break;
            }
        }

        return nullObjs;
    }
    /// <summary>
    /// Loop through a list of type T (For Loop) and execute an action on each element
    /// </summary>
    /// <typeparam name="T">The type of data being worked with</typeparam>
    /// <param name="loopList">The list to affect</param>
    /// <param name="loopAction">The action to perform on each element of the list</param>
    /// <param name="breakOut">An optional Func<bool> that will determine any break conditions for the loop</param>
    /// <returns>Will return a list of null objects in the loopList, is cut short if the breakOut is triggered</returns>
    public static List<T> LoopList_For<T>(List<T> loopList, Action<T> loopAction, Func<bool> breakOut = null)
    {
        List<T> nullObjs = new List<T>(loopList.Count);

        if (loopList != null && loopAction != null)
        {
            T loopVal = default;

            for (int i = 0; i < loopList.Count; i++)
            {
                loopVal = loopList[i];

                if (loopVal == null)
                {
                    nullObjs.Add(loopVal);
                    continue;
                }

                loopAction?.Invoke(loopVal);

                if (breakOut != null && breakOut()) break;
            }
        }

        return nullObjs;
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
        if (obj == null && monoList != null) return false;

        bool inList = false;

        if (monoList.Count > 0)
        {
            CachedMono = monoList[0] as MonoBehaviour;
            if (!CachedMono)
            { 
                return false;
            }
        }
        else
        {
            return false;
        }

        Helper.LoopList_ForEach<T>(monoList, 
        (T mono) => // LoopAction
        {
            CachedMono = mono as MonoBehaviour;
            if (CachedMono)
            {
                if(CachedMono.gameObject.Equals(obj))
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

    public static bool IsInLayerMask(LayerMask mask, int layer)
    {
        return mask == (mask | (1 << layer));
    }

    #region SetMaterials
    /// <summary>
    /// Set the material of all Renderers in the list
    /// </summary>
    /// <param name="list">List of Renderers to update</param>
    /// <param name="mat">The material to change to</param>
    /// <param name="breakOut">A breakout function to stop changing materials</param>
    public static void SetMaterials(List<Renderer> list, Material mat, Func<bool> breakOut = null)
    {
        if (list == null || !mat) return;

        Helper.LoopList_ForEach<Renderer>(list, (Renderer rend) => { SetMaterial(rend, mat); }, breakOut);
    }
    /// <summary>
    /// Set the material of a Renderer
    /// </summary>
    /// <param name="rend">Renderer to update</param>
    /// <param name="mat">Material to update to</param>
    public static void SetMaterial(Renderer rend, Material mat)
    {
        if (!rend || !mat) return;

        rend.material = mat;
    }

    /// <summary>
    /// Set the material of all GameObjects in the list (only the first Renderer on the GameObject)
    /// </summary>
    /// <param name="list">List of GameObjects to update</param>
    /// <param name="mat">The material to change to</param>
    /// <param name="breakOut">A breakout function to stop changing materials</param>
    public static void SetMaterials(List<GameObject> list, Material mat, Func<bool> breakOut = null)
    {
        if (list == null || !mat) return;

        Helper.LoopList_ForEach<GameObject>(list, (GameObject obj) => { SetMaterial(obj, mat); }, breakOut);
    }
    /// <summary>
    /// Set the material of a GameObject
    /// </summary>
    /// <param name="obj">GameObject to update</param>
    /// <param name="mat">Material to update to</param>
    public static void SetMaterial(GameObject obj, Material mat)
    {
        if (!obj || !mat) return;

        CachedRenderer = obj.GetComponent<Renderer>();
        if (CachedRenderer)
        {
            CachedRenderer.material = mat;
            CachedRenderer = null;
        }
    }
    #endregion
}
