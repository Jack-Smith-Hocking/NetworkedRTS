using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using RTS_System.Utility;
using UnityEngine.UI;

namespace RTS_System.UI
{
    public class UIDisplay : MonoBehaviour
    {
        [Tooltip("Worlds to excluding when displaying text")] public List<string> WordsToExcludeInDisplayName = new List<string>();
        [Space]
        [Tooltip("The UI text element to display details")] public TextMeshProUGUI DisplayText = null;
        [Tooltip("The image to display an icon")] public Image DisplayImage = null;
        [Tooltip("Whether or not to display the name of given")] public bool WriteName = true;
        [Space]
        [Range(0, 255)] [Tooltip("Opacity to use for the image")] public int ImageAlpha = 255;

        /// <summary>
        /// Set up the UIDisplay's image and text components to display the correct details
        /// </summary>
        /// <param name="icon">The sprite to display in the DisplayImage</param>
        /// <param name="displayName">The name field to be displayed in DisplayText</param>
        /// <param name="extraText">Extra information to be displayed by DisplayText</param>
        public void InitialiseUI(Sprite icon, string displayName, string extraText = "")
        {
            UpdateImage(icon);
            UpdateText(displayName, extraText);
        }

        public void UpdateText(string displayName, string extraText = "")
        {
            // Set up text
            if (DisplayText)
            {
                string actionName = displayName;

                if (WordsToExcludeInDisplayName.Count > 0)
                {
                    actionName = Helper.MultiExludeInString(actionName, WordsToExcludeInDisplayName);
                }

                extraText = Helper.SeparateByUpperCase(extraText);
                actionName = (WriteName ? actionName + " " : "") + extraText;

                DisplayText.text = actionName;
            }
        }
        public void UpdateImage(Sprite icon)
        {
            // Set up image
            if (DisplayImage)
            {
                DisplayImage.sprite = icon;

                Color newAlpha = DisplayImage.color;
                newAlpha.a = ImageAlpha / 255.0f;
                DisplayImage.color = newAlpha;
            }
        }
    }
}