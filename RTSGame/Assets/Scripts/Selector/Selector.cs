using Action_System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Selector_Systen
{
    public class Selector : MonoBehaviour
    {
        public static Selector Instance = null;

        [Space]
        public InputActionReference MainSelectButton;
        public InputActionReference MultiSelectInput;
        public InputActionReference ActionQueueInput;
        [Space]

        public AIAction CurrentAction = null;
        public Vector3 SelectedPoint;
        public bool AddToActionList = false;

        public float DragDelay = 0.1f;
        private float currentDragTime = 0;

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

            selectInput.PerformedActions += Select;
            selectInput.CancelledActions += Select;
            selectInput.Bind(MainSelectButton);

            multiSelectInput.Bind(MultiSelectInput);
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
                if (Helper.ObjectInMonoList<AIAgent>(AIManager.Instance.SceneAI, rayHit.collider.gameObject))
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
            Helper.LoopListForEach<ISelectable>(currentSelectables, (ISelectable selectable) => { selectable.OnSelect(); });

            selectables.AddRange(currentSelectables);
        }

        void SelectSingle(bool toggleOff = true)
        {
            RaycastHit rayHit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out rayHit))
            {
                if (Helper.ObjectInMonoList<AIAgent>(AIManager.Instance.SceneAI, rayHit.collider.gameObject))
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
            Helper.LoopListForEach<GameObject>(GetObjectsInArea(), (GameObject obj) => { AddSelected(obj); });
            SelectSingle(false);

            Helper.SetMaterials(selectedObjects, SelectedMat);
        }
        List<GameObject> GetObjectsInArea()
        {
            Rect selectRect = new Rect(startPos.x, startPos.y, (endPos.x - startPos.x), (endPos.y - startPos.y));

            List<GameObject> objs = new List<GameObject>();

            GameObject agent = null;
            foreach (AIAgent agentAI in AIManager.Instance.SceneAI)
            {
                agent = agentAI.gameObject;

                if (agent != null)
                {
                    if (selectRect.Contains(Camera.main.WorldToViewportPoint(agent.transform.position), true))
                    {
                        objs.Add(agent);
                    }
                }
            }

            return objs;
        }

        void ClearSelection()
        {
            Helper.LoopListForEach<ISelectable>(selectables, (ISelectable selectable) => { selectable.OnDeselect(); });

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