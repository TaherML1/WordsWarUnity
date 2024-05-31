using System.Collections;
using UnityEngine;
using TMPro;

public class FeedbackManager : MonoBehaviour
{
    public TextMeshProUGUI feedbackText;

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
        // Show the message
        feedbackText.text = message;
        feedbackText.gameObject.SetActive(true);

        // Wait for 2 seconds
        yield return new WaitForSeconds(3f);

        // Hide the message
        feedbackText.gameObject.SetActive(false);
    }
}
