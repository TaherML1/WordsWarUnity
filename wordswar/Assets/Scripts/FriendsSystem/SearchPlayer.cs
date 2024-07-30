using UnityEngine;
using UnityEngine.UI;
using Firebase.Firestore;
using TMPro;
using Firebase.Extensions;
using System.Collections;
using Firebase.Auth;
using System;

public class SearchPlayer : MonoBehaviour
{
   
    [SerializeField] TMP_InputField searchInput;
    [SerializeField] GameObject playerSearchResultPrefab;
    [SerializeField] Transform resultParent;

    private FirebaseFirestore db;
    

    string currentUserId;
    string currentPlayerId;

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
        currentUserId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
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
            Debug.Log("you cant search yourself");
        }
       
    }

    private void SearchPlayerById(string playerId)
    {
        Debug.Log("Searching for player ID: " + playerId);
        Debug.Log("current playerId : " + currentPlayerId);
        
            db.Collection("users").WhereEqualTo("playerId", playerId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("Search query completed.");
                    QuerySnapshot snapshot = task.Result;
                    Debug.Log("Snapshot count: " + snapshot.Count);
                    foreach (DocumentSnapshot document in snapshot.Documents)
                    {
                        Debug.Log("Document ID: " + document.Id);
                        Debug.Log("Document Data: " + document.ToDictionary());
                    }

                    if (snapshot.Count > 0)
                    {
                        Debug.Log("Player found in the database.");
                        foreach (DocumentSnapshot document in snapshot.Documents)
                        {
                            if (document.TryGetValue("username", out string username))
                            {
                                Debug.Log("Player username: " + username);
                                CheckIfAlreadyFriends(playerId, username);
                            }
                            else
                            {
                                Debug.Log("Username not found in the document.");
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("Player not found in the database.");
                        DisplaySearchResult("", "Player not found.", false);
                    }
                }
                else
                {
                    Debug.LogError("Search query failed: " + task.Exception.Message);
                    DisplaySearchResult("", "Error: " + task.Exception.Message, false);
                }
            });
        
        
    }

    private void CheckIfAlreadyFriends(string playerId, string username)
    {
       

        Debug.Log("Checking if " + playerId + " is a friend of " + currentUserId);

        var userFriendsCollection = db.Collection("users").Document(currentUserId).Collection("friends");

        // Check by friendId
        userFriendsCollection.WhereEqualTo("friendId", playerId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                QuerySnapshot friendIdSnapshot = task.Result;
                Debug.Log("Query completed for friendId. Number of friend records found: " + friendIdSnapshot.Count);

                if (friendIdSnapshot.Count > 0)
                {
                    DisplaySearchResult(playerId, username, false); // Friend found by friendId
                    return;
                }
                else
                {
                    // If no match found by friendId, check by playerId
                    userFriendsCollection.WhereEqualTo("playerId", playerId).GetSnapshotAsync().ContinueWithOnMainThread(playerIdTask =>
                    {
                        if (playerIdTask.IsCompleted)
                        {
                            QuerySnapshot playerIdSnapshot = playerIdTask.Result;
                            Debug.Log("Query completed for playerId. Number of friend records found: " + playerIdSnapshot.Count);

                            bool isFriend = playerIdSnapshot.Count > 0;
                            DisplaySearchResult(playerId, username, !isFriend); // Pass !isFriend
                        }
                        else
                        {
                            Debug.LogError("Failed to check friends list by playerId: " + playerIdTask.Exception);
                            DisplaySearchResult(playerId, username, true); // Assume not a friend if there is an error
                        }
                    });
                }
            }
            else
            {
                Debug.LogError("Failed to check friends list by friendId: " + task.Exception);
                DisplaySearchResult(playerId, username, true); // Assume not a friend if there is an error
            }
        });
    }


    private void DisplaySearchResult(string playerId, string message, bool showAddButton)
    {
        // Clear any existing results
        foreach (Transform child in resultParent)
        {
            Destroy(child.gameObject);
        }

        // Instantiate the result prefab
        GameObject resultInstance = Instantiate(playerSearchResultPrefab, resultParent);

        // Set the text and button
        TextMeshProUGUI resultText = resultInstance.GetComponentInChildren<TextMeshProUGUI>();
        resultText.text = message;

        Button sendButton = resultInstance.GetComponentInChildren<Button>();
        if (!string.IsNullOrEmpty(playerId) && showAddButton)
        {
            sendButton.onClick.RemoveAllListeners();
            sendButton.onClick.AddListener(() => FriendSystemManager.Instance.SendFriendRequest(playerId));
            sendButton.gameObject.SetActive(true);
        }
        else
        {
            sendButton.gameObject.SetActive(false);
        }
    }
}
