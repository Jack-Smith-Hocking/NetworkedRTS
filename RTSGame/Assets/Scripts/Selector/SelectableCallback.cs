using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Selector_System
{
    public class SelectableCallback : SelectableDefault
    {
        [Header("ISelectable Callbacks")]
        public UnityEvent OnHoverEvent;
        public UnityEvent OnSelectEvent;
        public UnityEvent OnDeselectEvent;

        #region ISelectables
        public override void SetHover()
        {
            OnHoverEvent?.Invoke();
        }

        public override void SetSelected()
        {
            OnSelectEvent?.Invoke();
        }

        public override void SetDeselected()
        {
            OnDeselectEvent?.Invoke();
        }
        #endregion
    }
}