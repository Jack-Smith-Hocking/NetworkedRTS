using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace RTS_System.Selection
{
    public class SelectableCallback : DefaultSelectable
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