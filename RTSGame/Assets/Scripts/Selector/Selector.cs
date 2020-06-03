﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using Mirror;
using RTS_System.AI;
using System;
using UnityEngine.XR.WSA.Input;

namespace RTS_System.Selection
{
    public class Selector : NetworkBehaviour
    {
        /// <summary>
        /// The client selector instance
        /// </summary>
        public static Selector ClientInstance = null;

        [Header("Details")]
        [Tooltip("The camera to use for selecting, if left null will use main camera")] public Camera SelectorCam = null;
        [Tooltip("The layers that the Selector will deem 'Selectable', anything not in the layer will be discarded")] public LayerMask SelectionMask;
        [Tooltip("The minimum distance of the mouse has to travel for a click to count as a drag")] public float MinDragDistance = 0.1f;

        [Header("Input Details")]
        [Tooltip("Determines the input for the main select button")] public InputActionReference MainSelectButton;
        [Tooltip("Determines the input for the multi-select button")] public InputActionReference MultiSelectInput;
        [Tooltip("Determines the input for adding an action to a queue")] public InputActionReference ActionQueueInput;
        [Space]

        /// <summary>
        /// All the GameObjects that are deemed as 'Selectable' in the scene, all 'Selectable' GameObjects will add themselves to this list
        /// </summary>
        public List<GameObject> SceneSelectables = new List<GameObject>();
        /// <summary>
        /// All of the currently selected GameObjects
        /// </summary>
        public List<GameObject> SelectedObjects = new List<GameObject>();

        /// <summary>
        /// Will be invoked whenever the list of SelectedObjects is updated
        /// </summary>
        public Action OnSelectedChange;

        /// <summary>
        /// Whether the next action call should be added to an AIAgent's queue
        /// </summary>
        public bool AddToActionList { get; private set; } = false;

        /// <summary>
        /// Will get the first selected GameObject in the SelectedObjects list
        /// </summary>
        public GameObject GetFirstSelected
        {
            get
            {
                if (SelectedObjects.Count > 0)
                {
                    return SelectedObjects[0];
                }
                return null;
            }
        }

        private List<GameObject> highlightedObjects = new List<GameObject>();
        /// 

        /// The start and end position of a drag, stored in ViewPort space, and screen space
        private Vector3 mStartPos
        {
            get
            {
                return SelectorCam.ViewportToScreenPoint(vpStartPos);
            }
        }
        private Vector3 mEndPos
        {
            get
            {
                return SelectorCam.ViewportToScreenPoint(vpEndPos);
            }
        }

        private Vector3 vpStartPos;
        private Vector3 vpEndPos;
        /// 

        bool isSelecting = false;

        private BoundInput selectInput = new BoundInput();
        private BoundInput multiSelectInput = new BoundInput();
        private BoundInput actionQueueInput = new BoundInput();

        // Start is called before the first frame update
        void Start()
        {
            // Check if this Selector is on the local player
            if (!isLocalPlayer) return;

            // This Selector is the client instance
            ClientInstance = this;

            if (!SelectorCam)
            {
                SelectorCam = Camera.main;
            }

            // Set up the select action
            selectInput.PerformedActions += SelectCallback;
            selectInput.CancelledActions += SelectCallback;
            selectInput.Bind(MainSelectButton);

            // Set up the multi-select button
            multiSelectInput.Bind(MultiSelectInput);
            // Set up the queue button
            actionQueueInput.Bind(ActionQueueInput);
        }

        /// <summary>
        /// Callback for when the 'selectInput' is activated, deals with selecting GameObjects
        /// </summary>
        /// <param name="cc"></param>
        void SelectCallback(InputAction.CallbackContext cc)
        {
            isSelecting = cc.ReadValueAsButton();

            // Check whether the player is adding to the current list of SelectedObjects
            if (multiSelectInput.CurrentBoolVal == false)
            {
                ClearSelection();
            }

            // Get the position of the mouse when the player first presses the selectInput button
            if (isSelecting)
            {
                vpStartPos = SelectorCam.ScreenToViewportPoint(Input.mousePosition);
            }
            else
            {
                vpEndPos = SelectorCam.ScreenToViewportPoint(Input.mousePosition);

                if (Helper.OutDistance(mStartPos, mEndPos, MinDragDistance))
                {
                    SelectMultiple();
                }
                else
                {
                    SelectSingle();
                }

                ClearHighlighted();

                // SelectedObjects has changed, update the callbacks
                OnSelectedChange?.Invoke();
            }
        }

        private void Update()
        {
            // If this is not the local Selector, don't do anything
            if (!isLocalPlayer) return;

            // Update whether an action should be added to AIAgent's queues or not
            AddToActionList = actionQueueInput.CurrentBoolVal;

            ClearHighlighted();

            if (isSelecting)
            {
                vpEndPos = SelectorCam.ScreenToViewportPoint(Input.mousePosition);

                if (Helper.OutDistance(mStartPos, mEndPos, MinDragDistance))
                {
                    // Get all the objects in the selection ViewPort 
                    highlightedObjects.AddRange(Helper.GetObjectsInViewport(SceneSelectables, SelectorCam, vpStartPos, vpEndPos, SelectionMask));

                    // Tell them they are being hovered over
                    Helper.LoopList_ForEach<GameObject>(highlightedObjects,
                    // Loop action    
                    (GameObject go) =>
                    {
                        Helper.SendMessageToChain(go, ISelectableEnum.SetHover.ToString());
                    });
                }
            }
            // Else if it is not active, then we will just highlight whatever is under the mouse
            else
            {
                RaycastHit rayHit;
                // Get the GameObject under the mouse
                if (Physics.Raycast(SelectorCam.ScreenPointToRay(Input.mousePosition), out rayHit, Mathf.Infinity, SelectionMask))
                {
                    // If it is a valid target
                    if (SceneSelectables.Contains(rayHit.collider.gameObject))
                    {
                        highlightedObjects.Add(rayHit.collider.gameObject);
                        Helper.SendMessageToChain(rayHit.collider.gameObject, ISelectableEnum.SetHover.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Add a GameObject to the list of SelectedObjects, also tell it that it is now selected
        /// </summary>
        /// <param name="selected">The GameObject to select</param>
        public void AddSelected(GameObject selected)
        {
            // If the GameObject is not already selected, select it and add it to the list
            if (Helper.ListAdd<GameObject>(ref SelectedObjects, selected))
            {
                Helper.SendMessageToChain(selected, ISelectableEnum.OnSelect.ToString());
            }
        }

        /// <summary>
        /// Select a single GameObject under the mouse
        /// </summary>
        /// <param name="toggleOff"></param>
        void SelectSingle(bool toggleOff = true)
        {
            RaycastHit rayHit;
            if (Physics.Raycast(SelectorCam.ScreenPointToRay(Input.mousePosition), out rayHit, Mathf.Infinity, SelectionMask))
            {
                // Check if the hit GameObject is valid
                if (SceneSelectables.Contains(rayHit.collider.gameObject))
                {
                    // If the hit GameObject is already selected, and it's being told to toggle off
                    // Then clear it from the SelectedObjects list
                    if (toggleOff && SelectedObjects.Contains(rayHit.collider.gameObject))
                    {
                        ClearFromSelected(rayHit.collider.gameObject);
                    }
                    // Add it to the SelectedObjects list
                    else
                    {
                        AddSelected(rayHit.collider.gameObject);
                    }
                }
            }
        }

        /// <summary>
        /// Get all of the GameObjects within the selection area
        /// </summary>
        void SelectMultiple()
        {
            // Add all valid GameObjects to the SelectedObjects list
            Helper.LoopList_ForEach<GameObject>(Helper.GetObjectsInViewport(SceneSelectables, SelectorCam, vpStartPos, vpEndPos, SelectionMask),
            // Loop action
            (GameObject obj) =>
            {
                AddSelected(obj);
            });

            // Add the GameObjects directly under the mouse to the SelectedObjects list if there is one
            SelectSingle(false);
        }

        void ClearHighlighted()
        {
            Helper.LoopList_ForEach<GameObject>(highlightedObjects,
            // Loop action
            (GameObject go) =>
            {
                // Check if the object we're checking are selected
                if (SelectedObjects.Contains(go))
                {
                    Helper.SendMessageToChain(go, ISelectableEnum.SetSelected.ToString());
                }
                // if not, then we will get all of the 'ISelectables' on it to deselect
                else
                {
                    Helper.SendMessageToChain(go, ISelectableEnum.SetDeselected.ToString());
                }
            });

            highlightedObjects.Clear();
        }

        /// <summary>
        /// Clear all of the SelectedObjects and tell them to deselect
        /// </summary>
        void ClearSelection()
        {
            // Deselect currently selected GameObject
            Helper.LoopList_ForEach<GameObject>(SelectedObjects,
            // Loop action    
            (GameObject selected) =>
            {
                Helper.SendMessageToChain(selected, ISelectableEnum.OnDeselect.ToString());
            });

            SelectedObjects.Clear();
        }

        /// <summary>
        /// Deselect a single GameObject and remove it from the SelectedObjects list
        /// </summary>
        /// <param name="obj">The GameObject to deselect and remove</param>
        void ClearFromSelected(GameObject obj)
        {
            if (Helper.IsNullOrDestroyed(obj)) return;

            // If the GameObject is actually selected then deselect and remove from lists
            if (SelectedObjects.Contains(obj))
            {
                Helper.SendMessageToChain(obj, ISelectableEnum.OnDeselect.ToString());
                SelectedObjects.Remove(obj);
            }
        }

        /// <summary>
        /// Send a command to the server to add an AIAction to an AIAgent's queue, the parameters to be used in the SetVariables(...) call 
        /// </summary>
        /// <param name="agent">The AIAgent to add the AIAction to</param>
        /// <param name="actionName">The name of the AIAction to lookup in the AIAgent's 'PossibleActions' dictionary</param>
        /// <param name="target">The GameObject that the client is telling the AIAction to target</param>
        /// <param name="targetPos">The Vector3 that the client is telling the AIAction to use</param>
        /// <param name="layer">The layer index that the client is telling the AIAction to use (this is used due to layer discrepancies from Server->Client)</param>
        /// <param name="addToList">Whether the AIAction should be added to the queue or nots</param>
        [Command]
        public void CmdAddAction(GameObject agent, string actionName, GameObject target, Vector3 targetPos, int layer, bool addToList)
        {
            if (Helper.IsNullOrDestroyed(agent))
            {
                DebugManager.WarningMessage("Attempting to add an AIAction to an AIAgent Server side!");
                return;
            }

            AIAgent ai = agent.GetComponent<AIAgent>();

            if (ai)
            {
                ai.AddAction(actionName, addToList, target, targetPos, layer);
            }
        }

        /// <summary>
        /// Spawn a GameObject on the server, propagates to all clients
        /// </summary>
        /// <param name="objToSpawn">GameObject to spawn, needs a NetworkIdentity and be apart of the SpawnablePrefabs list</param>
        [Server]
        public void ServSpawnObject(GameObject objToSpawn)
        {
            if (Helper.IsNullOrDestroyed(objToSpawn))
            {
                DebugManager.WarningMessage("Attempting to spawn a null GameObject Server side!");
                return;
            }

            NetworkServer.Spawn(objToSpawn);
        }
        /// <summary>
        /// Destroy a GameObject on the server, propagates to all clients
        /// </summary>
        /// <param name="objToDestroy">GameObject to destroy, needs a NetworkIdentity</param>
        [Server]
        public void ServDestroyObject(GameObject objToDestroy)
        {
            if (Helper.IsNullOrDestroyed(objToDestroy))
            {
                DebugManager.WarningMessage("Attempting to destroy a null GameObject Server side!");
            }

            NetworkServer.Destroy(objToDestroy);
        }

        /// <summary>
        /// Send a command to the server to inform an AIAgent that it has an owner
        /// </summary>
        /// <param name="agent">The AIAgent to update</param>
        /// <param name="owner">The new owner of the AIAgent</param>
        [Command]
        public void CmdSetAgentOwner(GameObject agent, GameObject owner)
        {
            if (Helper.IsNullOrDestroyed(agent) || Helper.IsNullOrDestroyed(owner))
            {
                DebugManager.WarningMessage("Failed to set the owner of an AIAgent Server side!");
                return;
            }

            // Tell all clients that the AIAgent has a new owner
            RpcSetAgentOwner(agent, owner);
        }
        /// <summary>
        /// Inform all clients that an AIAgent has been assigned an owner
        /// </summary>
        /// <param name="agent">THe AIAgent to update</param>
        /// <param name="owner">The new owner of the AIAgent</param>
        [ClientRpc]
        public void RpcSetAgentOwner(GameObject agent, GameObject owner)
        {
            if (Helper.IsNullOrDestroyed(agent) || Helper.IsNullOrDestroyed(owner))
            {
                DebugManager.WarningMessage("Failed to set the owner of an AIAgent Client side!");
                return;
            }

            AIAgent ai = Helper.GetComponent<AIAgent>(agent);
            NetworkPlayer player = Helper.GetComponent<NetworkPlayer>(owner);

            if (ai && player)
            {
                // Tell the AIAgent of its new owner
                ai.AgentOwner = player;

                // If the player that owns the AIAgent is the local player, then the AIAgent will be assigned to a 'friendly' layer
                if (player.isLocalPlayer)
                {
                    ai.gameObject.layer = LayerMask.NameToLayer(player.FriendlyLayer);
                }
                // If the player that owns the AIAgent is not the local player, then the AIAgent will be assigned to an 'enemy' layer
                else
                {
                    ai.gameObject.layer = LayerMask.NameToLayer(player.EnemyLayer);
                }
            }
        }
    }
}