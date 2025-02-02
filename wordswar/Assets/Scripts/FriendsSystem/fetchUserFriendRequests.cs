﻿using UnityEngine;
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
using UnityEngine.Networking;

public class FetchUserFriendsAndRequests : MonoBehaviour
{
    [SerializeField] InvitationManager invitationManager;

    [SerializeField] private Transform friendRequestListParent;
    [SerializeField] private Transform friendListParent;
    [SerializeField] Transform profileParent;
    
    [SerializeField] private GameObject friendRequestPrefab;
    [SerializeField] private GameObject friendPrefab;
    [SerializeField] private GameObject friendOptionsPrefab;
    [SerializeField] GameObject playerProfilePrefab;
    [SerializeField] private GameObject confirmationDialogPrefab;
   
   
    
    [SerializeField] private TextMeshProUGUI friendCountText; // Add this line


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

                UpdateFriendCount(); // Update the friend count after processing changes
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

        UpdateFriendCount(); // Update the friend count after adding a new friend
    }

    private void RemoveFriend(string friendId)
    {
        if (friendInstances.TryGetValue(friendId, out GameObject friendInstance))
        {
            Destroy(friendInstance);
            friendInstances.Remove(friendId);

            UpdateFriendCount(); // Update the friend count after removing a friend
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
            ShowConfirmationDialog(friendId, friendInstance, overlay);
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
        if (profileParent != null)
        {
            profileParent.gameObject.SetActive(true);
            Debug.Log("Profile parent activated.");
        }
        else
        {
            Debug.LogError("profileParent is not assigned.");
        }

        // Clear previous profile data
        foreach (Transform child in profileParent)
        {
            Destroy(child.gameObject);
        }
        Debug.Log("Cleared previous profile data.");

        // Instantiate the profile prefab
        if (playerProfilePrefab != null)
        {
            GameObject profileInstance = Instantiate(playerProfilePrefab, profileParent);
            Debug.Log("Profile instance instantiated.");

            // Set up profile data
            TextMeshProUGUI usernameText = profileInstance.transform.Find("UsernameText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI levelText = profileInstance.transform.Find("LevelText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI winsText = profileInstance.transform.Find("WinsText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI lossesText = profileInstance.transform.Find("LossesText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI scoreText = profileInstance.transform.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI winPercentageText = profileInstance.transform.Find("WinPercentageText")?.GetComponent<TextMeshProUGUI>();
            Transform copyButtonTransform = profileInstance.transform.Find("CopyButton");
            TextMeshProUGUI textCopied = profileInstance.transform.Find("copiedText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI playerIdText = copyButtonTransform?.Find("PlayerIdText")?.GetComponent<TextMeshProUGUI>();
            Button copyButton = copyButtonTransform?.GetComponent<Button>();
            Image profileImage = profileInstance.transform.Find("ProfileImage")?.GetComponent<Image>();
            Image winImage = profileInstance.transform.Find("WinImage")?.GetComponent<Image>();
            Image loseImage = profileInstance.transform.Find("LoseImage")?.GetComponent<Image>();

            IEnumerator ShowCopiedText()
            {
              
                textCopied.gameObject.SetActive(true);
                yield return new WaitForSeconds(4f);
                textCopied.gameObject.SetActive(false);
            }

            if (usernameText != null)
            {
                if (playerDoc.TryGetValue("username", out string username))
                {
                    Debug.Log("Username found: " + username);
                    usernameText.text = username;
                }
            }

            if (playerIdText != null && playerDoc.TryGetValue("playerId", out string playerId))
            {
                playerIdText.text = "#" + playerId;
                Debug.Log("Player ID found: " + playerId);

                // Add onClick listener to the CopyButton
                if (copyButton != null)
                {
                    copyButton.onClick.AddListener(() =>
                    {
                        GUIUtility.systemCopyBuffer = playerId;
                        StartCoroutine(ShowCopiedText());
                        Debug.Log("Copied Player ID to clipboard: " + playerId);

                    });
                }
            }

            if (levelText != null)
            {
                if (playerDoc.TryGetValue("level", out long level))
                {
                    Debug.Log("Level found: " + level);
                    levelText.text = level.ToString();
                }
            }

            long matchesWon = 0;
            if (winsText != null)
            {
                if (playerDoc.TryGetValue("matchesWon", out matchesWon))
                {
                    Debug.Log("Matches Won found: " + matchesWon);
                    winsText.text = matchesWon.ToString();
                }
            }

            long matchesLost = 0;
            if (lossesText != null)
            {
                if (playerDoc.TryGetValue("matchesLost", out matchesLost))
                {
                    Debug.Log("Matches Lost found: " + matchesLost);
                    lossesText.text = matchesLost.ToString();
                }
            }

            if (scoreText != null)
            {
                if (playerDoc.TryGetValue("scores", out long scores))
                {
                    Debug.Log("Scores found: " + scores);
                    scoreText.text = scores.ToString();
                }
            }

            if (profileImage != null)
            {
                if (playerDoc.TryGetValue("profileImageURL", out string profileImageURL))
                {
                    Debug.Log("Profile Image URL found: " + profileImageURL);
                    StartCoroutine(LoadProfileImage(profileImageURL, profileImage));
                }
                else
                {
                    Debug.Log("Profile Image URL not found");
                }
            }
            else
            {
                Debug.LogError("ProfileImage component not found in profileInstance.");
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

                // Calculate and set win percentage
                int winPercentage = Mathf.RoundToInt(winRate * 100f);

                if (winPercentageText != null)
                {
                    winPercentageText.text = $"{winPercentage}%";
                    Debug.Log("Win percentage set to: " + winPercentageText.text);
                }
            }

            // Find and configure the back button
            Button backButton = profileInstance.transform.Find("BackButton")?.GetComponent<Button>();
            if (backButton != null)
            {
                backButton.onClick.RemoveAllListeners();
                backButton.onClick.AddListener(() =>
                {
                    Debug.Log("Back button clicked.");
                    Destroy(profileInstance);         // Destroy the profileInstance
                    profileParent.gameObject.SetActive(false);  // Deactivate the profileParent
                    Debug.Log("Profile instance destroyed and profileParent deactivated.");
                });
            }
            else
            {
                Debug.LogError("BackButton component not found in profileInstance.");
            }
        }
        else
        {
            Debug.LogError("playerProfilePrefab is not assigned.");
        }
    }
    private IEnumerator LoadProfileImage(string imageUrl, Image profileImage)
    {
        Debug.Log($"Loading profile image from URL: {imageUrl}");

        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                Sprite profileSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                profileImage.sprite = profileSprite;
                Debug.Log("Profile image loaded successfully.");
            }
            else
            {
                Debug.LogError("Failed to load profile image: " + uwr.error);
            }
        }
    }

    private void UpdateFriendCount()
    {
        int friendCount = friendInstances.Count;
        if (friendCountText != null)
        {
            friendCountText.text = friendCount + " / 20";
        }
        else
        {
            Debug.LogError("friendCountText is not assigned.");
        }
    }

    private void ShowConfirmationDialog(string friendId, GameObject friendInstance, GameObject overlay)
    {
        // Instantiate the confirmation dialog prefab
        GameObject confirmationDialog = Instantiate(confirmationDialogPrefab, overlay.transform);
        RectTransform dialogRect = confirmationDialog.GetComponent<RectTransform>();

        // Position the dialog in the center of the overlay
        dialogRect.anchorMin = new Vector2(0.5f, 0.5f);
        dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
        dialogRect.pivot = new Vector2(0.5f, 0.5f);
        dialogRect.anchoredPosition = Vector2.zero;

        // Find and set up the Confirm and Cancel buttons
        var confirmButtonTransform = confirmationDialog.transform.Find("ConfirmButton");
        var confirmButton = confirmButtonTransform.GetComponent<Button>();
        confirmButton.onClick.AddListener(() =>
        {
            FriendSystemManager.Instance.DeleteFriend(friendId, friendInstance);
            Destroy(overlay);
        });

        var cancelButtonTransform = confirmationDialog.transform.Find("CancelButton");
        var cancelButton = cancelButtonTransform.GetComponent<Button>();
        cancelButton.onClick.AddListener(() =>
        {
            Destroy(confirmationDialog);
        });
    }

    
}
