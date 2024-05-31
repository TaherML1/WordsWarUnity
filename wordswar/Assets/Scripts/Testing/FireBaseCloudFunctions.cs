using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions; // for ContinueWithOnMainThread
using TMPro;
using Firebase.Functions;
using System;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine.Networking;


[Serializable]
public class TopicsData
{
    public Dictionary<string, List<string>> topics;
}
public class FireBaseCloudFunctions : MonoBehaviour
{
    private const string cloudFunctionUrl = "https://us-central1-finalwordswar2.cloudfunctions.net/getRandomTopic";
    private FirebaseDatabase database;
    private DatabaseReference reference;
    private FirebaseFunctions functions;

    public TMP_InputField playerInput;
    public TMP_InputField topicInput;
    string topicName = " البلدان";
    string word = "سوريا  ";
    // Start is called before the first frame update
    void Start()
    {
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        functions = FirebaseFunctions.DefaultInstance;
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                
                Task.Run(async () =>
                {
                    bool exists = await CheckTopicAndWordExistence(topicName, word);
                    if (exists)
                    {
                        Debug.Log($"Word '{word}' exists under the topic '{topicName}'.");
                    }
                    else
                    {
                        Debug.LogWarning($"Word '{word}' does not exist under the topic '{topicName}'.");
                    }
                });
            }
            else
            {
                Debug.LogError("Failed to initialize Firebase: " + task.Result.ToString());
            }
        });

        FetchRandomTopic();


      CallCloudFunction(topicName, word);
    }
    public void OnSubmitButtonClick()
    {
        string currentinput = playerInput.text;
        string currentTopic = topicInput.text;
        Debug.Log("Button clicked, input is: " + currentinput + " " + currentTopic);
        CallCloudFunction(currentTopic, currentinput);
        // Do something with the user input here, e.g., save it, process it, etc.
    }
    public async Task<bool> CheckTopicAndWordExistence(string topicName, string word)
    {
        topicName = topicName.Trim();
        word = word.Trim();

        // Check if the topic exists
        DataSnapshot topicSnapshot = await reference.Child("topics").Child(topicName).GetValueAsync();
        if (!topicSnapshot.Exists)
        {
            Debug.LogWarning($"Topic '{topicName}' does not exist.");
            return false;
        }

        // Check if the word exists under the topic
        DataSnapshot wordSnapshot = topicSnapshot.Child(word);
        if (!wordSnapshot.Exists)
        {
            Debug.LogWarning($"Word '{word}' does not exist under the topic '{topicName}'.");
            return false;
        }

        return true; // Both topic and word exist
    }

    public async void CallCloudFunction(string topicName, string word)
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
            HttpsCallableReference func = functions.GetHttpsCallable("checkTopicAndWordExistence");
            HttpsCallableResult result = await func.CallAsync(data);

            // Handle the result
            bool exists = (bool)result.Data;
            if (exists)
            {
                Debug.Log($"Word '{word}' exists under the topic '{topicName}'.");
            }
            else
            {
                Debug.LogWarning($"Word '{word}' does not exist under the topic '{topicName}'.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to call Cloud Function: {e.Message}");
        }
    }

    private IEnumerator GetRandomTopicCoroutine()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(cloudFunctionUrl))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string randomTopic = webRequest.downloadHandler.text;
                Debug.Log("Randomly selected topic: " + randomTopic);
                // Use the randomly selected topic in your game
            }
            else
            {
                Debug.LogError("Failed to fetch random topic: " + webRequest.error);
            }
        }
    }

    public void FetchRandomTopic()
    {
        StartCoroutine(GetRandomTopicCoroutine());
    }
}
