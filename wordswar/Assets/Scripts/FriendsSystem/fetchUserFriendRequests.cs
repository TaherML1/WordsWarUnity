using UnityEngine;
using Firebase.Firestore;
using TMPro;
using UnityEngine.UI;
using Firebase.Extensions;
using Firebase.Auth;
using System.Collections.Generic;
using Firebase.Functions;

public class FetchUserFriendsAndRequests : MonoBehaviour
{
    [SerializeField] private Transform friendRequestListParent;
    [SerializeField] private Transform friendListParent;
    [SerializeField] private GameObject friendRequestPrefab;
    [SerializeField] private GameObject friendPrefab;

    private FirebaseFirestore db;
    private FirebaseAuth auth;
    private FirebaseFunctions functions;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;
        functions = FirebaseFunctions.DefaultInstance;

        FetchFriendRequests();
        FetchFriends();
    }

    private void FetchFriendRequests()
    {
        Debug.Log("Fetching friend requests");
        db.Collection("users").Document(auth.CurrentUser.UserId).Collection("friendRequests").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error fetching friend requests: " + task.Exception);
                return;
            }

            QuerySnapshot snapshot = task.Result;
            Debug.Log("Friend request documents fetched: " + snapshot.Count);

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                Debug.Log("Processing document with ID: " + document.Id);

                Dictionary<string, object> data = document.ToDictionary();

                if (data.ContainsKey("senderId"))
                {
                    string senderId = data["senderId"].ToString();
                    string senderUsername = data.ContainsKey("username") ? data["username"].ToString() : "Unknown";

                    DisplayFriendRequest(document.Id, senderId, senderUsername);
                }
                else
                {
                    Debug.LogWarning("Document is missing 'senderId' field");
                }
            }
        });
    }

    private void FetchFriends()
    {
        Debug.Log("Fetching friends");
        db.Collection("users").Document(auth.CurrentUser.UserId).Collection("friends").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error fetching friends: " + task.Exception);
                return;
            }

            QuerySnapshot snapshot = task.Result;
            Debug.Log("Friend documents fetched: " + snapshot.Count);

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                Debug.Log("Processing document with ID: " + document.Id);

                Dictionary<string, object> data = document.ToDictionary();

                if (data.ContainsKey("friendId"))
                {
                    string friendId = data["friendId"].ToString();
                    string friendUsername = data.ContainsKey("username") ? data["username"].ToString() : "Unknown";

                    DisplayFriend(friendId, friendUsername);
                }
                else
                {
                    Debug.LogWarning("Document is missing 'friendId' field");
                }
            }
        });
    }

    private void DisplayFriendRequest(string documentId, string senderId, string senderUsername)
    {
        GameObject requestInstance = Instantiate(friendRequestPrefab, friendRequestListParent);

        TextMeshProUGUI usernameText = requestInstance.GetComponentInChildren<TextMeshProUGUI>();
        usernameText.text = senderUsername;

        Button acceptButton = requestInstance.GetComponentInChildren<Button>();
        acceptButton.onClick.AddListener(() => FriendSystemManager.Instance.AcceptFriendRequest(senderId, documentId, requestInstance));
    }

    private void DisplayFriend(string friendId, string friendUsername)
    {
        GameObject friendInstance = Instantiate(friendPrefab, friendListParent);

        TextMeshProUGUI usernameText = friendInstance.GetComponentInChildren<TextMeshProUGUI>();
        usernameText.text = friendUsername;

        Button deleteButton = friendInstance.GetComponentInChildren<Button>();
        deleteButton.onClick.AddListener(() => DeleteFriend(friendId, friendInstance));
    }

    private void DeleteFriend(string friendId, GameObject friendInstance)
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
