using UnityEngine;
using Firebase.Firestore;
using TMPro;
using UnityEngine.UI;
using Firebase.Extensions;
using Firebase.Auth;
using System.Collections.Generic;
using Firebase.Functions;
using System.Collections;
using System.Linq;
using System;

public class FetchUserFriendsAndRequests : MonoBehaviour
{
    [SerializeField] private Transform friendRequestListParent;
    [SerializeField] private Transform friendListParent;
    [SerializeField] Transform profileParent;
    [SerializeField] private GameObject friendRequestPrefab;
    [SerializeField] private GameObject friendPrefab;
    [SerializeField] private GameObject friendOptionsPrefab;
    [SerializeField] GameObject playerProfilePrefab;
    [SerializeField] InvitationManager invitationManager;


    private FirebaseFirestore db;
    private FirebaseAuth auth;
    private FirebaseFunctions functions;
    private ListenerRegistration friendRequestListener;
    private ListenerRegistration friendsListener;

    // Dictionary to track friend instances by their IDs
    private Dictionary<string, GameObject> friendInstances = new Dictionary<string, GameObject>();

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
                        Debug.Log("Friend removed with ID: " + document.Id);
                        RemoveFriend(document.Id);
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
        // Instantiate the friendPrefab and set it as a child of friendListParent
        GameObject friendInstance = Instantiate(friendPrefab, friendListParent);

        // Find the delete button and the text component inside it
        var expandButtonTransform = friendInstance.transform.Find("ExpandButton");

        var usernameText = expandButtonTransform.transform.Find("friendName")?.GetComponent<TextMeshProUGUI>();
        usernameText.text = friendUsername;

        // Find the delete button component and add an onClick listener
        var expandButton = expandButtonTransform.GetComponent<Button>();
        expandButton.onClick.AddListener(() => ShowFriendOptions(friendId, friendUsername, friendInstance));

        // Add the friend instance to the dictionary
        friendInstances[friendId] = friendInstance;
    }

    private void RemoveFriend(string friendId)
    {
        if (friendInstances.TryGetValue(friendId, out GameObject friendInstance))
        {
            Destroy(friendInstance);
            friendInstances.Remove(friendId);
        }
    }

    private void ShowFriendOptions(string friendId, string friendUsername, GameObject friendInstance)
    {
        // Destroy existing options if any
        Transform existingOptions = friendInstance.transform.Find("FriendOptions");
        if (existingOptions != null)
        {
            Destroy(existingOptions.gameObject);
        }

        // Create the transparent overlay
        GameObject overlay = new GameObject("Overlay");
        overlay.transform.SetParent(friendInstance.transform.root, false); // Parent to the root to cover the entire screen
        RectTransform overlayRect = overlay.AddComponent<RectTransform>();

        // Set the overlay's size and position
        overlayRect.anchorMin = new Vector2(0.5f, 0.5f);
        overlayRect.anchorMax = new Vector2(0.5f, 0.5f);
        overlayRect.pivot = new Vector2(0.5f, 0.5f);
        overlayRect.anchoredPosition = new Vector2(0, -120); // Coordinates for position 
        overlayRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1080f);
        overlayRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 2400);

        CanvasRenderer overlayCanvasRenderer = overlay.AddComponent<CanvasRenderer>();
        Image overlayImage = overlay.AddComponent<Image>();
        overlayImage.color = new Color(0, 0, 0, 0); // Fully transparent

        // Add a button component to detect clicks
        Button overlayButton = overlay.AddComponent<Button>();
        overlayButton.onClick.AddListener(() => Destroy(overlay)); // Destroy the overlay (and panel) on click

        // Create and set up the options prefab
        GameObject optionsInstance = Instantiate(friendOptionsPrefab, overlay.transform); // Parent to overlay
        optionsInstance.name = "FriendOptions";

        // Set the size of the optionsInstance
        RectTransform optionsRectTransform = optionsInstance.GetComponent<RectTransform>();
        optionsRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 350); // Adjust width as needed
        optionsRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 350); // Adjust height as needed

        // Position the options panel on the right of the friend instance
        RectTransform friendRectTransform = friendInstance.GetComponent<RectTransform>();
        if (optionsRectTransform != null && friendRectTransform != null)
        {
            // Get the size of the friend instance
            Vector2 friendSize = friendRectTransform.rect.size;

            // Calculate the position for the options panel to be on the right of the friend instance
            Vector2 optionsSize = optionsRectTransform.rect.size;
            Vector2 friendPosition = (Vector2)friendRectTransform.position;
            float offset = 250; // Adjust this value to move the panel further to the left or right
            Vector2 optionsPosition = new Vector2(friendPosition.x + friendSize.x / 2 + optionsSize.x / 2 - offset,
                                                   friendPosition.y);
            // Adjust the options panel position
            optionsRectTransform.position = optionsPosition;

            // Check if the options panel goes off the right side of the screen
            if (optionsRectTransform.position.x + optionsSize.x / 2 > Screen.width)
            {
                optionsRectTransform.position = new Vector3(Screen.width - optionsSize.x / 2,
                                                            optionsRectTransform.position.y,
                                                            optionsRectTransform.position.z);
            }

            // Check if the options panel goes off the left side of the screen
            if (optionsRectTransform.position.x - optionsSize.x / 2 < 0)
            {
                optionsRectTransform.position = new Vector3(optionsSize.x / 2,
                                                            optionsRectTransform.position.y,
                                                            optionsRectTransform.position.z);
            }

            // Check if the options panel goes off the top or bottom of the screen
            if (optionsRectTransform.position.y + optionsSize.y / 2 > Screen.height)
            {
                optionsRectTransform.position = new Vector3(optionsRectTransform.position.x,
                                                            Screen.height - optionsSize.y / 2,
                                                            optionsRectTransform.position.z);
            }

            if (optionsRectTransform.position.y - optionsSize.y / 2 < 0)
            {
                optionsRectTransform.position = new Vector3(optionsRectTransform.position.x,
                                                            optionsSize.y / 2,
                                                            optionsRectTransform.position.z);
            }
        }

        // Find and set up the delete button
        var deleteButtonTransform = optionsInstance.transform.Find("DeleteButton");
        var deleteButton = deleteButtonTransform.GetComponent<Button>();
        deleteButton.onClick.AddListener(() =>
        {
            FriendSystemManager.Instance.DeleteFriend(friendId, friendInstance);
            Destroy(overlay);
        });

        // Find and set up the send invite button
        var sendInviteButtonTransform = optionsInstance.transform.Find("SendInviteButton");
        var sendInviteButton = sendInviteButtonTransform.GetComponent<Button>();
        sendInviteButton.onClick.AddListener(() =>
        {
            invitationManager.SendInvitation(friendId);
            Destroy(overlay);
        });

        // Find and set up the show profile button
        var showProfileButtonTransform = optionsInstance.transform.Find("ShowProfileButton");
        var showProfileButton = showProfileButtonTransform.GetComponent<Button>();
        showProfileButton.onClick.AddListener(() =>
        {
            SearchPlayerById(friendId);
            Destroy(overlay);
        });
    }



    private void SearchPlayerById(string playerId)
    {
        Debug.Log("Searching for player ID: " + playerId);

        // Reference the specific user document using playerId
        DocumentReference playerDocRef = db.Collection("users").Document(playerId);

        // Fetch the document asynchronously
        playerDocRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DocumentSnapshot document = task.Result;

                if (document.Exists)
                {
                    // Document exists, process the data
                    Debug.Log("Document data: " + document.ToDictionary());

                    if (document.TryGetValue("username", out string username))
                    {
                        Debug.Log("Username found: " + username);
                        ShowFriendProfile(document); // Show the profile with the fetched document
                    }
                    else
                    {
                        Debug.Log("Username not found in the document.");
                    }
                }
                else
                {
                    Debug.Log("Player document does not exist.");
                }
            }
            else if (task.IsFaulted)
            {
                // Handle any errors that occurred during the fetch
                Debug.LogError("Failed to fetch document: " + task.Exception);
            }
        });
    }





    private void ShowFriendProfile(DocumentSnapshot playerDoc)
    {
        Debug.Log("ShowProfileButton clicked");

        // Log the player document data
        Debug.Log("Player Document Data: " + playerDoc.ToDictionary());

        // Activate the profileParent GameObject
        profileParent.gameObject.SetActive(true);

        // Clear previous profile data
        foreach (Transform child in profileParent)
        {
            Destroy(child.gameObject);
        }

        // Instantiate the profile prefab
        GameObject profileInstance = Instantiate(playerProfilePrefab, profileParent);
        Debug.Log("Profile instance instantiated");

        // Set up profile data
        TextMeshProUGUI usernameText = profileInstance.transform.Find("UsernameText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI levelText = profileInstance.transform.Find("LevelText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI winsText = profileInstance.transform.Find("WinsText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI lossesText = profileInstance.transform.Find("LossesText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI scoreText = profileInstance.transform.Find("ScoreText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI playerIdText = profileInstance.transform.Find("playerId").GetComponent<TextMeshProUGUI>();
        Image winImage = profileInstance.transform.Find("WinImage").GetComponent<Image>();
        Image loseImage = profileInstance.transform.Find("LoseImage").GetComponent<Image>();

        if (usernameText != null) Debug.Log("usernameText found");
        if (levelText != null) Debug.Log("levelText found");
        if (winsText != null) Debug.Log("winsText found");
        if (lossesText != null) Debug.Log("lossesText found");
        if (scoreText != null) Debug.Log("scoreText found");
        if (playerIdText != null) Debug.Log("playerIdText found");
        if (winImage != null) Debug.Log("winImage found");
        if (loseImage != null) Debug.Log("loseImage found");

        if (playerDoc.TryGetValue("username", out string username))
        {
            Debug.Log("Username found: " + username);
            usernameText.text = username.ToString();
        }
        else
        {
            Debug.Log("Username not found");
        }

        if (playerDoc.TryGetValue("playerId", out string playerId))
        {
            Debug.Log("playerId found: " + playerId);
            playerIdText.text = "#" + playerId;
        }
        else
        {
            Debug.Log("playerId not found");
        }

        if (playerDoc.TryGetValue("level", out long level))
        {
            Debug.Log("Level found: " + level);
            levelText.text = level.ToString();
        }
        else
        {
            Debug.Log("Level not found");
        }

        if (playerDoc.TryGetValue("matchesWon", out long matchesWon))
        {
            Debug.Log("Matches Won found: " + matchesWon);
            winsText.text = matchesWon.ToString();
        }
        else
        {
            Debug.Log("Matches Won not found");
        }

        if (playerDoc.TryGetValue("matchesLost", out long matchesLost))
        {
            Debug.Log("Matches Lost found: " + matchesLost);
            lossesText.text = matchesLost.ToString();
        }
        else
        {
            Debug.Log("Matches Lost not found");
        }

        if (playerDoc.TryGetValue("scores", out long scores))
        {
            Debug.Log("Scores found: " + scores);
            scoreText.text = scores.ToString();
        }
        else
        {
            Debug.Log("Scores not found");
        }

        // Calculate and set win and loss rates
        long totalMatches = matchesWon + matchesLost;
        if (totalMatches > 0)
        {
            float winRate = (float)matchesWon / totalMatches;
            winImage.fillAmount = winRate;
            loseImage.fillAmount = 1; // Set to full

            Debug.Log("winImage.fillAmount set to: " + winRate);
            Debug.Log("loseImage.fillAmount set to: 1");

            // Calculate win percentage
            int winPercentage = Mathf.RoundToInt(winRate * 100);
            string winPercentageText = winPercentage.ToString() + "%";

            // Display win percentage
            TextMeshProUGUI winPercentageTextComponent = profileInstance.transform.Find("WinPercentageText").GetComponent<TextMeshProUGUI>();
            winPercentageTextComponent.text = winPercentageText;

            Debug.Log("Win percentage set to: " + winPercentageText);
        }
        else
        {
            Debug.Log("No matches played.");
        }


        // Find and configure the back button
        Button backButton = profileInstance.transform.Find("BackButton").GetComponent<Button>();
        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(() =>
        {
            Destroy(profileInstance);         // Destroy the profileInstance
            profileParent.gameObject.SetActive(false);  // Deactivate the profileParent
        });
    }



}
