using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RTS_System.UI
{
    public class DisplayCounterUI : MonoBehaviour
    {
        [Header("Display Image")]
        [Tooltip("The image to represent current value")] public Image DisplayImage = null;
        [Tooltip("The tint of the display image")] public Color ImageColour = Color.white;

        [Header("Display Text")]
        [Tooltip("Text object to display value")] public TextMeshProUGUI DisplayText = null;
        [Tooltip("The text to go before the value / maxValue")] public string PreDisplayText = "Value: ";

        [Space]
        [Tooltip("Object to rotate to look at camera")] public GameObject RotateObject = null;
        [Tooltip("The camera that the DisplayCounterUI will look at")] public Camera FollowCamera = null;
        [Tooltip("Whether the DisplayCounterUI should look at the provided camera")] public bool DoFollowCamera = true;

        // Start is called before the first frame update
        IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();

            if (DisplayImage)
            {
                DisplayImage.color = ImageColour;
            }
            if (!FollowCamera)
            {
                FollowCamera = Camera.main;
            }
        }

        public void UpdateDisplay(float newDisplayValue, float maxValue)
        {
            if (DisplayImage)
            {
                DisplayImage.fillAmount = newDisplayValue / maxValue;
            }
            if (DisplayText)
            {
                DisplayText.text = PreDisplayText + newDisplayValue + " / " + maxValue;
            }
        }

        void UpdateFacing()
        {
            // Make the UI follow the camera
            if (DoFollowCamera && RotateObject && FollowCamera)
            {
                Vector3 camFor = FollowCamera.transform.forward;

                RotateObject.transform.forward = camFor;
            }
        }

        // Update is called once per frame
        void Update()
        {
            UpdateFacing();
        }

        private void OnEnable()
        {
            UpdateFacing();
        }

        public void ToggleDisplay(bool displayActive)
        {
            if (DisplayImage)
            {
                DisplayImage.gameObject.SetActive(displayActive);
            }
            if (DisplayText)
            {
                DisplayText.gameObject.SetActive(displayActive);
            }
        }

        public void TurnOffAfterDelay(float delay)
        {
            StopAllCoroutines();
            StartCoroutine(DoAfterDelay(delay, () =>
            {
                if (DisplayImage)
                {
                    DisplayImage.gameObject.SetActive(false);
                }
                if (DisplayText)
                {
                    DisplayText.gameObject.SetActive(false);
                }
            }));
        }

        public IEnumerator DoAfterDelay(float delay, Action doAction)
        {
            yield return new WaitForSecondsRealtime(delay);

            doAction?.Invoke();
        }
    }
}