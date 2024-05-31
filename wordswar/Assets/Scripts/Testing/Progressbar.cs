using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Progressbar : MonoBehaviour
{
    public Image progressBarForeground;
    public float maxValue = 100f; // Maximum value of the progress bar
    public float currentValue = 0f; // Current value of the progress bar

    // Call this method to update the progress bar
    public void SetProgress(float value)
    {
        // Ensure the value is between 0 and maxValue
        currentValue = Mathf.Clamp(value, 0f, maxValue);

        // Calculate the fill amount (value between 0 and 1)
        float fillAmount = currentValue / maxValue;

        // Set the fill amount of the foreground image
        progressBarForeground.fillAmount = fillAmount;
    }
}
