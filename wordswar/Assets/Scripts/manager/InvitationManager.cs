using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using Firebase.Auth;
using System;
using System.Collections;
using Firebase.Firestore;

public class InvitationManager : MonoBehaviour
{
    public static InvitationManager Instance { get; private set; }
    private DatabaseReference databaseRef;
    private FirebaseAuth auth;
    [SerializeField] GameObject invitationPrefab; // Reference to the invitation prefab
    [SerializeField] Transform invitationParent; // Parent to hold the invitation panels
    [SerializeField] GameObject invitationSentPrefab; // Reference to the invitation sent prefab
    [SerializeField] Transform invitationSentParent; // Parent to hold the invitation sent panels
    [SerializeField] GameObject BGPanel;
    [SerializeField] GameObject GameListener;

    private string senderUsername;
    private Dictionary<string, GameObject> invitationSentInstances = new Dictionary<string, GameObject>();

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
        FirebaseApp app = FirebaseApp.DefaultInstance;
        databaseRef = FirebaseDatabase.DefaultInstance.RootReference;
        auth = FirebaseAuth.DefaultInstance;
        ListenForInvitations(auth.CurrentUser.UserId);
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

    // Function to send an invitation
    public void SendInvitation(string toPlayerId)
    {
        GameListener.SetActive(true);
        string fromPlayerId = auth.CurrentUser.UserId;

        // Fetch the sender's username from UserManager
        var userProfile = UserManager.Instance.GetUserProfile();
        userProfile.TryGetValue("username", out object usernameObj);
        senderUsername = Convert.ToString(usernameObj);

        DatabaseReference invitationsRef = databaseRef.Child("invitations");
        string invitationId = invitationsRef.Push().Key;

        Dictionary<string, object> invitationData = new Dictionary<string, object>
        {
            ["from"] = fromPlayerId,
            ["to"] = toPlayerId,
            ["status"] = "pending",
            ["senderName"] = senderUsername
        };

        invitationsRef.Child(invitationId).SetValueAsync(invitationData).ContinueWith(task => {
            if (task.IsCompleted)
            {
                Debug.Log("Invitation sent successfully.");
                ShowInvitationSentPanel(toPlayerId, invitationId);
                ListenForInvitationStatusChanges(invitationId); // Listen for status changes
             
            }
            else
            {
                Debug.LogError("Failed to send invitation: " + task.Exception);
            }
        });
    }

    // Function to listen for incoming invitations
    public void ListenForInvitations(string playerId)
    {
        DatabaseReference invitationsRef = databaseRef.Child("invitations");
        invitationsRef.OrderByChild("to").EqualTo(playerId).ValueChanged += HandleInvitation;
        Debug.Log("Listening for invitations for player ID: " + playerId);
    }

    private void HandleInvitation(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError("Error receiving invitations: " + args.DatabaseError.Message);
            return;
        }

        foreach (DataSnapshot snapshot in args.Snapshot.Children)
        {
            Dictionary<string, object> invitation = snapshot.Value as Dictionary<string, object>;
            if (invitation["status"].ToString() == "pending")
            {
                string fromPlayerId = invitation["from"].ToString();
                string fromPlayerUsername = invitation.ContainsKey("senderName") ? invitation["senderName"].ToString() : "Unknown";

                Debug.Log("Received invitation from: " + fromPlayerUsername);

                invitationParent.gameObject.SetActive(true);
                // Display invitation UI
                ShowInvitationPanel(snapshot.Key, fromPlayerUsername);
            }
        }
    }

    private void ShowInvitationPanel(string invitationId, string fromPlayerUsername)
    {
        GameObject invitationInstance = Instantiate(invitationPrefab, invitationParent);

        // Center the invitation instance in the parent
        var rectTransform = invitationInstance.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;

        var usernameText = invitationInstance.transform.Find("friendName")?.GetComponent<TextMeshProUGUI>();
        if (usernameText != null)
        {
            usernameText.text = fromPlayerUsername;
        }

        var acceptButtonTransform = invitationInstance.transform.Find("AcceptButton");
        var acceptButton = acceptButtonTransform.GetComponent<Button>();
        acceptButton.onClick.AddListener(() =>
        {
            AcceptInvitation(invitationId);
            Destroy(invitationInstance);
        });

        var declineButtonTransform = invitationInstance.transform.Find("DeclineButton");
        var declineButton = declineButtonTransform.GetComponent<Button>();
        declineButton.onClick.AddListener(() =>
        {
            DeclineInvitation(invitationId);
            Destroy(invitationInstance);
        });
    }

    private void ShowInvitationSentPanel(string toPlayerId, string invitationId)
    {
        // Fetch the receiver's username from Firestore
        FirebaseFirestore.DefaultInstance.Collection("users").Document(toPlayerId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DocumentSnapshot document = task.Result;
                if (document.Exists)
                {
                    if (document.TryGetValue("username", out string receiverUsername))
                    {
                        DisplayInvitationSentPanel(receiverUsername, invitationId);
                    }
                    else
                    {
                        Debug.LogError("Receiver username not found in the document.");
                    }
                }
                else
                {
                    Debug.LogError("Receiver document does not exist.");
                }
            }
            else
            {
                Debug.LogError("Failed to fetch receiver document: " + task.Exception);
            }
        });
    }

    private void DisplayInvitationSentPanel(string receiverUsername, string invitationId)
    {
        invitationSentParent.gameObject.SetActive(true);
        GameObject invitationSentInstance = Instantiate(invitationSentPrefab, invitationSentParent);

        // Center the invitation sent instance in the parent
        var rectTransform = invitationSentInstance.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;

        var usernameText = invitationSentInstance.transform.Find("ReceiverName")?.GetComponent<TextMeshProUGUI>();
        if (usernameText != null)
        {
            usernameText.text = receiverUsername;
            BGPanel.SetActive(true);
        }

        var closeButtonTransform = invitationSentInstance.transform.Find("CloseButton");
        var closeButton = closeButtonTransform.GetComponent<Button>();
        closeButton.onClick.AddListener(() =>
        {
            DeclineInvitation(invitationId);
            BGPanel?.SetActive(false);
            Destroy(invitationSentInstance);
        });

        // Store the instance in the dictionary
        invitationSentInstances[invitationId] = invitationSentInstance;
    }

    private void ListenForInvitationStatusChanges(string invitationId)
    {
        DatabaseReference invitationRef = databaseRef.Child("invitations").Child(invitationId);
        invitationRef.ValueChanged += (sender, args) =>
        {
            if (args.DatabaseError != null)
            {
                Debug.LogError("Error receiving invitation status: " + args.DatabaseError.Message);
                return;
            }

            if (args.Snapshot.Exists)
            {
                Dictionary<string, object> invitation = args.Snapshot.Value as Dictionary<string, object>;
                string status = invitation["status"].ToString();

                if (status == "accepted")
                {
                    Debug.Log("Invitation accepted.");
                    // Handle accepted invitation
                }
                else if (status == "declined")
                {
                    Debug.Log("Invitation declined.");
                    // Handle declined invitation
                    ShowInvitationDeclinedNotification(invitationId);
                }
            }
            else
            {
                Debug.Log("Invitation removed.");

                // Handle removed invitation
                if (invitationSentInstances.TryGetValue(invitationId, out GameObject invitationSentInstance))
                {
                    Destroy(invitationSentInstance);
                    invitationSentInstances.Remove(invitationId);
                    BGPanel?.SetActive(false);
                }
            }
        };
    }

    private void ShowInvitationDeclinedNotification(string invitationId)
    {
        // Display a notification or update the UI to inform the sender that the invitation was declined
        Debug.Log("Invitation declined. Notification should be displayed here.");


        // Example: Show a UI panel or a popup
        GameObject notificationInstance = Instantiate(invitationSentPrefab, invitationSentParent);

        // Center the notification instance in the parent
        var rectTransform = notificationInstance.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;

        var usernameText = notificationInstance.transform.Find("ReceiverName")?.GetComponent<TextMeshProUGUI>();
        if (usernameText != null)
        {
            usernameText.text = "Invitation Declined";
            BGPanel.SetActive(true);
            Destroy(notificationInstance);
        }

        var closeButtonTransform = notificationInstance.transform.Find("CloseButton");
        var closeButton = closeButtonTransform.GetComponent<Button>();
        closeButton.onClick.AddListener(() =>
        {
            BGPanel?.SetActive(false);
            Destroy(notificationInstance);
        });
    }

    // Function to accept an invitation
    public void AcceptInvitation(string invitationId)
    {
        GameListener.SetActive(true);
        DatabaseReference invitationRef = databaseRef.Child("invitations").Child(invitationId);

        Dictionary<string, object> update = new Dictionary<string, object>
        {
            ["status"] = "accepted"
        };

        invitationRef.UpdateChildrenAsync(update).ContinueWith(task => {
            if (task.IsCompleted)
            {
                Debug.Log("Invitation accepted.");
                // Start the game
                StartGame(invitationId);

            }
            else
            {
                Debug.LogError("Failed to accept invitation: " + task.Exception);
            }
        });
    }

    // Function to decline an invitation
    public void DeclineInvitation(string invitationId)
    {
        GameListener.SetActive(false);
        DatabaseReference invitationRef = databaseRef.Child("invitations").Child(invitationId);

        Dictionary<string, object> update = new Dictionary<string, object>
        {
            ["status"] = "declined"
        };

        invitationRef.UpdateChildrenAsync(update).ContinueWith(task => {
            if (task.IsCompleted)
            {
                Debug.Log("Invitation declined.");
                invitationRef.RemoveValueAsync().ContinueWith(removeTask => {
                    if (removeTask.IsCompleted)
                    {
                        Debug.Log("Invitation removed.");
                    }
                    else
                    {
                        Debug.LogError("Failed to remove invitation: " + removeTask.Exception);
                    }
                });
            }
            else
            {
                Debug.LogError("Failed to decline invitation: " + task.Exception);
            }
        });
    }

    // Function to start a game after accepting an invitation
    private void StartGame(string invitationId)
    {
        // You can implement game start logic here if needed.
        // For now, the game start logic is handled in the Cloud Function.
        Debug.Log("Starting game with invitation ID: " + invitationId);
    }
}
