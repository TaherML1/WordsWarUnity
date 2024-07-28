using UnityEngine;
using Firebase.Firestore;
using TMPro;
using UnityEngine.UI;
using Firebase.Extensions;
using Firebase.Auth;
using System.Collections.Generic;

public class FetchUserFriendRequests : MonoBehaviour
{
    [SerializeField] private Transform friendRequestListParent;
    [SerializeField] private GameObject friendRequestPrefab;

    private FirebaseFirestore db;
    private FirebaseAuth auth;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;

        FetchFriendRequests();
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

    private void DisplayFriendRequest(string documentId, string senderId, string senderUsername)
    {
        GameObject requestInstance = Instantiate(friendRequestPrefab, friendRequestListParent);

        TextMeshProUGUI usernameText = requestInstance.GetComponentInChildren<TextMeshProUGUI>();
        usernameText.text = senderUsername;

        Button acceptButton = requestInstance.GetComponentInChildren<Button>();
        acceptButton.onClick.AddListener(() => FriendSystemManager.Instance.AcceptFriendRequest(documentId, senderId, requestInstance));
    }
}
