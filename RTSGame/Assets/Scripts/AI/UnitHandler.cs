using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Action_System;
using Selector_Systen;
using System;

[RequireComponent(typeof(AIAgent))]
public class UnitHandler : MonoBehaviour, ISelectable
{
    [System.Serializable]
    public class SelectorInput
    {
        public string SelectorInputName;
        public InputActionReference InputAction = null;
        public AIAction Action = null;

        public BoundInput BoundInput = new BoundInput();
    }

    public AIAgent Agent = null;

    public List<SelectorInput> SelectionInputs = new List<SelectorInput>();

    // Start is called before the first frame update
    void Start()
    {
        if (!Agent) Agent = gameObject.GetComponent<AIAgent>();

        Action<SelectorInput> loopAction = (SelectorInput s) =>
        {
            if (s.Action != null)
            {
                s.BoundInput.PerformedActions += (InputAction.CallbackContext cc) =>
                {
                    Agent.AddAction(s.Action, Selector.Instance.AddToActionList);
                };
            }
        };

        Helper.LoopListForEach<SelectorInput>(SelectionInputs, loopAction);
    }

    public void BindInput(SelectorInput s)
    {
        if (s != null)
        {
            s.BoundInput.Bind(s.InputAction);
        }
    }
    public void BindAllInputs()
    {
        Helper.LoopListForEach<SelectorInput>(SelectionInputs, BindInput);
    }

    public void UnbindInput(SelectorInput s)
    {
        if (s != null)
        {
            s.BoundInput.Unbind(s.InputAction);
        }
    }
    public void UnbindAllInputs()
    {
        Helper.LoopListForEach<SelectorInput>(SelectionInputs, UnbindInput);
    }

    #region ISelectable
    public void OnHover()
    {
    }

    public void OnSelect()
    {
        BindAllInputs();
    }

    public void OnDeselect()
    {
        UnbindAllInputs();
    }

    public void OnExecute()
    {
    }
    #endregion
}
