using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;

public class LeaderboardManager : MonoBehaviour
{
    FirebaseFirestore db;

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError($"Failed to initialize Firebase with {task.Exception}");
                return;
            }
            db = FirebaseFirestore.DefaultInstance;
            Debug.Log("Firebase initialized successfully.");
            RetrieveLeaderboardData();
        });

        Debug.Log("Start method finished."); // Add this log
    }

    void RetrieveLeaderboardData()
    {
        Debug.Log("fetchinggg");
        // Query to retrieve top N players based on scores
        db.Collection("users").OrderByDescending("scores").Limit(10).GetSnapshotAsync()
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    // Handle the error
                    Debug.LogError("Error fetching leaderboard data: " + task.Exception);
                    return;
                }

                // Process the retrieved data
                List<PlayerData> leaderboard = new List<PlayerData>();
                foreach (DocumentSnapshot document in task.Result.Documents)
                {
                    PlayerData player = document.ConvertTo<PlayerData>();
                    leaderboard.Add(player);
                }

                // Now 'leaderboard' contains the top N players based on scores
                // You can update your UI to display this leaderboard data
                UpdateLeaderboardUI(leaderboard);
            });

        Debug.Log("RetrieveLeaderboardData method finished."); // Add this log
    }

    void UpdateLeaderboardUI(List<PlayerData> leaderboard)
    {
        // Update your UI to display the leaderboard
        // For example, you can display usernames and scores in a text element
        foreach (PlayerData player in leaderboard)
        {
            Debug.Log("Username: " + player.username + ", Score: " + player.scores);
        }

        Debug.Log("UpdateLeaderboardUI method finished."); // Add this log
    }
}

// Define a class to hold player data
public class PlayerData
{
    public string username;
    public int scores;
    // Add other fields as needed (xp, level, etc.)
}
