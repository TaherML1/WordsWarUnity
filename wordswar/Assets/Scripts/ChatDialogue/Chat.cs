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
       
    }

    public void SendMessage()
    {
        string messageText = playerInput.text;
        if (!string.IsNullOrEmpty(messageText))
        {
            //GetMessage(messageText, true); // true indicates the local player
            playerInput.text = ""; // Clear the input field after sending the message
          
        }
    }

    public void SendMessage2()
    {
        string messageText = playerInput.text;
        if (!string.IsNullOrEmpty(messageText))
        {
         //   GetMessage(messageText, false); // false indicates the enemy player
            playerInput.text = ""; // Clear the input field after sending the message
            
        }
    }

    public void GetMessage(string receivedMessage, bool isLocalPlayer, bool isCorrect)
    {
        GameObject messageObject = Instantiate(MessagePrefab, Content.transform);
        TextMeshProUGUI messageText = messageObject.GetComponentInChildren<TextMeshProUGUI>();
        messageText.text = receivedMessage;
        RectTransform rectTransform = messageObject.GetComponent<RectTransform>();
        Image background = messageObject.GetComponentInChildren<Image>();

        // Get references to the correct and incorrect images
        RawImage correctImage = messageObject.transform.Find("CorrectImage").GetComponent<RawImage>();
        RawImage incorrectImage = messageObject.transform.Find("IncorrectImage").GetComponent<RawImage>();

        // Adjust position of the correct and incorrect images
        RectTransform correctRectTransform = correctImage.GetComponent<RectTransform>();
        RectTransform incorrectRectTransform = incorrectImage.GetComponent<RectTransform>();

        // Set the position and activate the correct or incorrect image based on the isCorrect parameter
        if (isCorrect)
        {
            correctImage.gameObject.SetActive(true);
            incorrectImage.gameObject.SetActive(false);
        }
        else
        {
            correctImage.gameObject.SetActive(false);
            incorrectImage.gameObject.SetActive(true);
        }

        if (isLocalPlayer)
        {
            // Align to the right for local player
            rectTransform.anchorMin = new Vector2(1, 0.5f);
            rectTransform.anchorMax = new Vector2(1, 0.5f);
            rectTransform.pivot = new Vector2(2.9f, 0.5f);
            messageText.alignment = TextAlignmentOptions.Right;
            messageText.margin = new Vector4(0, 0, 10, 0); // Add margin to the right

            
        }
        else
        {
            // Align to the left for non-local player
            rectTransform.anchorMin = new Vector2(0, 0.5f);
            rectTransform.anchorMax = new Vector2(0, 0.5f);
            rectTransform.pivot = new Vector2(-2.25f, 0.5f);
            messageText.alignment = TextAlignmentOptions.Left;
            messageText.margin = new Vector4(10, 0, 0, 0); // Add margin to the left

            // Position the correct/incorrect image for the non-local player at the specific X position
            correctRectTransform.anchoredPosition = new Vector2(250, correctRectTransform.anchoredPosition.y);
            incorrectRectTransform.anchoredPosition = new Vector2(250, incorrectRectTransform.anchoredPosition.y);
        }

        // Ensure the Content size is updated
        LayoutRebuilder.ForceRebuildLayoutImmediate(Content.GetComponent<RectTransform>());

        // Animate the message with new scale
        Vector3 targetScale = new Vector3(0.77f, 0.7f, 1f); // New target scale
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



}