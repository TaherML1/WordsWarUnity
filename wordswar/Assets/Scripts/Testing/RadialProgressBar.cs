using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RadialProgressBar : MonoBehaviour
{
    public Image progressBar; // Reference to the radial progress bar Image
    public float duration = 3.0f; // Duration for the progress to complete
    public float rotationSpeed = 200f; // Speed of the continuous rotation

    private bool isRotating = true;

    private void Start()
    {
        // Optionally, start the progress automatically
        StartCoroutine(UpdateProgressLoop(duration));
        StartCoroutine(ContinuousRotation());
    }

    public IEnumerator UpdateProgress(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            progressBar.fillAmount = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        // Ensure the fill amount is set to 100% after the duration
        progressBar.fillAmount = 1.0f;
    }

    // Function to start the progress externally
    public void StartProgress(float taskDuration)
    {
        StartCoroutine(UpdateProgress(taskDuration));
    }


    public IEnumerator UpdateProgressLoop(float taskDuration)
    {
        while (isRotating)
        {
            float elapsed = 0f;

            while (elapsed < taskDuration)
            {
                elapsed += Time.deltaTime;
                progressBar.fillAmount = Mathf.Clamp01(elapsed / taskDuration);
                yield return null;
            }

            // Reset the fill amount to zero to start the next cycle
            progressBar.fillAmount = 0f;
        }
    }

    public IEnumerator ContinuousRotation()
    {
        while (isRotating)
        {
            // Rotate the progress bar
            progressBar.transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
            yield return null;
        }
    }

    public void StartSpinning()
    {
        isRotating = true;
        StartCoroutine(UpdateProgressLoop(duration));
        StartCoroutine(ContinuousRotation());
    }

    public void StopSpinning()
    {
        isRotating = false;
        progressBar.fillAmount = 0f; // Optionally reset the fill amount
    }
}
