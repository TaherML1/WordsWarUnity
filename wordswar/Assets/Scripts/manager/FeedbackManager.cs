using System.Collections;
using UnityEngine;
using TMPro;

public class FeedbackManager : MonoBehaviour
{
    public TextMeshProUGUI feedbackText;
    public float animationDuration = 0.5f; // Duration for the fade-out animation

    private void Start()
    {
        // Ensure the feedback message is hidden at the start
        feedbackText.gameObject.SetActive(false);
    }

    public void ShowFeedback(string message)
    {
        StartCoroutine(ShowFeedbackCoroutine(message));
    }

    private IEnumerator ShowFeedbackCoroutine(string message)
    {
        // Set the message and show the text
        feedbackText.text = message;
        feedbackText.gameObject.SetActive(true);

        // Wait for the duration of the message display
        yield return new WaitForSeconds(3f);

        // Fade out the message
        LeanTween.alphaText(feedbackText.rectTransform, 0f, animationDuration);

        // Wait for the fade-out animation to complete
        yield return new WaitForSeconds(animationDuration);

        // Hide the message
        feedbackText.gameObject.SetActive(false);
    }
}
