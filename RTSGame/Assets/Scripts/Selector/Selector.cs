using Unit_System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Selector_System
{
    public class Selector : MonoBehaviour
    {
        public static Selector Instance = null;
        public List<GameObject> SceneSelectables = new List<GameObject>();

        [Space]
        [Tooltip("Determines the input for the main select button")] public InputActionReference MainSelectButton;
        [Tooltip("Determines the input for the multi-select button")] public InputActionReference MultiSelectInput;
        [Tooltip("Determines the input for adding an action to a queue")] public InputActionReference ActionQueueInput;
        [Space]

        public float DragDelay = 0.1f;
        private float currentDragTime = 0;

        public bool AddToActionList { get; private set; } = false;

        [Space]
        public Material NormalMat;
        public Material HighlightedMat;
        public Material SelectedMat;
        [Space]

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
            if (!Instance)
            {
                Instance = this;
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

        void Select(InputAction.CallbackContext cc)
        {
            bool isDown = cc.ReadValueAsButton();

            if (multiSelectInput.CurrentBoolVal == false)
            {
                ClearSelection();
            }

            if (isDown)
            {
                startPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
                currentDragTime = Time.time;
            }
            else
            {
                endPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);

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
            AddToActionList = actionQueueInput.CurrentBoolVal;

            Helper.SetMaterials(highlightedObjects, NormalMat);

            foreach (GameObject obj in highlightedObjects)
            {
                if (selectedObjects.Contains(obj))
                {
                    Helper.SetMaterial(obj, SelectedMat);
                }
            }
            highlightedObjects.Clear();

            RaycastHit rayHit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out rayHit))
            {
                if (SceneSelectables.Contains(rayHit.collider.gameObject))
                {
                    rayHit.collider.gameObject.GetComponent<Renderer>().material = HighlightedMat;
                    highlightedObjects.Add(rayHit.collider.gameObject);
                }
            }

            if (selectInput.CurrentBoolVal)
            {
                endPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
                highlightedObjects.AddRange(GetObjectsInArea());
                Helper.SetMaterials(highlightedObjects, HighlightedMat);
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
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out rayHit))
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

            Helper.SetMaterials(selectedObjects, SelectedMat);
        }

        void SelectMultiple()
        {
            Helper.LoopList_ForEach<GameObject>(GetObjectsInArea(), (GameObject obj) => { AddSelected(obj); });
            SelectSingle(false);

            Helper.SetMaterials(selectedObjects, SelectedMat);
        }

        List<GameObject> GetObjectsInArea()
        {
            Rect selectRect = new Rect(startPos.x, startPos.y, (endPos.x - startPos.x), (endPos.y - startPos.y));

            List<GameObject> objs = new List<GameObject>();

            foreach (GameObject selectableObj in SceneSelectables)
            {
                if (selectableObj != null)
                {
                    if (selectRect.Contains(Camera.main.WorldToViewportPoint(selectableObj.transform.position), true))
                    {
                        objs.Add(selectableObj);
                    }
                }
            }

            return objs;
        }

        void ClearSelection()
        {
            Helper.LoopList_ForEach<ISelectable>(selectables, (ISelectable selectable) => { selectable.OnDeselect(); });

            Helper.SetMaterials(selectedObjects, NormalMat);

            selectables.Clear();
            selectedObjects.Clear();
        }

        void ClearFromSelected(GameObject obj)
        {
            if (obj == null) return;

            Helper.SetMaterial(obj, NormalMat);

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
    }
}