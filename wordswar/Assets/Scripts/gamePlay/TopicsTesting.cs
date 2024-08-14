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
using Firebase.Extensions;


public class TopicsTesting : MonoBehaviour
{
    [SerializeField] FeedbackManager feedbackManager;

    public Chat ChatInstance;
    private FirebaseDatabase database;
    private DatabaseReference databaseReference;

    private FirebaseAuth auth;
    private FirebaseFirestore db;
    private FirebaseFunctions functions;


    public TMP_InputField playerInput;
    public Button submitButton;

    bool wordExists;
   public bool islocalplayer = true;

   public string selectedTopic = "بلدان قارة اسيا";
    void Start()
    {
        Task<DependencyStatus> firebaseTask = FirebaseApp.CheckAndFixDependenciesAsync();

        if (firebaseTask.Result == DependencyStatus.Available)
        {
            // Initialize Firebase components
            database = FirebaseDatabase.DefaultInstance;
            auth = FirebaseAuth.DefaultInstance;
            db = FirebaseFirestore.DefaultInstance;
            functions = FirebaseFunctions.DefaultInstance;

            submitButton.onClick.AddListener(submitAnswer);
        }
        else
        {
            Debug.LogError("Failed to initialize Firebase");
        }
    }



    public async void submitAnswer()
    {
        submitButton.interactable = false;
        string currentInput = playerInput.text.ToLower();
        currentInput = NormalizeWord(currentInput);
        Debug.Log("your current input is : " + currentInput);
        wordExists = await CallCloudFunction(selectedTopic, currentInput);
    //    ChatInstance.GetMessage(currentInput, islocalplayer);
        if (wordExists)
        {
            submitButton.interactable = true;
            Debug.Log("your word is correct");
            feedbackManager.ShowFeedback("your word is correct");
        }else
        {
            submitButton.interactable = true;
            Debug.Log("your word is not correct");
            feedbackManager.ShowFeedback("your word in not correct");
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
}
