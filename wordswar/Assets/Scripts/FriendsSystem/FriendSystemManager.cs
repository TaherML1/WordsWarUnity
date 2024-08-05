using UnityEngine;
using Firebase.Functions;
using Firebase.Auth;
using Firebase.Extensions;
using System.Collections.Generic;
using System.Collections;
using System;

public class FriendSystemManager : MonoBehaviour
{
    public static FriendSystemManager Instance { get; private set; }
    private FirebaseFunctions functions;
    private FirebaseAuth auth;

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
        // Check if Firebase is initialized
        if (FirebaseManager.Instance != null && FirebaseManager.Instance.IsFirebaseInitialized)
        {
            InitializeFirebaseComponents();
        }
        else
        {
            // Wait until Firebase is initialized
            StartCoroutine(WaitForFirebaseInitialization());
        }
    }

    private void InitializeFirebaseComponents()
    {
        functions = FirebaseFunctions.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;
    }

    private IEnumerator WaitForFirebaseInitialization()
    {
        // Wait until Firebase is initialized
        while (!FirebaseManager.Instance.IsFirebaseInitialized)
        {
            yield return null;
        }

        // Firebase is now initialized, initialize Firebase components
        InitializeFirebaseComponents();
    }

    public void SendFriendRequest(string receiverId, Action<bool> onRequestSent)
    {
        onRequestSent?.Invoke(true);
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
                    onRequestSent?.Invoke(false);
                }
                else if (task.IsCompleted)
                {
                    Debug.Log("Friend request sent successfully.");
                   
                }
            });
    }


    public void AcceptFriendRequest(string senderId, string documentId, GameObject requestInstance)
    {
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

    public void DeclineFriendRequest(string requestId, GameObject requestInstance)
    {
        Debug.Log("Declining friend request: " + requestId);

        var declineRequestFunction = functions.GetHttpsCallable("declineFriendRequest");
        var data = new Dictionary<string, object>
        {
            { "requestId", requestId }
        };

        declineRequestFunction.CallAsync(data).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error declining friend request: " + task.Exception);
                return;
            }

            Debug.Log("Friend request declined successfully");
            Destroy(requestInstance);
        });
    }

    public void DeleteFriend(string friendId, GameObject friendInstance)
    {
        Debug.Log("Deleting friend: " + friendId);

        var deleteFriendFunction = functions.GetHttpsCallable("deleteFriend");
        var data = new Dictionary<string, object>
        {
            { "friendId", friendId }
        };

        deleteFriendFunction.CallAsync(data).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error deleting friend: " + task.Exception);
                return;
            }

            Debug.Log("Friend deleted successfully");
            Destroy(friendInstance);
        });
    }
}
