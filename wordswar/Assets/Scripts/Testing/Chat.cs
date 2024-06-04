using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Chat : MonoBehaviour
{
    public TMP_InputField playerInput;
    public GameObject Message;
    public GameObject Content;

    public void SendMessage()
    {
        string messageText = playerInput.text;
        if (!string.IsNullOrEmpty(messageText))
        {
            GetMessage(messageText);
            playerInput.text = ""; // Clear the input field after sending the message
        }
    }

    public void GetMessage(string ReciveMessage)
    {
        GameObject M = Instantiate(Message, Vector3.zero, Quaternion.identity, Content.transform);
        M.GetComponent<Message>().MyMessage.text = ReciveMessage;
    }
}
