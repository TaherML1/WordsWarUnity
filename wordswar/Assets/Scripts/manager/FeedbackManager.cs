using System.Collections;
using UnityEngine;
using TMPro;

public class FeedbackManager : MonoBehaviour
{
    public TextMeshProUGUI feedbackText;
    public float animationDuration = 0.5f; // Duration for the fade-in and fade-out animations
    public float moveDistance = 50f; // Distance to move the message upwards

    private Vector3 originalPosition;

    private void Start()
    {
        // Ensure the feedback message is hidden at the start
        feedbackText.gameObject.SetActive(false);
        originalPosition = feedbackText.rectTransform.localPosition;
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

        // Fade in and move the message
        yield return StartCoroutine(FadeInAndMove());

        // Wait for the duration of the message display
        yield return new WaitForSeconds(3f);

        // Fade out and move the message
        yield return StartCoroutine(FadeOutAndMove());

        // Reset the position and hide the message
        feedbackText.rectTransform.localPosition = originalPosition;
        feedbackText.gameObject.SetActive(false);
    }

    private IEnumerator FadeInAndMove()
    {
        float elapsedTime = 0f;
        Color color = feedbackText.color;
        color.a = 0f;
        feedbackText.color = color;

        Vector3 startPosition = originalPosition;
        Vector3 targetPosition = originalPosition + new Vector3(0, moveDistance, 0);

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / animationDuration);
            feedbackText.color = color;
            feedbackText.rectTransform.localPosition = Vector3.Lerp(startPosition, targetPosition, elapsedTime / animationDuration);
            yield return null;
        }

        color.a = 1f;
        feedbackText.color = color;
        feedbackText.rectTransform.localPosition = targetPosition;
    }

    private IEnumerator FadeOutAndMove()
    {
        float elapsedTime = 0f;
        Color color = feedbackText.color;
        color.a = 1f;
        feedbackText.color = color;

        Vector3 startPosition = feedbackText.rectTransform.localPosition;
        Vector3 targetPosition = startPosition + new Vector3(0, moveDistance, 0);

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = 1f - Mathf.Clamp01(elapsedTime / animationDuration);
            feedbackText.color = color;
            feedbackText.rectTransform.localPosition = Vector3.Lerp(startPosition, targetPosition, elapsedTime / animationDuration);
            yield return null;
        }

        color.a = 0f;
        feedbackText.color = color;
        feedbackText.rectTransform.localPosition = targetPosition;
    }
}
