using UnityEngine;
using UnityEngine.UI;
using TMPro; // For TextMeshPro support

public class MessageAnimator : MonoBehaviour
{
    public GameObject messagePrefab; // Prefab of the message bubble
    public RectTransform messagesContainer; // Container to hold the messages (Parent RectTransform)

    // Method to show a message with animation
    public void ShowMessage(string message, bool isLocalPlayer)
    {
        // Instantiate the message prefab
        GameObject messageInstance = Instantiate(messagePrefab, messagesContainer);

        // Set the message text (Assuming your prefab has a TMP_Text component for message display)
        TMP_Text messageText = messageInstance.GetComponentInChildren<TMP_Text>();
        messageText.text = message;

        // Get the RectTransform of the instantiated message
        RectTransform messageRectTransform = messageInstance.GetComponent<RectTransform>();

        // Determine the start position based on the player type
        Vector2 startPosition = isLocalPlayer ? new Vector2(messagesContainer.rect.width, 0) : new Vector2(-messagesContainer.rect.width, 0);

        // Set the initial position outside the screen
        messageRectTransform.anchoredPosition = startPosition;

        // Define the target position inside the container
        Vector2 targetPosition = Vector2.zero; // Adjust this based on your desired final position

        // Animate the message
        LeanTween.move(messageRectTransform, targetPosition, 0.5f).setEase(LeanTweenType.easeOutCubic);

        // Optionally, you can add a delay before destroying or reusing the message
        Destroy(messageInstance, 5f); // Destroy after 5 seconds
    }
}
