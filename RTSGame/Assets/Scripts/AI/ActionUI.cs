using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RTS_System.AI
{
    public class ActionUI : MonoBehaviour
    {
        public AIAction ActionToDisplay = null;
        [Space]
        public Image ActionDisplayImage = null;
        public TextMeshProUGUI ActionDisplayText = null;

        public void UpdateActionUI()
        {
            if (!ActionToDisplay) return;

            if (ActionDisplayImage)
            {
                ActionDisplayImage.sprite = ActionToDisplay.ActionIcon;
            }

            if (ActionDisplayText)
            {
                ActionDisplayText.text = ActionToDisplay.ActionName;
            }
        }
    }
}