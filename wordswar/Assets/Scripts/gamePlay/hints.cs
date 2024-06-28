using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Extensions;
using Firebase.Functions;
using Firebase.Firestore;
using TMPro;
using Firebase.Auth;
using Firebase.Database;
using System;

public class hints : MonoBehaviour
{
    public FeedbackManager feedbackManager;

    private FirebaseFunctions functions;
    private FirebaseAuth auth;
    string localPlayerId;
    string roomId;
    int jokerHints;
    int extraTimeHints;
    public TextMeshProUGUI hintText;

    void Start()
    {
        roomId = PlayerPrefs.GetString("roomId");

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError($"Firebase Initialization Error: {task.Exception}");
                return;
            }

            auth = FirebaseAuth.DefaultInstance;
            functions = FirebaseFunctions.DefaultInstance;
            localPlayerId = auth.CurrentUser.UserId;
            UserManager.Instance.OnUserHintsUpdated += UpdateUserHints;

            UserManager.Instance.CheckUserProfileCompletion();

        });
    }

    private void OnDestroy()
    {
        // Unsubscribe from events to avoid memory leaks
        UserManager.Instance.OnUserHintsUpdated -= UpdateUserHints;
    }

    private void UpdateUserHints(Dictionary<string, object> userHints)
    {
        // Update UI elements with user hints data
        if (userHints.TryGetValue("joker", out object jokerObj) && userHints.TryGetValue("extraTime", out object extraTimeObj))
        {
            jokerHints = Convert.ToInt32(jokerObj);
            extraTimeHints = Convert.ToInt32(extraTimeObj);
            Debug.Log("joker hints : " + jokerHints);
            Debug.Log("extra times : " + extraTimeHints);
        }
        else
        {
            Debug.LogError("joker or extraTime key is missing in hintsData");
        }
    }




    public void onclickJokerButton()
    {

        if (jokerHints != 0)
        {
            GetJokerHint(roomId, selectedTopicManager.selectedTopic);
        }
        else
        {
            feedbackManager.ShowFeedback("you dont have enough hints");
            Debug.Log("you dont have enough hints");
        }


    }

    void GetJokerHint(string gameId, string selectedTopic)
    {
        // Create the data object to be sent to the Cloud Function
        var data = new Dictionary<string, object>
        {
            {"gameId", gameId},
            {"selectedTopic", selectedTopic}

        };

        // Call the Cloud Function
        functions.GetHttpsCallable("getJokerHint2")
            .CallAsync(data).ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("Cloud Function Call Failed: " + "there is no more words");
                    return;
                }

                if (task.IsCompleted)
                {
                    var result = (string)task.Result.Data;
                    Debug.Log("Joker Hint Word: " + result);

                    // Now you can use the result in your Unity application as needed
                    hintText.text = result;
                    //hintListener();
                }
            });
    }

}