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

    public void SendMessage()
    {
        string messageText = playerInput.text;
        if (!string.IsNullOrEmpty(messageText))
        {
            GetMessage(messageText, true); // true indicates the local player
            playerInput.text = ""; // Clear the input field after sending the message
        }
    }

    public void SendMessage2()
    {
        string messageText = playerInput.text;
        if (!string.IsNullOrEmpty(messageText))
        {
            GetMessage(messageText, false); // false indicates the enemy player
            playerInput.text = ""; // Clear the input field after sending the message
        }
    }

    public void GetMessage(string receivedMessage, bool isLocalPlayer)
    {
        GameObject messageObject = Instantiate(MessagePrefab, Vector3.zero, Quaternion.identity, Content.transform);
        TextMeshProUGUI messageText = messageObject.GetComponent<Message>().MyMessage;
        messageText.text = receivedMessage;

        // Align the message to the left or right based on the player
        RectTransform rectTransform = messageObject.GetComponent<RectTransform>();
        if (isLocalPlayer)
        {
            rectTransform.anchorMin = new Vector2(0, 0.5f);
            rectTransform.anchorMax = new Vector2(0, 0.5f);
            rectTransform.pivot = new Vector2(0, 0.5f);
            messageText.alignment = TextAlignmentOptions.Left;
            messageText.margin = new Vector4(10, 0, 0, 0); // Add margin for left alignment
        }
        else
        {
            rectTransform.anchorMin = new Vector2(1, 0.5f);
            rectTransform.anchorMax = new Vector2(1, 0.5f);
            rectTransform.pivot = new Vector2(1, 0.5f);
            messageText.alignment = TextAlignmentOptions.Right;
            messageText.margin = new Vector4(0, 0, 10, 0); // Add margin for right alignment
        }
    }
}
