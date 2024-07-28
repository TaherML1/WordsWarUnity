using UnityEngine;
using Firebase.Functions;
using Firebase.Auth;
using Firebase.Extensions;
using System.Collections.Generic;

public class FriendSystemManager : MonoBehaviour
{
    public static FriendSystemManager Instance { get; private set; }
    private FirebaseFunctions functions;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        functions = FirebaseFunctions.DefaultInstance;
    }

    public void SendFriendRequest(string receiverId)
    {
        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        string senderId = auth.CurrentUser.UserId;

        Debug.Log("Attempting to send friend request...");

        // Create the data payload to send to the Cloud Function
        Dictionary<string, object> data = new Dictionary<string, object>
        {
            { "senderId", senderId },
            { "receiverId", receiverId }
        };

        // Call the Cloud Function
        functions.GetHttpsCallable("sendFriendRequest1")
            .CallAsync(data)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("Error sending friend request: " + task.Exception.Flatten().InnerException.Message);
                }
                else if (task.IsCompleted)
                {
                    Debug.Log("Friend request sent successfully.");
                }
            });
    }

    public void AcceptFriendRequest(string senderId, string documentId, GameObject requestInstance)
    {
        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        string receiverId = auth.CurrentUser.UserId;

        Debug.Log("Attempting to accept friend request...");

        // Create the data payload to send to the Cloud Function
        Dictionary<string, object> data = new Dictionary<string, object>
        {
            { "senderId", senderId },
            { "receiverId", receiverId }
        };

        // Call the Cloud Function
        functions.GetHttpsCallable("acceptFriendRequest")
            .CallAsync(data)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("Error accepting friend request: " + task.Exception.Flatten().InnerException.Message);
                }
                else if (task.IsCompleted)
                {
                    // Remove the friend request from the UI
                    Destroy(requestInstance);
                    Debug.Log("Friend request accepted and removed successfully.");
                }
            });
    }
}
