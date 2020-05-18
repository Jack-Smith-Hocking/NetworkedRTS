using AI_System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Selector_Systen
{
    public class Selector : MonoBehaviour
    {
        [System.Serializable]
        public class SelectorInput
        {
            public string InputName = "New Input";
            public AIAction InputAction = null;
        }

        public static Selector Instance = null;

        public PlayerInput PlayerInput;
        public LayerMask MovementLayer;

        [Space]
        public string MainSelectButton = "LeftMouseClick";
        public string MultiSelectInput = "LeftCtr";
        public string ActionQueueInput = "LeftShift";
        [Space]

        [Space]
        public List<SelectorInput> SelectorInputs = new List<SelectorInput>();
        [Space]

        public AIAction CurrentAction = null;
        public Vector3 SelectedPoint;
        public bool SetPoint = false;
        public bool AddToPath = false;
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

        private List<BoundInput> boundInputs = new List<BoundInput>();

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

            foreach (SelectorInput s in SelectorInputs)
            {
                if (s.InputAction)
                {
                    s.InputAction = Instantiate(s.InputAction);
                }

                InitialiseInputs(s);
            }

            selectInput.PerformedActions += Select;
            selectInput.CancelledActions += Select;
            selectInput.Bind(PlayerInput, MainSelectButton);

            multiSelectInput.Bind(PlayerInput, MultiSelectInput);
            actionQueueInput.Bind(PlayerInput, ActionQueueInput);
        }

        void InitialiseInputs(SelectorInput selection)
        {
            if (selection == null) return;

            BoundInput binding = new BoundInput();
            binding.PerformedActions += (InputAction.CallbackContext cc) =>
            {
                ExecuteSelected(selection.InputAction);
            };

            binding.Bind(PlayerInput, selection.InputName);

            boundInputs.Add(binding);
        }

        void Select(InputAction.CallbackContext cc)
        {
            bool isDown = cc.ReadValueAsButton();

            if (isDown)
            {
                startPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
                currentDragTime = Time.time;
            }
            else
            {
                endPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);

                if (multiSelectInput.CurrentBoolVal == false)
                {
                    ClearSelection();
                }

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
            SetMaterial(highlightedObjects, NormalMat);
            highlightedObjects.Clear();

            RaycastHit rayHit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out rayHit))
            {

                if (!selectedObjects.Contains(rayHit.collider.gameObject) && Helper.ObjectInMonoList<AIAgent>(AIManager.Instance.SceneAI, rayHit.collider.gameObject))
                {
                    rayHit.collider.gameObject.GetComponent<Renderer>().material = HighlightedMat;
                    highlightedObjects.Add(rayHit.collider.gameObject);
                }
            }

            if (selectInput.CurrentBoolVal)
            {
                endPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
                highlightedObjects.AddRange(GetObjectsInArea());
                SetMaterial(highlightedObjects, HighlightedMat);
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

        void SelectSingle()
        {
            RaycastHit rayHit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out rayHit))
            {
                AddSelected(rayHit.collider.gameObject);
            }

            SetMaterial(selectedObjects, SelectedMat);
        }
        void SelectMultiple()
        {
            Helper.LoopListForEach<GameObject>(GetObjectsInArea(), (GameObject obj) => { AddSelected(obj); });
            SelectSingle();

            SetMaterial(selectedObjects, SelectedMat);
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

        void SetMaterial(List<GameObject> list, Material mat)
        {
            Renderer rend = null;
            Helper.LoopListForEach<GameObject>(list, (GameObject obj) =>
                {
                    if (!Helper.ObjectInMonoList(AIManager.Instance.SceneAI, obj)) return;

                    rend = obj.GetComponent<Renderer>();
                    if (rend)
                    {
                        rend.material = mat;
                    }
                }
            );
        }

        void ClearSelection()
        {
            foreach (GameObject obj in selectedObjects)
            {
                selectables.AddRange(obj.GetComponentsInChildren<ISelectable>());
            }

            Helper.LoopListForEach<ISelectable>(selectables, (ISelectable selectable) => { selectable.OnDeselect(); });

            SetMaterial(selectedObjects, NormalMat);

            selectables.Clear();
            selectedObjects.Clear();
        }

        void ExecuteSelected(AIAction action)
        {
            if (action == null) return;

            CurrentAction = Instantiate(action);

            AddToActionList = actionQueueInput.CurrentBoolVal;

            Helper.LoopListForEach<ISelectable>(selectables, (ISelectable selectable) => { selectable.OnExecute(); });
        }
    }
}