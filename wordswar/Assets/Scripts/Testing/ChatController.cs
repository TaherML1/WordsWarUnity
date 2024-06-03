using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ChatController : MonoBehaviour
{
    public TMP_InputField inputField;
    public Button sendButton;
    public Transform messageContainer;
    public GameObject messagePrefabLeft;
    public GameObject messagePrefabRight;

    private bool sendRight = true;

    void Start()
    {
        sendButton.onClick.AddListener(SendMessage);
    }

    void SendMessage()
    {
        if (string.IsNullOrWhiteSpace(inputField.text))
        {
            return;
        }

        GameObject message = Instantiate(sendRight ? messagePrefabRight : messagePrefabLeft, messageContainer);
        TMP_Text messageText = message.GetComponentInChildren<TMP_Text>();
        messageText.text = inputField.text;

        inputField.text = string.Empty;
        sendRight = !sendRight; // Toggle the side for the next message
    }
}
