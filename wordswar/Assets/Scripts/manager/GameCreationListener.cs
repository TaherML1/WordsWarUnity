using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using TMPro;


public class GameCreationListener : MonoBehaviour
{
    DatabaseReference databaseReference;
    bool isListening = false; // Flag to track if the listener is active
    string currentPlayerId; // Store the current player ID
    public TextMeshProUGUI countdownText; // For regular Text UI element

    [SerializeField] GameObject MatchStartPanel;
    // public TMP_Text countdownText; // Use this instead if using TextMeshPro
    public float countdownTime = 6f; // Countdown duration in seconds
    float currentCountdownTime; // To keep track of the countdown

    void Start()
    {
        // Ensure this object persists across scene changes
        DontDestroyOnLoad(gameObject);

        // Initialize Firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                // Set up the Firebase Database reference
                databaseReference = FirebaseDatabase.DefaultInstance.RootReference.Child("games");
                // Start listening for changes in the "games" node
                StartListening();

                // Get the current player ID if the user is authenticated
                if (FirebaseAuth.DefaultInstance.CurrentUser != null)
                {
                    currentPlayerId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
                }
                else
                {
                    Debug.LogWarning("Current user is not authenticated.");
                }
            }
            else
            {
                Debug.LogError("Failed to initialize Firebase");
            }
        });
    }

    void StartListening()
    {
        if (!isListening && databaseReference != null)
        {
            // Subscribe to the ValueChanged event
            databaseReference.ValueChanged += HandleGameCreation;
            isListening = true; // Set the flag to indicate that the listener is active
        }
    }

    void StopListening()
    {
        if (isListening && databaseReference != null)
        {
            // Unsubscribe from the ValueChanged event
            databaseReference.ValueChanged -= HandleGameCreation;
            isListening = false; // Set the flag to indicate that the listener is inactive
        }
    }

    private void HandleGameCreation(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError("Database error: " + args.DatabaseError.Message);
            return;
        }

        // Check if a new game entry was added
        if (args.Snapshot.HasChildren && args.Snapshot.ChildrenCount > 0)
        {
            foreach (var childSnapshot in args.Snapshot.Children)
            {
                string roomId = childSnapshot.Key;
                var gameInfo = childSnapshot.Child("gameInfo");

                // Check if current player ID is in the playersIds list
                if (gameInfo.HasChild("playersIds") && gameInfo.Child("playersIds").HasChildren)
                {
                    bool playerIsInRoom = false;
                    foreach (var playerIdSnapshot in gameInfo.Child("playersIds").Children)
                    {
                        if (playerIdSnapshot.Value.ToString() == currentPlayerId)
                        {
                            playerIsInRoom = true;
                            break;
                        }
                    }

                    // Check if the room has a status (not ended)
                    if (playerIsInRoom && !childSnapshot.HasChild("status") || childSnapshot.Child("status").Value.ToString() != "ended")
                    {
                        MatchStartPanel.SetActive(true);
                        // Transition to gameplay scene with countdown
                        PlayerPrefs.SetString("roomId", roomId);
                        currentCountdownTime = countdownTime;
                        InvokeRepeating("UpdateCountdown", 0f, 1f); // Start countdown
                        break; // Only join the first valid room
                    }
                }
            }
        }
    }

    private void UpdateCountdown()
    {
        if (countdownText != null)
        {
            countdownText.text = currentCountdownTime.ToString("F0") ;
        }

        if (currentCountdownTime > 0)
        {
            currentCountdownTime -= 1;
        }
        else
        {
            CancelInvoke("UpdateCountdown");
            StartGameplayScene();
        }
    }

    private void StartGameplayScene()
    {
        StopListening();
        if (SceneController.Instance != null)
        {
            SceneController.Instance.LoadGameplay();
        }
        else
        {
            Debug.LogError("SceneController instance not found. Cannot load GamePlay.");
        }
        Destroy(gameObject);
    }
}
