using UnityEngine;
using Firebase.Messaging;

public class FCMHandler : MonoBehaviour
{
    private static FCMHandler instance;

    void Awake()
    {
        // Singleton pattern to ensure only one instance
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeFCM();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeFCM()
    {
        FirebaseMessaging.TokenReceived += OnTokenReceived;
        FirebaseMessaging.MessageReceived += OnMessageReceived;
    }

    public void OnTokenReceived(object sender, TokenReceivedEventArgs token)
    {
        Debug.Log("Received Registration Token: " + token.Token);
        // Handle token
    }

    public void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        Debug.Log("Received a new message");
        if (e.Message.Notification != null)
        {
            Debug.Log("Title: " + e.Message.Notification.Title);
            Debug.Log("Body: " + e.Message.Notification.Body);
        }
    }
}
