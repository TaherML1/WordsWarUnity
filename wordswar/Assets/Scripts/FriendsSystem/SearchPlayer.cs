using UnityEngine;
using UnityEngine.UI;
using Firebase.Firestore;
using TMPro;
using Firebase.Extensions;

public class SearchPlayer : MonoBehaviour
{
    [SerializeField] TMP_InputField searchInput;
    [SerializeField] GameObject playerSearchResultPrefab;
    [SerializeField] Transform resultParent;

    private FirebaseFirestore db;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
    }

    public void OnSearchButtonClicked()
    {
        string playerId = searchInput.text;
        Debug.Log("Search button clicked. Player ID: " + playerId);
        SearchPlayerById(playerId);
    }

    private void SearchPlayerById(string playerId)
    {
        Debug.Log("Searching for player ID: " + playerId);
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
                            DisplaySearchResult(playerId, username);
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
                    DisplaySearchResult("", "Player not found.");
                }
            }
            else
            {
                Debug.LogError("Search query failed: " + task.Exception.Message);
                DisplaySearchResult("", "Error: " + task.Exception.Message);
            }
        });
    }

    private void DisplaySearchResult(string playerId, string message)
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
        if (!string.IsNullOrEmpty(playerId))
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
