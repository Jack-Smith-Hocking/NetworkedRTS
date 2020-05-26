using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Selector_System
{
    public interface ISelectable
    {
        #region Materials
        void SetHover();
        void SetSelected();
        void SetDeselected();
        #endregion


        #region Selections
        void OnHover();
        void OffHover();

        void OnSelect();
        void OnDeselect();

        void OnExecute();
        #endregion
    }
}