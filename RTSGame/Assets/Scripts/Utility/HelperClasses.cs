using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using System.Collections.Generic;

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

/// <summary>
/// Helps to keep track of many timers, stored in a dictionary
/// </summary>
public class TimerTracker
{
    private Dictionary<string, float> StaticTimerDict = new Dictionary<string, float>();

    /// <summary>
    /// Will add a timer (or overwrite) to the dictionary
    /// </summary>
    /// <param name="timerName">The key in the dictionary</param>
    /// <param name="time">The value to be held</param>
    /// <param name="overwrite">Whether or not to overwrite a timer that is held already</param>
    public void SetTimer(string timerName, float time, bool overwrite = false)
    {
        if (StaticTimerDict.ContainsKey(timerName) && overwrite)
        {
            StaticTimerDict[timerName] = time;
            return;
        }

        StaticTimerDict[timerName] = time;
    }

    /// <summary>
    /// Get the value of a timer 
    /// </summary>
    /// <param name="timerName">The key of the timer in the dictionary</param>
    /// <returns>Returns the value of the timer, or -1 if there is no timer</returns>
    public float GetTimer(string timerName)
    {
        if (StaticTimerDict.ContainsKey(timerName))
        {
            return StaticTimerDict[timerName];
        }

        DebugManager.WarningMessage($"Failed to find timer of name: {timerName}");
        return -1;
    }
    /// <summary>
    /// Checks if the timer at timerName is <= to the timeCheck
    /// </summary>
    /// <param name="timerName">The name of the timer to get</param>
    /// <param name="timeCheck">The time to check against</param>
    /// <returns>Will return true if no timer was found at timerName</returns>
    public bool CheckTimer(string timerName, float timeCheck)
    {
        return GetTimer(timerName) <= timeCheck;
    }
}

/// <summary>
/// Collection of useful functions
/// </summary>
public static class Helper
{
    // Reduces memory allocation
    #region VariableCaches
    public static Renderer CachedRenderer = null;
    public static MonoBehaviour CachedMono = null;
    public static List<GameObject> CachedList = new List<GameObject>();
    public static Rect CachedRect;
    #endregion

    public static TimerTracker TimerInstance = new TimerTracker();

    #region ListFunctions
    /// <summary>
    /// Will add a list to another list, but will check if each element of the additive list is already in the original
    /// </summary>
    /// <typeparam name="T">The type of data being worked with</typeparam>
    /// <param name="originalList">The list to add to</param>
    /// <param name="addList">The list to add</param>
    public static void ListAddRange<T>(ref List<T> originalList, List<T> addList)
    {
        foreach (T elem in addList)
        {
            Helper.ListAdd<T>(ref originalList, elem);
        }
    }

    /// <summary>
    /// Will add a item to an list if the item isn't already in it
    /// </summary>
    /// <typeparam name="T">The type of data being worked with</typeparam>
    /// <param name="originalList">The list to add to"</param>
    /// <param name="addObj">The object to add</param>
    public static bool ListAdd<T>(ref List<T> originalList, T addObj)
    {
        if (IsNullOrDestroyed<T>(addObj) && originalList == null)
        {
            return false;
        }

        // If there are no elements in the list, add
        if (originalList.Count == 0)
        {
            originalList.Add(addObj);

            return true;
        }
        // If the obj is not in the list, add
        else if (!originalList.Contains(addObj))
        {
            originalList.Add(addObj);

            return true;
        }

        return false;
    }

    /// <summary>
    /// Remove an element from a list
    /// </summary>
    /// <typeparam name="T">The type of data in the lsit</typeparam>
    /// <param name="trimList">The list to be 'trimmed'</param>
    /// <param name="trimElem">The element to remove from the list</param>
    public static void TrimElement<T>(ref List<T> trimList, T trimElem)
    {
        if (trimList != null && trimList.Count > 0)
        {
            trimList.Remove(trimElem);
        }
    }
    /// <summary>
    /// Remove a list of objects from another list
    /// </summary>
    /// <typeparam name="T">The type of data in the list</typeparam>
    /// <param name="listToTrim">The list to have elements removed from</param>
    /// <param name="trimList">The list of elements to remove</param>
    public static void TrimElements<T>(ref List<T> listToTrim, List<T> trimList)
    {
        // If both lists are valid
        if (listToTrim != null && trimList != null)
        {
            foreach (T elem in trimList)
            {
                TrimElement<T>(ref listToTrim, elem);
            }
        }
    }

    /// <summary>
    /// Remove all of the null or destroyed elements from a list
    /// </summary>
    /// <typeparam name="T">The type of data in the list</typeparam>
    /// <param name="listToTrim">The list to trim</param>
    public static void TrimList<T>(ref List<T> listToTrim)
    {
        if (listToTrim != null)
        {
            TrimElements<T>(ref listToTrim, GetNullOrDestroyed<T>(listToTrim));
        }
    }

    /// <summary>
    /// Loop through a list of type T (For Each Loop) and execute an action on each element
    /// </summary>
    /// <typeparam name="T">The type of data being worked with</typeparam>
    /// <param name="loopList">The list to affect</param>
    /// <param name="loopAction">The action to perform on each element of the list</param>
    /// <param name="breakOut">An optional Func<bool> that will determine any break conditions for the loop</bool></param>
    public static void LoopList_ForEach<T>(List<T> loopList, Action<T> loopAction, Func<bool> breakOut = null)
    {
        // Check if the list and action are both valid
        if (loopList != null && loopAction != null)
        {
            // Loop through the list and perform an action
            foreach (T loopVal in loopList)
            {
                if (IsNullOrDestroyed<T>(loopVal))
                {
                    continue;
                }

                loopAction?.Invoke(loopVal);

                if (breakOut != null && breakOut()) break;
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
    public static void LoopList_For<T>(List<T> loopList, Action<T> loopAction, Func<bool> breakOut = null)
    {
        if (loopList != null && loopAction != null)
        {
            T loopVal = default;

            for (int i = 0; i < loopList.Count; i++)
            {
                loopVal = loopList[i];

                if (IsNullOrDestroyed<T>(loopVal))
                {
                    continue;
                }

                loopAction?.Invoke(loopVal);

                if (breakOut != null && breakOut()) break;
            }
        }
    }

    /// <summary>
    /// Get a list of all the null or destroyed elements in a list
    /// </summary>
    /// <typeparam name="T">The type of data in the list</typeparam>
    /// <param name="listToCheck">The list to get null or destroyed elements from</param>
    /// <returns>A list of all null or destroyed elements in the list</returns>
    public static List<T> GetNullOrDestroyed<T>(List<T> listToCheck)
    {
        if (listToCheck == null || listToCheck.Count == 0)
        {
            return new List<T>(0);
        }

        List<T> nullList = new List<T>(listToCheck.Count);

        foreach (T elem in listToCheck)
        {
            if (IsNullOrDestroyed<T>(elem))
            {
                nullList.Add(elem);
            }
        }

        return nullList;
    }
    /// <summary>
    /// Will loop (For Each) through a list and see if the GameObject is in it. Ensure the list is a list of MonoBehavioours, checks are in place to reject anything else (Modifies 'CachedList')
    /// </summary>
    /// <typeparam name="T">The type of list, will need to be a MonoBehaviour or child of a MonoBehaviour</typeparam>
    /// <param name="monoList">TThe list to check in</param>
    /// <param name="obj">The GameObject to check for</param>
    /// <returns>Returns true if the GameObject is in the list</returns>
    public static bool ObjectInMonoList<T>(List<T> monoList, GameObject obj)
    {
        if (obj == null || monoList == null) return false;

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
        (T mono) => 
        // LoopAction
        {
            CachedMono = mono as MonoBehaviour;
            if (CachedMono)
            {
                if (CachedMono.gameObject.Equals(obj))
                {
                    inList = true;
                }
            }
        },
        // BreakOut action
        () => { return inList; }); 

        return inList;
    }
    #endregion

    #region VectorFunctions
    /// <summary>
    /// Get the distance between two GameObjects
    /// </summary>
    /// <param name="objOne">First object</param>
    /// <param name="objTwo">Second object</param>
    /// <returns>Returns -1 if either object is null</returns>
    public static float Distance(GameObject objOne, GameObject objTwo)
    {
        if (!objOne || !objTwo)
        {
            DebugManager.WarningMessage("Tried to get the distance between null GameObjects");
            return -1;
        }

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
        if (!transOne || !transTwo)
        {
            DebugManager.WarningMessage("Tried to get the distance between null GameObjects");
            return -1;
        }

        return Vector3.Distance(transOne.position, transTwo.position);
    }

    /// <summary>
    /// Return whether two points are within 'dist' of each other 
    /// </summary>
    /// <param name="start">The start position</param>
    /// <param name="end">The end position</param>
    /// <param name="dist">The distance to check against</param>
    /// <returns>Whether the two points are within 'dist'</returns>
    public static bool InDistance(Vector3 start, Vector3 end, float dist)
    {
        return (Vector3.Distance(start, end) <= dist);
    }
    /// <summary>
    /// Returns whether two points are further away than 'dist'
    /// </summary>
    /// <param name="start">The start position</param>
    /// <param name="end">The end position</param>
    /// <param name="dist">The distance to check against</param>
    /// <returns>Whether the two points are further than 'dist' apart</returns>
    public static bool OutDistance(Vector3 start, Vector3 end, float dist)
    {
        return !InDistance(start, end, dist);
    }
    #endregion

    #region GameObjectFunctions
    /// <summary>
    /// Will return all the GameObjects within a selection area (Modifies 'CachedList')
    /// </summary>
    /// <param name="objList">List of objects to check within selection area</param>
    /// <param name="startPos">Start selection position (ensure is in ViewPort space)</param>
    /// <param name="endPos">End selection position (ensure is in ViewPort space)</param>
    /// <param name="cam">The camera to base the calculations off of</param>
    /// <returns>A list of GameObjects in the area</returns> 
    public static List<GameObject> GetObjectsInViewport(List<GameObject> objList, Camera cam, Vector3 startPos, Vector3 endPos)
    {
        if (objList == null || cam == null)
        {
            return null;
        }

        CachedRect.x = startPos.x;
        CachedRect.y = startPos.y;
        CachedRect.width = (endPos.x - startPos.x);
        CachedRect.height = (endPos.y - startPos.y);

        CachedList.Clear();

        Helper.LoopList_ForEach<GameObject>(objList, (GameObject go) =>
        {
            if (go != null)
            {
                if (CachedRect.Contains(cam.WorldToViewportPoint(go.transform.position), true))
                {
                    CachedList.Add(go);
                }
            }
        });

        return CachedList;
    }

    /// <summary>
    /// Will return all the GameObjects within a selection area (Modifies 'CachedList')
    /// </summary>
    /// <param name="objList">List of objects to check within selection area</param>
    /// <param name="startPos">Start selection position (ensure is in ViewPort space)</param>
    /// <param name="endPos">End selection position (ensure is in ViewPort space)</param>
    /// <param name="objLayerMask">A GameObject in the area needs to be in this layer to be valid</param>
    /// <param name="cam">The camera to base the calculations off of</param>
    /// <returns>A list of GameObjects in the area</returns>
    public static List<GameObject> GetObjectsInViewport(List<GameObject> objList, Camera cam, Vector3 startPos, Vector3 endPos, LayerMask objLayerMask)
    {
        if (objList == null || cam == null)
        {
            return new List<GameObject>(0);
        }

        CachedRect.x = startPos.x;
        CachedRect.y = startPos.y;
        CachedRect.width = (endPos.x - startPos.x);
        CachedRect.height = (endPos.y - startPos.y);

        CachedList.Clear();

        // Loop over the list to see which GameObjects are in the area
        Helper.LoopList_ForEach<GameObject>(objList, (GameObject go) =>
        {
            if (go != null)
            {
                // Check if this GameObject is in the right layer
                if (Helper.IsInLayerMask(objLayerMask, go.layer))
                {
                    // Determines whether the GameObject is in the selection area
                    if (CachedRect.Contains(cam.WorldToViewportPoint(go.transform.position), true))
                    {
                        CachedList.Add(go);
                    }
                }
            }
        });

        return CachedList;
    }

    /// <summary>
    /// Only pass in MonoBehaviour as type parameter otherwise there will be errors!
    /// </summary>
    /// <typeparam name="T">Make sure this is a MonoBehaviour type</typeparam>
    /// <param name="obj">The object to check for the MonoBehaviour</param>
    /// <returns>The first found MonoBehaviour (Checks GetComponent, GetComponentInChildren then GetComponentInParent)</returns>
    public static T GetComponent<T>(GameObject obj)
    {
        if (IsNullOrDestroyed<GameObject>(obj)) return default;

        T type = obj.GetComponent<T>();

        if (type == null)
        {
            type = obj.GetComponentInChildren<T>();
        }
        if (type == null)
        {
            type = obj.GetComponentInParent<T>();
        }

        return type;
    }
    /// <summary>
    /// Only pass in MonoBehaviour as type parameter otherwise there will be errors! Gets all of the components in a GameObject hierarchy 
    /// </summary>
    /// <typeparam name="T">The type of MonoBehaviour to get</typeparam>
    /// <param name="obj">The GameObject to get the MonoBehaviours from</param>
    /// <returns>A list of found MonoBehaviours</returns>
    public static List<T> GetComponents<T>(GameObject obj)
    {
        if (IsNullOrDestroyed(obj)) return new List<T>(0);

        List<T> componentList = new List<T>();

        componentList.AddRange(obj.GetComponents<T>());
        componentList.AddRange(obj.GetComponentsInChildren<T>());
        componentList.AddRange(obj.GetComponentsInParent<T>());

        return componentList;
    }
    /// <summary>
    /// Will send a message up and down a GameObject hierarchy 
    /// </summary>
    /// <param name="go">GameObject to send message to</param>
    /// <param name="message">Message to be sent</param>
    /// <param name="options">Whether there will be a warning message if there is no receiver</param>
    public static void SendMessageToChain(GameObject go, string message, SendMessageOptions options = SendMessageOptions.DontRequireReceiver)
    {
        // Check if the message and GameObject are both valid
        if (!IsNullOrDestroyed<GameObject>(go) && message.Length > 0)
        {
            // Send the message to all children
            go.BroadcastMessage(message, options);

            // Send the message to parent if it has one
            if (go.transform.parent)
            {
                go.transform.parent.SendMessageUpwards(message, options);
            }
        }
    }
    #endregion

    #region StringFunctions
    /// <summary>
    /// Remove a string from another string
    /// </summary>
    /// <param name="original">The string to remove from</param>
    /// <param name="toExclude">The string to remove</param>
    /// <returns>The original string after toExclude is removed</returns>
    public static string ExcludeInString(string original, string toExclude)
    {
        return original.Replace(toExclude, "");
    }
    /// <summary>
    /// Remove a list of strings from a list
    /// </summary>
    /// <param name="original">The string to remove from</param>
    /// <param name="toExclude">The list of strings to remove</param>
    /// <returns>The original string after all exclusions are done</returns>
    public static string MultiExludeInString(string original, List<string> toExclude)
    {
        // Go through each element in toExclude and remove it from the original string
        Helper.LoopList_ForEach<string>(toExclude, (string s) =>
        {
            original = Helper.ExcludeInString(original, s);
        });

        return original;
    }
    /// <summary>
    /// Separate a string by upper cases (place a space before every upper case)
    /// </summary>
    /// <param name="text">The text to change</param>
    /// <returns>The text with spaces before every upper case</returns>
    public static string SeparateByUpperCase(string text)
    {
        string newText = "";

        for (int i = 0; i < text.Length; i++)
        {
            // Check if the character is upper case
            // Add a space if it is
            if (char.IsUpper(text[i]) && i > 0)
            {
                newText += " ";
            }
         
            // Add the character to the new string
            newText += text[i];
        }

        return newText;
    }
    #endregion

    #region MaterialFunctions
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
    /// Set the material of all GameObjects in the list (only the first Renderer on the GameObject) (Modifies 'CachedRenderer')
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
    /// Set the material of a GameObject (Modifies 'CachedRenderer')
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
        }
    }
    #endregion

    /// <summary>
    /// Get a random point on a NavMesh
    /// </summary>
    /// <param name="origin">The original point of the object</param>
    /// <param name="dist">The max distance away from the origin to test for</param>
    /// <param name="areaMask">The area to test</param>
    /// <returns>A valid position on the NavMesh</returns>
    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int areaMask)
    {
        Vector3 rand_direction = UnityEngine.Random.insideUnitSphere * Mathf.Abs(dist);

        rand_direction += origin;

        NavMeshHit navHit;

        bool foundPos = NavMesh.SamplePosition(rand_direction, out navHit, Mathf.Abs(dist), areaMask);
        if (foundPos == false)
        {
            foundPos = NavMesh.SamplePosition(origin, out navHit, Mathf.Abs(dist), areaMask);

            if (foundPos == false)
            {
                return RandomNavSphere(origin, dist * 1.5f, areaMask);
            }
        }

        return navHit.position;
    }

    /// <summary>
    /// Get whether a Layer is in a LayerMask
    /// </summary>
    /// <param name="mask">The mask to check against</param>
    /// <param name="layer">The layer to check</param>
    /// <returns>Whether the layer is in the mask</returns>
    public static bool IsInLayerMask(LayerMask mask, int layer)
    {
        return mask == (mask | (1 << layer));
    }
    /// <summary>
    /// Determine whether an object is null or has been destroyed 
    /// </summary>
    /// <typeparam name="T">The type of data being worked with</typeparam>
    /// <param name="obj">The object to test</param>
    /// <returns>Returns true if it was null or destroyed</returns>
    public static bool IsNullOrDestroyed<T>(T obj)
    {
        return (obj == null) || (obj.Equals(null));
    }
}
