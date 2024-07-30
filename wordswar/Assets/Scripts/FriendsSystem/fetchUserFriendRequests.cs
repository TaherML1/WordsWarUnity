using UnityEngine;
using Firebase.Firestore;
using TMPro;
using UnityEngine.UI;
using Firebase.Extensions;
using Firebase.Auth;
using System.Collections.Generic;
using Firebase.Functions;
using System.Collections;

public class FetchUserFriendsAndRequests : MonoBehaviour
{
    [SerializeField] private Transform friendRequestListParent;
    [SerializeField] private Transform friendListParent;
    [SerializeField] private GameObject friendRequestPrefab;
    [SerializeField] private GameObject friendPrefab;
    [SerializeField] InvitationManager invitationManager;

    private FirebaseFirestore db;
    private FirebaseAuth auth;
    private FirebaseFunctions functions;
    private ListenerRegistration friendRequestListener;
    private ListenerRegistration friendsListener;

    void Start()
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
        db = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;
        functions = FirebaseFunctions.DefaultInstance;

        ListenToFriendRequests();
        ListenToFriends();
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

    void OnDestroy()
    {
        friendRequestListener?.Stop();
        friendsListener?.Stop();
    }

    private void ListenToFriendRequests()
    {
        Debug.Log("Listening to friend requests");
        friendRequestListener = db.Collection("users").Document(auth.CurrentUser.UserId).Collection("friendRequests")
            .Listen(snapshot =>
            {
                Debug.Log("Friend request documents changed: " + snapshot.Count);

                foreach (DocumentChange change in snapshot.GetChanges())
                {
                    DocumentSnapshot document = change.Document;

                    if (change.ChangeType == DocumentChange.Type.Added)
                    {
                        Debug.Log("New friend request with ID: " + document.Id);

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
                    else if (change.ChangeType == DocumentChange.Type.Removed)
                    {
                        // Handle friend request removal if needed
                    }
                }
            });
    }

    private void ListenToFriends()
    {
        Debug.Log("Listening to friends");
        friendsListener = db.Collection("users").Document(auth.CurrentUser.UserId).Collection("friends")
            .Listen(snapshot =>
            {
                Debug.Log("Friend documents changed: " + snapshot.Count);

                foreach (DocumentChange change in snapshot.GetChanges())
                {
                    DocumentSnapshot document = change.Document;

                    if (change.ChangeType == DocumentChange.Type.Added)
                    {
                        Debug.Log("New friend with ID: " + document.Id);

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
                    else if (change.ChangeType == DocumentChange.Type.Removed)
                    {
                        // Handle friend removal if needed
                    }
                }
            });
    }

    private void DisplayFriendRequest(string documentId, string senderId, string senderUsername)
    {
        GameObject requestInstance = Instantiate(friendRequestPrefab, friendRequestListParent);

        var usernameText = requestInstance.transform.Find("UserName")?.GetComponent<TextMeshProUGUI>();
        usernameText.text = senderUsername;

        var acceptButtonTransform = requestInstance.transform.Find("AcceptButton");
        var acceptButton = acceptButtonTransform.GetComponent<Button>();
        acceptButton.onClick.AddListener(() => FriendSystemManager.Instance.AcceptFriendRequest(senderId, documentId, requestInstance));

        var declineButtonTransform = requestInstance.transform.Find("DeclineButton");
        var declineButton = declineButtonTransform.GetComponent<Button>();
        declineButton.onClick.AddListener(() => FriendSystemManager.Instance.DeclineFriendRequest(documentId, requestInstance));
    }

    private void DisplayFriend(string friendId, string friendUsername)
    {
        GameObject friendInstance = Instantiate(friendPrefab, friendListParent);

        var usernameText = friendInstance.transform.Find("friendName")?.GetComponent<TextMeshProUGUI>();
        usernameText.text = friendUsername;

        var deleteButtonTransform = friendInstance.transform.Find("DeleteButton");
        var deleteButton = deleteButtonTransform.GetComponent<Button>();
        deleteButton.onClick.AddListener(() => FriendSystemManager.Instance.DeleteFriend(friendId, friendInstance));

        var sendInviteButtonTransform = friendInstance.transform.Find("SendInviteButton");
        var sendInviteButton = sendInviteButtonTransform.GetComponent<Button>();
        sendInviteButton.onClick.AddListener(() => invitationManager.SendInvitation(friendId));
    }
}
