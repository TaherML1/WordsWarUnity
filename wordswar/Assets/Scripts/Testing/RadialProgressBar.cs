using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RadialProgressBar : MonoBehaviour
{
    public GameObject BGimageProgressBar;
    public Image progressBar; // Reference to the radial progress bar Image
    public float rotationSpeed = 200f; // Speed of the continuous rotation

    private bool isRotating = false; // Flag to control the rotation

    private void Start()
    {
        // Initially, the progress bar can be hidden or inactive as needed

    }

    // Function to start the rotation and show the progress bar
    public void StartSpinning()
    {
        isRotating = true;
       BGimageProgressBar.SetActive(true);
        StartCoroutine(ContinuousRotation());
    }

    // Function to stop the rotation and hide the progress bar
    public void StopSpinning()
    {
        isRotating = false;
        BGimageProgressBar.SetActive(false);
       
    }

    // Function to set the fill amount to a specific value
    public void SetFillAmount(float amount)
    {
        progressBar.fillAmount = Mathf.Clamp01(amount); // Clamp to ensure value is between 0 and 1
    }

    private IEnumerator ContinuousRotation()
    {
        while (isRotating)
        {
            // Rotate the progress bar
            progressBar.transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
