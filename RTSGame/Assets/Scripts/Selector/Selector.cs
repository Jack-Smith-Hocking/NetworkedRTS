using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using Mirror;
using RTS_System.AI;    

namespace RTS_System.Selection
{
    public class Selector : NetworkBehaviour
    {
        public Camera SelectorCam = null;
        public LayerMask SelectionMask;
        public List<GameObject> SceneSelectables = new List<GameObject>();

        [Tooltip("Determines the input for the main select button")] public InputActionReference MainSelectButton;
        [Tooltip("Determines the input for the multi-select button")] public InputActionReference MultiSelectInput;
        [Tooltip("Determines the input for adding an action to a queue")] public InputActionReference ActionQueueInput;
        [Space]


        public float DragDelay = 0.1f;
        private float currentDragTime = 0;
        public bool AddToActionList { get; private set; } = false;

        private List<ISelectable> selectables = new List<ISelectable>();
        private List<ISelectable> currentSelectables = new List<ISelectable>();

        private List<GameObject> selectedObjects = new List<GameObject>();
        private List<GameObject> highlightedObjects = new List<GameObject>();

        private Vector3 startPos;
        private Vector3 endPos;

        private BoundInput selectInput = new BoundInput();
        private BoundInput multiSelectInput = new BoundInput();
        private BoundInput actionQueueInput = new BoundInput();

        // Start is called before the first frame update
        void Start()
        {
            if (!isLocalPlayer) return;

            if (!SelectorCam)
            {
                SelectorCam = Camera.main;
            }

            // Set up the select action
            selectInput.PerformedActions += Select;
            selectInput.CancelledActions += Select;
            selectInput.Bind(MainSelectButton);

            // Set up the multi-select button
            multiSelectInput.Bind(MultiSelectInput);
            // Set up the queue button
            actionQueueInput.Bind(ActionQueueInput);
        }

        bool isDown = false;
        void Select(InputAction.CallbackContext cc)
        {
            isDown = cc.ReadValueAsButton();

            if (multiSelectInput.CurrentBoolVal == false)
            {
                ClearSelection();
            }

            if (isDown)
            {
                startPos = SelectorCam.ScreenToViewportPoint(Input.mousePosition);
                currentDragTime = Time.time;
            }
            else
            {
                endPos = SelectorCam.ScreenToViewportPoint(Input.mousePosition);

                highlightedObjects.Clear();

                if (Time.time - currentDragTime >= DragDelay)
                {
                    SelectMultiple();
                }
                else
                {
                    SelectSingle();
                }
            }
        }

        private void Update()
        {
            if (!isLocalPlayer) return;

            AddToActionList = actionQueueInput.CurrentBoolVal;

            currentSelectables.Clear();

            Helper.LoopList_ForEach<GameObject>(highlightedObjects, (GameObject go) => 
            {
                if (selectedObjects.Contains(go))
                {
                    Helper.LoopList_ForEach<ISelectable>(go.GetComponentsInChildren<ISelectable>().ToList<ISelectable>(), (ISelectable s) =>
                    {
                        s.SetSelected();
                    });
                }
                else
                {
                    currentSelectables.AddRange(go.GetComponentsInChildren<ISelectable>());
                }
            });
            Helper.LoopList_ForEach<ISelectable>(currentSelectables, (ISelectable s) =>
            {
                s.SetDeselected();
            });

            highlightedObjects.Clear();

            if (isDown)
            {
                endPos = SelectorCam.ScreenToViewportPoint(Input.mousePosition);

                highlightedObjects.AddRange(Helper.GetObjectsInViewport(SceneSelectables, SelectorCam, startPos, endPos, SelectionMask));

                currentSelectables.Clear();
                Helper.LoopList_ForEach<GameObject>(highlightedObjects, (GameObject go) => 
                {
                    currentSelectables.AddRange(go.GetComponentsInChildren<ISelectable>());
                });

                Helper.LoopList_ForEach<ISelectable>(currentSelectables, (ISelectable s) => 
                {
                    s.SetHover();
                });
            }
            else
            {
                currentSelectables.Clear();

                RaycastHit rayHit;
                if (Physics.Raycast(SelectorCam.ScreenPointToRay(Input.mousePosition), out rayHit, Mathf.Infinity, SelectionMask))
                {
                    if (SceneSelectables.Contains(rayHit.collider.gameObject))
                    {
                        highlightedObjects.Add(rayHit.collider.gameObject);
                        currentSelectables.AddRange(rayHit.collider.GetComponentsInChildren<ISelectable>());
                    }
                }
                Helper.LoopList_ForEach<ISelectable>(currentSelectables, (ISelectable s) =>
                {
                    s.SetHover();
                });
            }
        }

        public void AddSelected(GameObject selected)
        {
            if (!selected)
            {
                return;
            }

            if (selectedObjects.Contains(selected))
            {
                return;
            }

            selectedObjects.Add(selected);
            
            currentSelectables.Clear();
            currentSelectables.AddRange(selected.GetComponentsInChildren<ISelectable>());
            Helper.LoopList_ForEach<ISelectable>(currentSelectables, (ISelectable selectable) => { selectable.OnSelect(); });

            selectables.AddRange(currentSelectables);
        }

        void SelectSingle(bool toggleOff = true)
        {
            RaycastHit rayHit;
            if (Physics.Raycast(SelectorCam.ScreenPointToRay(Input.mousePosition), out rayHit, Mathf.Infinity, SelectionMask))
            {
                if (SceneSelectables.Contains(rayHit.collider.gameObject))
                {
                    if (toggleOff && selectedObjects.Contains(rayHit.collider.gameObject))
                    {
                        ClearFromSelected(rayHit.collider.gameObject);
                    }
                    else
                    {
                        AddSelected(rayHit.collider.gameObject);
                    }
                }
            }
        }

        void SelectMultiple()
        {
            Helper.LoopList_ForEach<GameObject>(Helper.GetObjectsInViewport(SceneSelectables, SelectorCam, startPos, endPos, SelectionMask), (GameObject obj) => { AddSelected(obj); });
            SelectSingle(false);
        }

        void ClearSelection()
        {
            Helper.LoopList_ForEach<ISelectable>(selectables, (ISelectable selectable) => { selectable.OnDeselect(); });

            selectables.Clear();
            selectedObjects.Clear();
        }

        void ClearFromSelected(GameObject obj)
        {
            if (obj == null) return;

            currentSelectables.Clear();
            currentSelectables.AddRange(obj.GetComponentsInChildren<ISelectable>());
            foreach (ISelectable s in currentSelectables)
            {
                s.OnDeselect();

                selectables.Remove(s);
            }

            currentSelectables.Clear();
            selectedObjects.Remove(obj);
        }

        [Command]
        public void CmdAddAction(GameObject agent, string actionName, GameObject target, Vector3 targetPos, bool addToList)
        {
            if (!agent) return;

            AIAgent ai = agent.GetComponent<AIAgent>();

            if (ai && AIAction.ActionTable.ContainsKey(actionName))
            {
                AIAction action = Instantiate(AIAction.ActionTable[actionName]);

                action.InitialiseAction(ai);
                action.SetVariables(ai, target, targetPos);

                ai.AddAction(action, addToList, false, false);
            }
        }

        [Server]
        public void ServSpawnObject(GameObject objToSpawn)
        {
            NetworkServer.Spawn(objToSpawn);
        }
        [Server]
        public void ServDestroyObject(GameObject objToDestroy)
        {
            NetworkServer.Destroy(objToDestroy);
        }

        [ClientRpc]
        public void RpcSetAgentOwner(GameObject agent, GameObject owner)
        {
            if (!agent || !owner) return;

            AIAgent ai = Helper.GetComponent<AIAgent>(agent);
            NetworkPlayer player = Helper.GetComponent<NetworkPlayer>(owner);

            if (ai && player)
            {
                ai.AgentOwner = player;

                if (player.isLocalPlayer)
                {
                    ai.gameObject.layer = LayerMask.NameToLayer(player.FriendlyLayer);
                }
                else
                {
                    ai.gameObject.layer = LayerMask.NameToLayer(player.EnemyLayer);
                }
            }
        }
    }
}