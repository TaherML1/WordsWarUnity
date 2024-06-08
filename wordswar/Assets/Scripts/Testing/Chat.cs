using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Chat : MonoBehaviour
{
   

    public TMP_InputField playerInput;
    public GameObject MessagePrefab;
    public GameObject Content;
    public ScrollRect ScrollRect; // Reference to the ScrollRect component

    public void SendMessage()
    {
        string messageText = playerInput.text;
        if (!string.IsNullOrEmpty(messageText))
        {
            GetMessage(messageText, true); // true indicates the local player
            playerInput.text = ""; // Clear the input field after sending the message
            StartCoroutine(SmoothScrollToBottom()); // Scroll to the bottom after sending the message
        }
    }

    public void SendMessage2()
    {
        string messageText = playerInput.text;
        if (!string.IsNullOrEmpty(messageText))
        {
            GetMessage(messageText, false); // false indicates the enemy player
            playerInput.text = ""; // Clear the input field after sending the message
            StartCoroutine(SmoothScrollToBottom()); // Scroll to the bottom after sending the message
        }
    }

    public void GetMessage(string receivedMessage, bool isLocalPlayer)
    {
        GameObject messageObject = Instantiate(MessagePrefab, Content.transform);

        TextMeshProUGUI messageText = messageObject.GetComponentInChildren<TextMeshProUGUI>();
        messageText.text = receivedMessage;

        RectTransform rectTransform = messageObject.GetComponent<RectTransform>();

        if (isLocalPlayer)
        {
            rectTransform.anchorMin = new Vector2(1, 0.5f);
            rectTransform.anchorMax = new Vector2(1, 0.5f);
            rectTransform.pivot = new Vector2(1.66f, 0.5f);
            messageText.alignment = TextAlignmentOptions.Right;

            messageText.margin = new Vector4(0, 0, 10, 0);
            
        }
        else
        {
            rectTransform.anchorMin = new Vector2(0, 0.5f);
            rectTransform.anchorMax = new Vector2(0, 0.5f);
            rectTransform.pivot = new Vector2(-0.67f, 0.5f);
            messageText.alignment = TextAlignmentOptions.Left;
            messageText.margin = new Vector4(10, 0, 0, 0);
        }

        // Adjust the position to accommodate for new messages
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, -Content.transform.childCount * 20);
    
    }

    public IEnumerator SmoothScrollToBottom()
    {
        float duration = 0.3f; // Duration of the scroll in seconds
        float elapsedTime = 0f; // Time elapsed since the start of the scroll
        float startPos = ScrollRect.verticalNormalizedPosition; // Current scroll position

        // Ensure the content layout is updated before scrolling
        Canvas.ForceUpdateCanvases();

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime; // Increment elapsed time
            float newPos = Mathf.Lerp(startPos, 0f, elapsedTime / duration); // Interpolate between start and end positions
            ScrollRect.verticalNormalizedPosition = newPos; // Set the new scroll position
            yield return null; // Wait for the next frame
        }

        // Ensure the final position is exactly at the bottom
        ScrollRect.verticalNormalizedPosition = 0f;
    }
}
