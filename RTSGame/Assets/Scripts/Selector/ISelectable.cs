using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Selector_System
{
    public interface ISelectable
    {
        void OnHover();
        void OnSelect();
        void OnDeselect();

        void OnExecute();
    }
}