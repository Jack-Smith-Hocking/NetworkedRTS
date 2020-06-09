using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    [Tooltip("Text object to display the number of health points left")] public TextMeshProUGUI HealthText = null;
    [Tooltip("The camera that the HealthBar will look at")] public Camera HealthBarCamera = null;
    [Tooltip("Whether the HealthBar should look at the provided camera")] public bool HealthBarFollowCamera = true;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();

        if (HealthImage)
        {
            HealthImage.color = HealthImageColour;
        }
        if (!HealthBarCamera)
        {
            HealthBarCamera = Camera.main;
        }
        if (HealthText)
        {
            HealthText.text = $"Health: {HealthDisplay.CurrentHealth} / {HealthDisplay.MaxHealth}";
        }

        if (HealthDisplay)
        {
            // Update the UI when the associated Health takes damage rather than in Update()
            HealthDisplay.OnTakeDamageSimple += (float damage) =>
            {
                if (HealthImage)
                {
                    HealthImage.fillAmount = HealthDisplay.HealthRatio;
                }
                if (HealthText)
                {
                    HealthText.text = $"Health: {HealthDisplay.CurrentHealth} / {HealthDisplay.MaxHealth}";
                }
            };
        }
    }

    void UpdateFacing()
    {
        // Make the UI follow the camera
        if (HealthBarFollowCamera && HealthRotateObject && HealthBarCamera)
        {
            Vector3 camFor = HealthBarCamera.transform.forward;

            HealthRotateObject.transform.forward = camFor;
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
        if (HealthImage)
        {
            HealthImage.gameObject.SetActive(displayActive);
        }
        if (HealthText)
        {
            HealthText.gameObject.SetActive(displayActive);
        }
    }

    public void TurnOffAfterDelay(float delay)
    {
        StopAllCoroutines();
        StartCoroutine(DoAfterDelay(delay, () =>
        {
            if (HealthImage)
            {
                HealthImage.gameObject.SetActive(false);
            }
            if (HealthText)
            {
                HealthText.gameObject.SetActive(false);
            }
        }));
    }

    public IEnumerator DoAfterDelay(float delay, Action doAction)
    {
        yield return new WaitForSecondsRealtime(delay);

        doAction?.Invoke();
    }
}
