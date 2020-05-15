using AI_System;
using ScriptableActions;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Selector_Systen
{
    public class Selector : MonoBehaviour
    {
        public static Selector Instance = null;

        public LayerMask MovementLayer;

        public Vector3 SelectedPoint;
        public bool SetPoint = false;
        public bool AddToPath = false;

        public float DragDelay = 0.1f;
        private float currentDragTime = 0;

        private List<ISelectable> selectables = new List<ISelectable>();
        private List<GameObject> selectedObjects = new List<GameObject>();

        private Vector3 startPos;
        private Vector3 endPos;

        // Start is called before the first frame update
        void Start()
        {
            if (!Instance)
            {
                Instance = this;
            }
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                startPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
                currentDragTime = Time.time;
            }
            if (Input.GetMouseButtonUp(0))
            {
                endPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);

                if (Input.GetKey(KeyCode.LeftControl) == false)
                {
                    ClearSelection();
                }

                if (Time.time - currentDragTime >= DragDelay)
                {
                
                    SelectMultiple();
                }
                else
                {
                    SelectSingle();
                }
            }

            if (selectedObjects.Count > 0)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    AddToPath = Input.GetKey(KeyCode.LeftShift);

                    RaycastHit rayHit;
                    if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out rayHit, Mathf.Infinity, MovementLayer))
                    {
                        SelectedPoint = rayHit.point;
                        Helper.LoopListForEach<ISelectable>(selectables, (ISelectable selectable) => { selectable.OnExecute(); });
                    }
                }
            }
        }

        public void AddSelected(GameObject selected)
        {
            if (!selected)
            {
                return;
            }

            selectedObjects.Add(selected);
            selectables.AddRange(selected.GetComponentsInChildren<ISelectable>());
        }

        void SelectSingle()
        {
            RaycastHit rayHit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out rayHit))
            {
                AddSelected(rayHit.collider.gameObject);
            }

            Helper.LoopListForEach<ISelectable>(selectables, (ISelectable selectable) => { selectable.OnSelect(); });
        }
        void SelectMultiple()
        {
            Rect selectRect = new Rect(startPos.x, startPos.y, (endPos.x - startPos.x), (endPos.y - startPos.y));

            GameObject agent = null;
            foreach (AIAgent agentAI in AIManager.Instance.SceneAI)
            {
                agent = agentAI.gameObject;

                if (agent != null)
                {
                    if (selectRect.Contains(Camera.main.WorldToViewportPoint(agent.transform.position), true))
                    {
                        AddSelected(agent);
                    }
                }
            }

            Helper.LoopListForEach<ISelectable>(selectables, (ISelectable selectable) => { selectable.OnSelect(); });
        }

        void ClearSelection()
        {
            foreach (GameObject obj in selectedObjects)
            {
                selectables.AddRange(obj.GetComponentsInChildren<ISelectable>());
            }

            Helper.LoopListForEach<ISelectable>(selectables, (ISelectable selectable) => { selectable.OnDeselect(); });

            selectables.Clear();
            selectedObjects.Clear();
        }
    }
}