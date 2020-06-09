using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RTS_System.AI
{
    public class ActionUI : MonoBehaviour
    {
        public List<string> WordsToExcludeInDisplayName = new List<string>();
        [Space]
        public TextMeshProUGUI ActionDisplayText = null;
        public Image ActionDisplayImage = null;
        [Space]
        [Range(0, 255)] public int ImageAlpha = 255;

        public void UpdateActionUI(AIAction actionToDisplay, string actionInput = "")
        {
            if (!actionToDisplay) return;

            if (ActionDisplayImage)
            {
                ActionDisplayImage.sprite = actionToDisplay.ActionIcon;
                
                Color newAlpha = ActionDisplayImage.color;
                newAlpha.a = ImageAlpha / 255.0f;
                ActionDisplayImage.color = newAlpha;
            }

            if (ActionDisplayText)
            {
                string actionName = actionToDisplay.ActionName;

                if (WordsToExcludeInDisplayName.Count > 0)
                {
                    actionName = Helper.MultiExludeInString(actionName, WordsToExcludeInDisplayName);
                }
                
                actionInput = Helper.SeparateByUpperCase(actionInput);
                actionName = actionName + " " + actionInput;

                ActionDisplayText.text =  actionName;
            }
        }
    }
}