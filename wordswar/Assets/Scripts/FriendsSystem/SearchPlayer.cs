using UnityEngine;
using UnityEngine.UI;
using Firebase.Firestore;
using TMPro;
using Firebase.Extensions;
using System.Collections;
using Firebase.Auth;
using System;
using System.Linq;
using UnityEngine.Networking;

public class SearchPlayer : MonoBehaviour
{
    [SerializeField] TMP_InputField searchInput;
    [SerializeField] GameObject playerSearchResultPrefab;
    [SerializeField] GameObject playerProfilePrefab;
    [SerializeField] Transform resultParent;
    [SerializeField] Transform profileParent;

    private FirebaseFirestore db;
    private string currentUserId;
    private string currentPlayerId;

    void Start()
    {
        if (FirebaseManager.Instance != null && FirebaseManager.Instance.IsFirebaseInitialized)
        {
            InitializeFirebaseComponents();
        }
        else
        {
            StartCoroutine(WaitForFirebaseInitialization());
        }
    }

    private void InitializeFirebaseComponents()
    {
        db = FirebaseFirestore.DefaultInstance;
        currentUserId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
    }

    private IEnumerator WaitForFirebaseInitialization()
    {
        while (!FirebaseManager.Instance.IsFirebaseInitialized)
        {
            yield return null;
        }
        InitializeFirebaseComponents();
    }

    public void OnSearchButtonClicked()
    {
        string playerId = searchInput.text;
        Debug.Log("Search button clicked. Player ID: " + playerId);
        var userProfile = UserManager.Instance.GetUserProfile();
        userProfile.TryGetValue("playerId", out object playerIdObj);
        currentPlayerId = Convert.ToString(playerIdObj);

        if (playerId != currentPlayerId)
        {
            SearchPlayerById(playerId);
        }
        else
        {
            Debug.Log("You can't search for yourself.");
        }
    }

    private void SearchPlayerById(string playerId)
    {
        Debug.Log("Searching for player ID: " + playerId);
        db.Collection("users").WhereEqualTo("playerId", playerId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                QuerySnapshot snapshot = task.Result;
                if (snapshot.Count > 0)
                {
                    var document = snapshot.Documents.First(); // Use LINQ to get the first document
                    if (document.TryGetValue("username", out string username))
                    {
                        CheckIfAlreadyFriends(playerId, username, document);
                    }
                    else
                    {
                        Debug.Log("Username not found in the document.");
                        DisplaySearchResult("", "Username not found in the document.", false, false, null);
                    }
                }
                else
                {
                    Debug.Log("Player not found in the database.");
                    DisplaySearchResult("", "Player not found.", false,false, null);
                }
            }
            else
            {
                Debug.LogError("Search query failed: " + task.Exception.Message);
                DisplaySearchResult("", "Error: " + task.Exception.Message, false, false,null);
            }
        });
    }

    private void CheckIfAlreadyFriends(string playerId, string username, DocumentSnapshot playerDoc)
    {
        Debug.Log($"Checking if already friends or if request has been sent. PlayerId: {playerId}, Username: {username}");

        var userFriendsCollection = db.Collection("users").Document(currentUserId).Collection("friends");
        var friendRequestsCollection = db.Collection("users").Document(currentUserId).Collection("sentRequests");

        // Check if the player is already a friend
        userFriendsCollection.WhereEqualTo("playerId", playerId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                QuerySnapshot friendIdSnapshot = task.Result;
                Debug.Log($"Friends snapshot count: {friendIdSnapshot.Count}");
                if (friendIdSnapshot.Count > 0)
                {
                    Debug.Log("Player is already a friend.");
                    DisplaySearchResult(playerId, username, false, false, playerDoc);
                }
                else
                {
                    // No friend found, check if a request has been sent
                    Debug.Log("No friend found, checking if a request has been sent.");
                    friendRequestsCollection.WhereEqualTo("receiverId", playerId).GetSnapshotAsync().ContinueWithOnMainThread(requestTask =>
                    {
                        if (requestTask.IsCompleted)
                        {
                            QuerySnapshot requestSnapshot = requestTask.Result;
                            bool requestSent = requestSnapshot.Count > 0;
                            Debug.Log($"Friend requests snapshot count: {requestSnapshot.Count}");
                            if (requestSent)
                            {
                                Debug.Log("Friend request has already been sent.");
                            }
                            else
                            {
                                Debug.Log("No friend request has been sent.");
                            }
                            DisplaySearchResult(playerId, username, !requestSent, requestSent, playerDoc);
                        }
                        else
                        {
                            Debug.LogError("Failed to check friend requests: " + requestTask.Exception);
                            DisplaySearchResult(playerId, username, true, false, playerDoc);
                        }
                    });
                }
            }
            else
            {
                Debug.LogError("Failed to check friends list: " + task.Exception);
                DisplaySearchResult(playerId, username, true, false, playerDoc);
            }
        });
    }


    private void DisplaySearchResult(string playerId, string username, bool showAddButton, bool requestSent, DocumentSnapshot playerDoc)
    {
        foreach (Transform child in resultParent)
        {
            Destroy(child.gameObject);
        }

        GameObject resultInstance = Instantiate(playerSearchResultPrefab, resultParent);

        TextMeshProUGUI resultText = resultInstance.GetComponentInChildren<TextMeshProUGUI>();
        resultText.text = username;

        Button addButton = resultInstance.transform.Find("AddFriendButton").GetComponent<Button>();
        TextMeshProUGUI requestText = resultInstance.transform.Find("RequestSentText").GetComponent<TextMeshProUGUI>();
        Button profileButton = resultInstance.transform.Find("ShowProfileButton").GetComponent<Button>();

        if (showAddButton)
        {
            addButton.onClick.RemoveAllListeners();
            addButton.onClick.AddListener(() =>
            {
                FriendSystemManager.Instance.SendFriendRequest(playerId, requestSentSuccess =>
                {
                    if (requestSentSuccess)
                    {
                        // Update the UI to show "Request Sent"
                        addButton.gameObject.SetActive(false);
                        requestText.gameObject.SetActive(true);
                        requestText.text = "Request Sent";
                    }
                });
            });
            addButton.gameObject.SetActive(true);
            requestText.gameObject.SetActive(false);
        }
        else if (requestSent)
        {
            addButton.gameObject.SetActive(false);
            requestText.gameObject.SetActive(true);
            requestText.text = "Request Sent";
        }
        else
        {
            addButton.gameObject.SetActive(false);
            requestText.gameObject.SetActive(false);
        }

        if (!string.IsNullOrEmpty(playerId))
        {
            profileButton.onClick.RemoveAllListeners();
            profileButton.onClick.AddListener(() => ShowPlayerProfile(playerDoc));
            profileButton.gameObject.SetActive(true);
        }
        else
        {
            profileButton.gameObject.SetActive(false);
        }
    }



    private void ShowPlayerProfile(DocumentSnapshot playerDoc)
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
            TextMeshProUGUI playerIdText = profileInstance.transform.Find("PlayerIdText")?.GetComponent<TextMeshProUGUI>();
            Image profileImage = profileInstance.transform.Find("ProfileImage")?.GetComponent<Image>();

            if (usernameText != null)
            {
                if (playerDoc.TryGetValue("username", out string username))
                {
                    Debug.Log("Username found: " + username);
                    usernameText.text = username;
                }
                
            }
            

            if (playerIdText != null)
            {
                if (playerDoc.TryGetValue("playerId", out string playerId))
                {
                    playerIdText.text = "#" +  playerId;
                    Debug.Log("Player ID found: " + playerId);
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
           

            if (winsText != null)
            {
                if (playerDoc.TryGetValue("matchesWon", out long matchesWon))
                {
                    Debug.Log("Matches Won found: " + matchesWon);
                    winsText.text = matchesWon.ToString();
                }
               
            }
           

            if (lossesText != null)
            {
                if (playerDoc.TryGetValue("matchesLost", out long matchesLost))
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



}
