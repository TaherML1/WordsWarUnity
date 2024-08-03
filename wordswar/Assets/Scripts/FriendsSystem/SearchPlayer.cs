using UnityEngine;
using UnityEngine.UI;
using Firebase.Firestore;
using TMPro;
using Firebase.Extensions;
using System.Collections;
using Firebase.Auth;
using System;
using System.Linq;

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
                        DisplaySearchResult("", "Username not found in the document.", false, null);
                    }
                }
                else
                {
                    Debug.Log("Player not found in the database.");
                    DisplaySearchResult("", "Player not found.", false, null);
                }
            }
            else
            {
                Debug.LogError("Search query failed: " + task.Exception.Message);
                DisplaySearchResult("", "Error: " + task.Exception.Message, false, null);
            }
        });
    }

    private void CheckIfAlreadyFriends(string playerId, string username, DocumentSnapshot playerDoc)
    {
        var userFriendsCollection = db.Collection("users").Document(currentUserId).Collection("friends");

        userFriendsCollection.WhereEqualTo("friendId", playerId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                QuerySnapshot friendIdSnapshot = task.Result;
                if (friendIdSnapshot.Count > 0)
                {
                    DisplaySearchResult(playerId, username, false, playerDoc);
                    return;
                }
                else
                {
                    userFriendsCollection.WhereEqualTo("playerId", playerId).GetSnapshotAsync().ContinueWithOnMainThread(playerIdTask =>
                    {
                        if (playerIdTask.IsCompleted)
                        {
                            QuerySnapshot playerIdSnapshot = playerIdTask.Result;
                            bool isFriend = playerIdSnapshot.Count > 0;
                            DisplaySearchResult(playerId, username, !isFriend, playerDoc);
                        }
                        else
                        {
                            Debug.LogError("Failed to check friends list by playerId: " + playerIdTask.Exception);
                            DisplaySearchResult(playerId, username, true, playerDoc);
                        }
                    });
                }
            }
            else
            {
                Debug.LogError("Failed to check friends list by friendId: " + task.Exception);
                DisplaySearchResult(playerId, username, true, playerDoc);
            }
        });
    }

    private void DisplaySearchResult(string playerId, string username, bool showAddButton, DocumentSnapshot playerDoc)
    {
        foreach (Transform child in resultParent)
        {
            Destroy(child.gameObject);
        }

        GameObject resultInstance = Instantiate(playerSearchResultPrefab, resultParent);

        TextMeshProUGUI resultText = resultInstance.GetComponentInChildren<TextMeshProUGUI>();
        resultText.text = username;

        Button addButton = resultInstance.transform.Find("AddFriendButton").GetComponent<Button>();
        Button profileButton = resultInstance.transform.Find("ShowProfileButton").GetComponent<Button>();

        if (!string.IsNullOrEmpty(playerId) && showAddButton)
        {
            addButton.onClick.RemoveAllListeners();
            addButton.onClick.AddListener(() => FriendSystemManager.Instance.SendFriendRequest(playerId));
            addButton.gameObject.SetActive(true);
        }
        else
        {
            addButton.gameObject.SetActive(false);
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

        if (playerDoc.TryGetValue("username", out string username))
        {
            Debug.Log("Username found: " + username);
            usernameText.text = "Username: " + username;
        }
        else
        {
            Debug.Log("Username not found");
        }
        if(playerDoc.TryGetValue("playerId", out string playerId))
        {
            Debug.Log("playerId found" +  playerId);

        }else
        {
            Debug.Log("player id not found");
        }

        if (playerDoc.TryGetValue("level", out long level))
        {
            Debug.Log("Level found: " + level);
            levelText.text = "Level: " + level;
        }
        else
        {
            Debug.Log("Level not found");
        }

        if (playerDoc.TryGetValue("matchesWon", out long matchesWon))
        {
            Debug.Log("Matches Won found: " + matchesWon);
            winsText.text = "Wins: " + matchesWon;
        }
        else
        {
            Debug.Log("Matches Won not found");
        }

        if (playerDoc.TryGetValue("matchesLost", out long matchesLost))
        {
            Debug.Log("Matches Lost found: " + matchesLost);
            lossesText.text = "Losses: " + matchesLost;
        }
        else
        {
            Debug.Log("Matches Lost not found");
        }

        if (playerDoc.TryGetValue("scores", out long scores))
        {
            Debug.Log("Scores found: " + scores);
            scoreText.text = "Score: " + scores;
        }
        else
        {
            Debug.Log("Scores not found");
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
