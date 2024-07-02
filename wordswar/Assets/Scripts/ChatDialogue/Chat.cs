using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Chat : MonoBehaviour
{


    [Header("UI References")]
    public TMP_InputField playerInput;
    public GameObject MessagePrefab;
    public GameObject Content;
    public ScrollRect ScrollRect;

    [Header("Message Colors")]
    public Color localPlayerColor = Color.cyan;
    public Color nonLocalPlayerColor = Color.yellow;

    [Header("Animation Settings")]
    public float messageScaleDuration = 0.3f;
    public LeanTweenType messageScaleEase = LeanTweenType.easeOutBack;
    public float messageFadeDuration = 0.3f;
    public float scrollDuration = 0.3f;
    public LeanTweenType scrollEase = LeanTweenType.easeInOutQuad;

    void Start()
    {
        // Initialize colors using hex values
        ColorUtility.TryParseHtmlString("#B2FBF5", out localPlayerColor);
        ColorUtility.TryParseHtmlString("#F0C97B", out nonLocalPlayerColor);
    }

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
        Image background = messageObject.GetComponentInChildren<Image>();

        // Set initial state for animation
        messageObject.transform.localScale = Vector3.zero;
        background.color = new Color(background.color.r, background.color.g, background.color.b, 0);
        messageText.color = new Color(messageText.color.r, messageText.color.g, messageText.color.b, 0);

        if (isLocalPlayer)
        {
            // Align to the right for local player
            rectTransform.anchorMin = new Vector2(1, 0.5f);
            rectTransform.anchorMax = new Vector2(1, 0.5f);
            rectTransform.pivot = new Vector2(2.2f, 0.5f); // Changed pivot for local player
            messageText.alignment = TextAlignmentOptions.Right;
            messageText.margin = new Vector4(0, 0, 10, 0); // Add margin to the right
            background.color = localPlayerColor;
        }
        else
        {
            // Align to the left for non-local player
            rectTransform.anchorMin = new Vector2(0, 0.5f);
            rectTransform.anchorMax = new Vector2(0, 0.5f);
            rectTransform.pivot = new Vector2(-1.27f, 0.5f); // Changed pivot for non-local player
            messageText.alignment = TextAlignmentOptions.Left;
            messageText.margin = new Vector4(10, 0, 0, 0); // Add margin to the left
            background.color = nonLocalPlayerColor;
        }

        // Ensure the Content size is updated
        LayoutRebuilder.ForceRebuildLayoutImmediate(Content.GetComponent<RectTransform>());

        // Animate the message with new scale
        Vector3 targetScale = new Vector3(0.8f, 0.7f, 1f); // New target scale
        LeanTween.scale(messageObject, targetScale, messageScaleDuration).setEase(messageScaleEase);
        LeanTween.alpha(background.rectTransform, 1f, messageFadeDuration);
        LeanTween.value(messageObject, 0f, 1f, messageFadeDuration)
            .setOnUpdate((float val) =>
            {
                messageText.color = new Color(messageText.color.r, messageText.color.g, messageText.color.b, val);
            });

        // Scroll to bottom after animation
        LeanTween.value(gameObject, ScrollRect.verticalNormalizedPosition, 0f, scrollDuration)
            .setEase(scrollEase)
            .setOnUpdate((float val) =>
            {
                ScrollRect.verticalNormalizedPosition = val;
            });
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