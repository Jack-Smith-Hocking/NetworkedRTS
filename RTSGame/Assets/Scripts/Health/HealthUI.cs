using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [Tooltip("The Health script to display")] public Health HealthDisplay = null;

    [Space]

    [Header("Health Bar Details")]
    [Tooltip("Object to rotate to look at camera")] public GameObject HealthRotateObject = null;
    [Tooltip("The health image to represent current health")] public Image HealthImage = null;
    [Tooltip("The tint of the health image")] public Color HealthImageColour = Color.white;
    [Tooltip("The camera that the HealthBar will look at")] public Camera HealthBarCamera = null;
    [Tooltip("Whether the HealthBar should look at the provided camera")] public bool HealthBarFollowCamera = true;

    // Start is called before the first frame update
    void Start()
    {
        if (HealthImage)
        {
            HealthImage.color = HealthImageColour;
        }
        if (!HealthBarCamera)
        {
            HealthBarCamera = Camera.main;
        }

        if (HealthDisplay)
        {
            // Update the UI when the associate Health takes damage rather than in Update()
            HealthDisplay.OnTakeDamageSimple += (float damage) =>
            {
                if (HealthImage)
                {
                    HealthImage.fillAmount = HealthDisplay.HealthRatio;
                }
            };
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Make the UI follow the camera
        if (HealthBarFollowCamera && HealthRotateObject)
        {
            Vector3 camFor = HealthBarCamera.transform.forward;
            camFor.y = 0;

            HealthRotateObject.transform.forward = camFor;
        }
    }
}
