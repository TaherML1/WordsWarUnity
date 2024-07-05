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
            // Check if Firebase is initialized
            if (FirebaseManager.Instance != null && FirebaseManager.Instance.IsFirebaseInitialized)
            {
                InitializeFCM();
            }
            else
            {
                // Wait until Firebase is initialized
                FirebaseManager.Instance.StartCoroutine(WaitForFirebaseInitialization());
            }
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
        Debug.Log("FCM Initialized.");
    }

    private System.Collections.IEnumerator WaitForFirebaseInitialization()
    {
        // Wait until Firebase is initialized
        while (!FirebaseManager.Instance.IsFirebaseInitialized)
        {
            yield return null;
        }
        InitializeFCM();
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
