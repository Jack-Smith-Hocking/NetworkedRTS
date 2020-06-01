using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTS_System.Selection
{
    public abstract class DefaultSelectable : MonoBehaviour, ISelectable
    {
        protected virtual IEnumerator Start()
        {
            yield return new WaitForSecondsRealtime(1);

           // Helper.ListAdd<GameObject>(ref Selector.Instance.SceneSelectables, gameObject);
        }

        #region ISelectables
        public virtual void SetHover()
        {
        }

        public virtual void SetSelected()
        {
        }

        public virtual void SetDeselected()
        {
        }

        public virtual void OnHover()
        {
            SetHover();
        }

        public virtual void OffHover()
        {
            SetDeselected();
        }

        public virtual void OnSelect()
        {
            SetSelected();
        }

        public virtual void OnDeselect()
        {
            SetDeselected();
        }

        public virtual void OnExecute()
        {
        }
        #endregion
    }
}