using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Selector_System
{
    public class SelectorUI : MonoBehaviour
    {
        public Camera SelectorCam = null;
        public RectTransform SelectorTrans = null;

        private Vector3 startPos;
        private Vector3 endPos;

        private void Start()
        {
            SelectorTrans.gameObject.SetActive(false);

            if (!SelectorCam)
            {
                SelectorCam = Camera.main;
            }
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit rayHit;
                if (Physics.Raycast(SelectorCam.ScreenPointToRay(Input.mousePosition), out rayHit))
                {
                    startPos = rayHit.point;

                    SelectorTrans.gameObject.SetActive(true);
                }
            }
            if (Input.GetMouseButtonUp(0))
            {
                SelectorTrans.gameObject.SetActive(false);
            }

            if (Input.GetMouseButton(0))
            {
                endPos = Input.mousePosition;

                DisplaySquare();
            }
        }

        void DisplaySquare()
        {
            //The start position of the square is in 3d space, or the first coordinate will move
            //as we move the camera which is not what we want
            Vector3 squareStartScreen = SelectorCam.WorldToScreenPoint(startPos);

            squareStartScreen.z = 0f;

            //Get the middle position of the square
            Vector3 middle = (squareStartScreen + endPos) / 2f;

            //Set the middle position of the GUI square
            SelectorTrans.position = middle;

            //Change the size of the square
            float sizeX = Mathf.Abs(squareStartScreen.x - endPos.x);
            float sizeY = Mathf.Abs(squareStartScreen.y - endPos.y);

            //Set the size of the square
            SelectorTrans.sizeDelta = new Vector2(sizeX, sizeY);
        }
    }
}
