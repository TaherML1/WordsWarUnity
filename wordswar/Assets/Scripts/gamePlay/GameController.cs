using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using ArabicSupport;
using TMPro;
using Firebase;
using Firebase.Database;
using System.Collections.Generic;
using Firebase.Auth;
using System;
using Firebase.Functions;
using System.Threading.Tasks;
using Firebase.Firestore;
using UnityEngine.Networking;

/// <summary>
/// Manages the game logic, including player scores, turns, and word submission.
/// </summary>
public class GameController : MonoBehaviour
{
    public static GameController instance;
    [SerializeField] FeedbackManager feedbackManager;
    [SerializeField] Chat ChatInstance;
    [SerializeField] MessageAnimator messageAnimator;

    private FirebaseDatabase database;
    private DatabaseReference databaseReference;
    private DatabaseReference turnReference;
    private FirebaseAuth auth;
    private FirebaseFirestore db;
    private FirebaseFunctions functions;

    [Header("Stats")]
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] TextMeshProUGUI topicText;
    [SerializeField] TextMeshProUGUI resultText;
    [SerializeField] TextMeshProUGUI invalidWordText;
    [SerializeField] TextMeshProUGUI TurnLabel;
    [Header("EnemyPlayer")]
    [SerializeField] TextMeshProUGUI enemyScoreText;
    [SerializeField] TextMeshProUGUI enemyNameText;
    [SerializeField] Image enemyProfileImage;

    [Header("LocalPlayer")]
    [SerializeField] TextMeshProUGUI localPlayScoreText;
    [SerializeField] TextMeshProUGUI localPlayerNameText;
    [SerializeField] Image localPlayerProfileImage;

    [Header("UI Elements")]
    [SerializeField] TMP_InputField playerInput;
    [SerializeField] ScrollRect scrollView;
    [SerializeField] Button submitButton;
    [SerializeField] Button jokerHintButton;
    [SerializeField] AudioSource correctSound;
    [SerializeField] AudioSource incorrectSound;
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] GameObject panelToHideKeyboard;

    [SerializeField] GameOverController gameOverController;

    [Header("Game Over Panel")]
    [SerializeField] TextMeshProUGUI winnerText;
    [SerializeField]

    private string selectedTopic;

    bool wordExists;
    bool wordIsUsed;

    private float timer; // Timer variable
    private float originalTimer;

    string localPlayerId; // Assuming you have a way to identify the local player
    string enemyPlayerId;
    bool isLocalPlayerTurn = true;
    string roomId;
    HashSet<string> displayedMessages = new HashSet<string>(); // Maintain a set to store displayed messages

    private async void Awake()
    {
        instance = this;

        roomId = PlayerPrefs.GetString("roomId");

        // Check if Firebase is already initialized
        if (FirebaseManager.Instance != null && FirebaseManager.Instance.IsFirebaseInitialized)
        {
            InitializeFirebaseComponents();
        }
        else
        {
            // Wait until Firebase is initialized
            FirebaseManager.Instance.OnFirebaseInitialized += HandleFirebaseInitialized;
        }
    }

    private void HandleFirebaseInitialized()
    {
        InitializeFirebaseComponents();
    }

    private async void InitializeFirebaseComponents()
    {
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        database = FirebaseDatabase.DefaultInstance;
        functions = FirebaseFunctions.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;
        localPlayerId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;

        InitializeGame();

        await FetchPlayerNameAsync(localPlayerId, localPlayerNameText, localPlayerProfileImage);

        submitButton.onClick.AddListener(submitAnswer);
    }

    private void InitializeGame()
    {
        // Call functions that should only be called once
        CheckTurn(localPlayerId, roomId);
        FetchAndDisplaySelectedTopic(roomId);
        Invoke("FetchGameInfoDelayed", 1f);
        ListenForScoreChanges(roomId);
        ListenForUsedWordsUpdates(roomId);
        WinnerValueChangedListener(roomId);
        listenForTime(roomId);
    }

    // Just for testing to avoid displaying name error
    private void FetchGameInfoDelayed()
    {
        FetchGameInfo(roomId);
    }

    void SetTopic(string topic)
    {
        topicText.text = ArabicFixer.Fix(topic) + ArabicFixer.Fix(" الموضوع : ");
    }

    public async void submitAnswer()
    {
        submitButton.interactable = false;
        string currentInput = playerInput.text.ToLower();
        currentInput = NormalizeWord(currentInput);

        if (string.IsNullOrEmpty(currentInput))
        {
            Debug.LogWarning("Empty input word.");

            feedbackManager.ShowFeedback("Empty input word.");
            submitButton.interactable = true;

            return;
        }

        wordIsUsed = await CheckIfWordIsUsed(roomId, selectedTopicManager.selectedTopic, currentInput);
        if (wordIsUsed)
        {
            // Handle the case where the word has already been used
            Debug.LogWarning("The word has already been used.");
            feedbackManager.ShowFeedback("تم استخدام هذه الكلمة");
            submitButton.interactable = true;
            return; // Exit the function early
        }
        Debug.Log("Current Input: " + currentInput);
        Debug.Log("Selected Topic: " + selectedTopicManager.selectedTopic);
        wordExists = await CallCloudFunction(selectedTopicManager.selectedTopic, currentInput);
        if (wordExists)
        {
            Debug.Log("roomm id from submit word is : " + roomId);
            Debug.Log("Word exists in the database from word exist .");
            //UpdateScore();
            feedbackManager.ShowFeedback("Correct!");
            timer = originalTimer;
            IncrementPlayerScore(roomId, localPlayerId);
            newSwitchTurn(roomId, localPlayerId);
            // SwitchTurn();

            correctSound.Play();
            clearInputfiled();
            ListenForUsedWordsUpdates(roomId);
            // Update the used words in the database
            UpdateUsedWordsInDatabase(roomId, currentInput, localPlayerId, true);
        }
        else
        {
            // The word does not exist in the database
            Debug.LogWarning("Word does not exist in the database from word exist.");
            feedbackManager.ShowFeedback("خطا,كلمة لا تتعلق بالموضوع");
            invalidWordText.gameObject.SetActive(true);
            UpdateUsedWordsInDatabase(roomId, currentInput, localPlayerId, false);
            // Set the timer to zero when a wrong answer is submitted
            incorrectSound.Play();
            ListenForUsedWordsUpdates(roomId);
            clearInputfiled();
            //  Invoke("determinewinner", 1f);
            submitButton.interactable = true;
        }
    }

    string NormalizeWord(string word)
    {
        // Normalize the word by removing diacritics (like hamza) and other variations
        // Replace "أ" (alef with hamza above) with "ا" (alef without hamza)
        word = word.Replace("أ", "ا");
        // Replace "آ" (alef with madda) with "ا" (alef without madda)
        word = word.Replace("آ", "ا");
        // Remove hamza (ء) from the word
        word = word.Replace("ء", ""); // Or use word.Replace("ء", string.Empty);

        // Add more normalization rules as needed

        return word;
    }

    async void FetchAndDisplaySelectedTopic(string roomId)
    {
        try
        {
            Debug.Log("Fetching selected topic for room: " + roomId);

            if (databaseReference != null)
            {
                Debug.Log("Database reference is not null.");

                // Construct the correct database reference path to fetch the selected topic
                DataSnapshot snapshot = await databaseReference.Child("games").Child(roomId).Child("gameInfo").Child("selectedTopic").GetValueAsync();

                if (snapshot.Exists)
                {
                    selectedTopic = snapshot.Value.ToString();
                    selectedTopicManager.selectedTopic = selectedTopic;
                    SetTopic(selectedTopic); // Update UI with the selected topic
                    Debug.Log("Selected topic fetched and displayed: " + selectedTopic);
                }
                else
                {
                    Debug.LogError("Selected topic not found in the database.");
                }
            }
            else
            {
                Debug.LogError("Database reference is not initialized.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error fetching and displaying selected topic: " + e.Message);
        }
    }

    public async Task<bool> CallCloudFunction(string topicName, string word)
    {
        topicName = topicName.Trim();
        word = word.Trim();

        // Create data dictionary with topicName and word
        var data = new Dictionary<string, object>
    {
        { "topicName", topicName },
        { "word", word }
    };

        try
        {
            // Call the Cloud Function
            HttpsCallableReference func = functions.GetHttpsCallable("checkTopicAndWordExistence2");
            HttpsCallableResult result = await func.CallAsync(data);

            // Handle the result
            bool exists = (bool)result.Data;
            if (exists)
            {
                Debug.Log($"Word '{word}' exists under the topic '{topicName}'.");
            }
            else
            {
                Debug.Log($"Word '{word}' does not exist under the topic '{topicName}'.");
            }

            return exists; // Return the result
        }
        catch (Exception e)
        {
            Debug.Log($"Failed to call Cloud Function: {e.Message}");
            return false; // Return false in case of an error
        }
    }

    async void FetchGameInfo(string gameId)
    {
        try
        {
            Debug.Log("Fetching game info for gameId: " + gameId);

            // Fetch game info and player IDs...
            DatabaseReference gameRef = FirebaseDatabase.DefaultInstance.RootReference.Child("games").Child(gameId).Child("gameInfo");
            DataSnapshot snapshot = await gameRef.GetValueAsync();

            if (!snapshot.Exists)
            {
                Debug.LogError("Game info snapshot does not exist.");
                return;
            }

            List<string> playerIds = new List<string>();
            foreach (var playerIdSnapshot in snapshot.Child("playersIds").Children)
            {
                playerIds.Add(playerIdSnapshot.Value.ToString());
            }

            // Assuming there are always two players
            string localPlayerId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
            enemyPlayerId = playerIds.Find(id => id != localPlayerId);

            // Fetch enemy player name asynchronously
            await FetchPlayerNameAsync(enemyPlayerId, enemyNameText, enemyProfileImage);
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to fetch game info: " + ex.Message);
        }
    }

    public async Task FetchPlayerNameAsync(string playerId, TextMeshProUGUI playerNameText, Image ProfileImage)
    {
        try
        {
            DocumentReference docRef = db.Collection("users").Document(playerId);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                Dictionary<string, object> userData = snapshot.ToDictionary();
                string username = userData["username"].ToString();
                if (userData.TryGetValue("profileImageURL", out object profileImageObject))
                {
                    string profileImageURL = profileImageObject as string;
                    if (profileImageURL != null)
                    {
                        Debug.Log("Profile Image URL found: " + profileImageURL);
                        StartCoroutine(LoadProfileImage(profileImageURL, ProfileImage));
                    }
                    else
                    {
                        Debug.Log("Profile Image URL is not a string.");
                    }
                }
                else
                {
                    Debug.Log("Profile Image URL not found.");
                }

                // Display the username in the UI
                playerNameText.text = username;
                playerNameText.gameObject.SetActive(true); // Activate the playerNameText GameObject

                Debug.Log("Player name fetched and displayed: " + username);
            }
            else
            {
                Debug.LogError("User profile not found in the database.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to fetch player name: " + ex.Message);
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

    void CheckTurn(string localPlayerId, string gameId)
    {
        // Get the current turn reference
        turnReference = databaseReference.Child("games").Child(gameId).Child("turn");

        // Add a listener for value changes
        turnReference.ValueChanged += OnTurnValueChanged;
    }

    void ListenForScoreChanges(string gameId)
    {
        // Construct the reference to the scores node for the specific game
        DatabaseReference scoresRef = databaseReference.Child("games").Child(gameId).Child("gameInfo").Child("scores");

        scoresRef.ValueChanged += OnScoresChanged;
    }

    void OnScoresChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError("Database error: " + args.DatabaseError.Message);
            return;
        }

        // Parse and update the scores
        DataSnapshot scoresSnapshot = args.Snapshot;
        if (scoresSnapshot != null && scoresSnapshot.HasChildren)
        {
            foreach (DataSnapshot playerSnapshot in scoresSnapshot.Children)
            {
                string playerId = playerSnapshot.Key;
                int score = int.Parse(playerSnapshot.Value.ToString());

                // Update the UI or game logic with the scores
                Debug.Log("Player " + playerId + " has a score of: " + score);

                // Example: Update UI with player scores
                if (playerId == localPlayerId)
                {
                    // Update UI with local player's score
                    localPlayScoreText.text = score.ToString();
                }
                else
                {
                    // Update UI with opponent's score
                    enemyScoreText.text = score.ToString();
                }
            }
        }
    }

    void OnTurnValueChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError("Database error: " + args.DatabaseError.Message);
            return;
        }

        // Get the current turn from the event args
        string currentTurn = args.Snapshot.GetValue(true).ToString();

        // Check if it's the local player's turn
        if (currentTurn == localPlayerId)
        {
            Debug.Log("It's your turn!");
            // Enable the input field
            TurnLabel.text = "انه دورك";
            playerInput.interactable = true;
            submitButton.interactable = true;
            jokerHintButton.interactable = true;
            panelToHideKeyboard.SetActive(false);
            timer = originalTimer;
        }
        else
        {
            Debug.Log("It's the opponent's turn.");

            TurnLabel.text = "دور الخصم";
            // Disable the input field
            playerInput.interactable = false;
            submitButton.interactable = false;
            jokerHintButton.interactable = false;
            panelToHideKeyboard.SetActive(true);
            timer = originalTimer;
        }
    }

    public void IncrementPlayerScore(string gameId, string playerId)
    {
        DatabaseReference playerScoreRef = databaseReference.Child("games").Child(gameId).Child("gameInfo").Child("scores").Child(playerId);

        playerScoreRef.RunTransaction(mutableData =>
        {
            int currentScore = mutableData.Value != null ? int.Parse(mutableData.Value.ToString()) : 0;
            mutableData.Value = currentScore + 1;
            return TransactionResult.Success(mutableData);
        }).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Failed to increment player score: " + task.Exception);
            }
            else if (task.IsCompleted)
            {
                Debug.Log("Player score incremented successfully.");
            }
        });
    }

    // Unsubscribe from the event when the GameObject is destroyed
    void OnDestroy()
    {
        if (turnReference != null)
        {
            turnReference.ValueChanged -= OnTurnValueChanged;
        }
    }

    public void newSwitchTurn(string gameId, string currentPlayerId)
    {
        // Get the game reference
        DatabaseReference gameRef = databaseReference.Child("games").Child(gameId);

        // Get the current turn from the database
        gameRef.Child("turn").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Failed to read turn from database: " + task.Exception);
                return;
            }

            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                string currentTurn = snapshot.Value.ToString(); // Use .Value instead of .GetValue(true)

                // Check if it's the current player's turn
                if (currentTurn == currentPlayerId)
                {
                    // Get the index of the current player
                    gameRef.Child("gameInfo").Child("playersIds").GetValueAsync().ContinueWith(playersTask =>
                    {
                        if (playersTask.IsFaulted)
                        {
                            Debug.LogError("Failed to read playersIds from database: " + playersTask.Exception);
                            return;
                        }

                        if (playersTask.IsCompleted)
                        {
                            DataSnapshot playersSnapshot = playersTask.Result;
                            var playerIds = playersSnapshot.Value as List<object>;
                            if (playerIds != null)
                            {
                                int currentPlayerIndex = playerIds.IndexOf(currentPlayerId);
                                // Calculate the index of the next player
                                int nextPlayerIndex = (currentPlayerIndex + 1) % playerIds.Count;

                                // Get the next player's ID
                                string nextPlayerId = playerIds[nextPlayerIndex].ToString();

                                // Update the turn in the database
                                gameRef.Child("turn").SetValueAsync(nextPlayerId);
                            }
                            else
                            {
                                Debug.LogError("Failed to parse playerIds from database.");
                            }
                        }
                    });
                }
                else
                {
                    Debug.LogWarning("It's not your turn to switch.");
                }
            }
        });
    }

    void UpdateUsedWordsInDatabase(string roomId, string word, string localPlayerId, bool isCorrect)
    {
        try
        {
            // Construct the reference to the used words node for the specific game
            DatabaseReference usedWordsRef = databaseReference.Child("games").Child(roomId).Child("gameInfo").Child("usedwords").Child(localPlayerId);

            // Push the new word to the correct list (correct or incorrect)
            if (isCorrect)
            {
                usedWordsRef.Child("correct").Push().SetValueAsync(word);
            }
            else
            {
                usedWordsRef.Child("incorrect").Push().SetValueAsync(word);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to update used words in the database: " + ex.Message);
        }
    }

    public async Task<bool> CheckIfWordIsUsed(string roomId, string topicName, string word)
    {
        var checkWordUsage = functions.GetHttpsCallable("checkWordUsage");

        try
        {
            var data = new Dictionary<string, object>
            {
                { "roomId", roomId },
                { "topicName", topicName },
                { "word", word }
            };

            var result = await checkWordUsage.CallAsync(data);

            // Directly get the boolean result from the response
            bool isUsed = Convert.ToBoolean(result.Data);

            if (isUsed)
            {
                Debug.Log("word has been used");
            }
            else
            {
                Debug.Log("word has not been used");
            }

            return isUsed;
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to check word usage: " + ex.Message);
            return false;
        }
    }

    void ListenForUsedWordsUpdates(string roomId)
    {
        // Construct the reference to the used words node for the specific game
        DatabaseReference usedWordsRef = databaseReference.Child("games").Child(roomId).Child("gameInfo").Child("usedwords");

        // Listen for changes in the usedWords node
        usedWordsRef.ChildChanged += (sender, args) =>
        {
            // Update UI with the new used words
            FetchAndDisplayUsedWords(roomId, localPlayerId);
        };
    }

    async void FetchAndDisplayUsedWords(string roomId, string localPlayerId)
    {
        try
        {
            // Construct the reference to the used words node for the specific game
            DatabaseReference usedWordsRef = databaseReference.Child("games").Child(roomId).Child("gameInfo").Child("usedwords");

            // Fetch used words from the database
            DataSnapshot snapshot = await usedWordsRef.GetValueAsync();

            if (snapshot != null && snapshot.Exists)
            {
                // Iterate through each child snapshot to retrieve used words
                foreach (DataSnapshot playerSnapshot in snapshot.Children)
                {
                    string playerId = playerSnapshot.Key;

                    // Iterate through each word under the player's ID node
                    foreach (DataSnapshot categorySnapshot in playerSnapshot.Children)
                    {
                        string category = categorySnapshot.Key; // 'correct' or 'incorrect'
                        bool isCorrect = category == "correct"; // Flag to indicate if the word is correct

                        foreach (DataSnapshot wordSnapshot in categorySnapshot.Children)
                        {
                            string usedWord = wordSnapshot.Value.ToString();

                            // Check if the word is not null or empty
                            if (!string.IsNullOrEmpty(usedWord))
                            {
                                // Determine which player used the word and add it to the corresponding list
                                bool isLocalPlayer = playerId == localPlayerId;
                                Debug.Log((isLocalPlayer ? "The local player" : "The enemy player") + " submitted: " + usedWord + " (" + category + ")");

                                // Check if the message has been displayed already
                                if (!displayedMessages.Contains(usedWord))
                                {
                                    // If not, display the message and add it to the set of displayed messages
                                    ChatInstance.GetMessage(usedWord, isLocalPlayer, isCorrect);
                                    displayedMessages.Add(usedWord);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                Debug.Log("no words");
                // If no used words are found, display a message or clear the UI elements
                // Optionally handle this case
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to fetch and display used words: " + ex.Message);
        }
    }


    private void StartListeningForUsedWords(string roomId, string localPlayerId)
    {
        // Construct the reference to the used words node for the specific game
        DatabaseReference usedWordsRef = databaseReference.Child("games").Child(roomId).Child("gameInfo").Child("usedWords");

        // Add a listener for child added event
        usedWordsRef.ChildAdded += HandleUsedWordAdded;
    }

    private void HandleUsedWordAdded(object sender, ChildChangedEventArgs args)
    {
        List<string> enemyPlayerUsedWords = new List<string>();

        if (args.Snapshot != null && args.Snapshot.Exists)
        {
            foreach (DataSnapshot playerSnapshot in args.Snapshot.Children)
            {
                string playerId = playerSnapshot.Key; // Get the player ID

                Debug.Log("Player ID: " + playerId); // Log the player ID

                foreach (DataSnapshot wordSnapshot in playerSnapshot.Children)
                {
                    string usedWord = wordSnapshot.Value.ToString();

                    Debug.Log(usedWord);

                    enemyPlayerUsedWords.Add(usedWord);
                }

                string enemyPlayerUsedWordsString = string.Join("\n", enemyPlayerUsedWords);
            }
        }
    }

    private void clearInputfiled()
    {
        playerInput.text = string.Empty;
    }

    public void SetGameEnd(string roomId)
    {
        // Set the gameEnd node to trigger the cloud function
        databaseReference.Child("games").Child(roomId).Child("gameEnd").SetValueAsync(true)
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("Failed to set gameEnd: " + task.Exception);
                }
                else if (task.IsCompleted)
                {
                    Debug.Log("gameEnd set successfully.");
                }
            });
    }

    private void determinewinner(string roomId, string localplayerId)
    {
        databaseReference.Child("games").Child(roomId).Child("winner").SetValueAsync(localplayerId)
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("Failed to set winner: " + task.Exception);
                }
                else
                {
                    Debug.Log("you are the winner");
                }
            });
    }

    private void WinnerValueChangedListener(string roomId)
    {
        // Construct the reference to the winner node for the specific game
        DatabaseReference winnerRef = databaseReference.Child("games").Child(roomId).Child("winner");

        // Add the value changed listener
        winnerRef.ValueChanged += HandleWinnerChange;
    }

    private void HandleWinnerChange(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError("Database error: " + args.DatabaseError.Message);
            return;
        }

        if (args.Snapshot != null && args.Snapshot.Exists)
        {
            roomId = "";
            string winnerId = args.Snapshot.Value.ToString();

            if (winnerId == localPlayerId)
            {
                Debug.Log("you are the winner");
                winnerText.text = "أنت الفائز";
            }
            else
            {
                Debug.Log("you lost");
                winnerText.text = "أنت الخاسر";
            }

            // Call the method to show the game over panel
            ShowGameOverPanel(winnerId == localPlayerId);
        }
    }

    public void ShowGameOverPanel(bool isWinner)
    {
        if (gameOverPanel != null)
        {
            // Activate the game over panel
            gameOverPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("gameOverPanel is null!");
        }
    }

    async void listenForTime(string roomId)
    {
        DatabaseReference timerRef = databaseReference.Child("games").Child(roomId).Child("gameInfo").Child("timer");
        DataSnapshot snapshot = await timerRef.GetValueAsync();
        if (snapshot != null && snapshot.Value != null)
        {
            originalTimer = Convert.ToSingle(snapshot.Value); // Store the original timer value
            timer = originalTimer; // Set the timer to the original value
            StartCoroutine(StartCountdown());
        }
    }

    IEnumerator StartCountdown()
    {
        while (timer > 0)
        {
            // Display the remaining time
            timerText.text = timer.ToString("F0"); // Display as integer
            yield return new WaitForSeconds(1f); // Wait for 1 second
            timer--;
        }
        timerText.text = "time is over"; // Ensure it shows 0 when finished

        if (isLocalPlayerTurn)
        {
            SetGameEnd(roomId);
        }
    }
}

public static class selectedTopicManager
{
    public static string selectedTopic { get; set; }
}
