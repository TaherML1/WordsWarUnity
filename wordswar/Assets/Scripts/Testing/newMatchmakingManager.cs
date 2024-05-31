using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Functions;

public class newMatchmakingManager : MonoBehaviour
{
    DatabaseReference databaseReference;
    FirebaseAuth auth;
    FirebaseFunctions functions;


    void Start()
    {
        // Initialize Firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                // Set up the Firebase Database reference
                databaseReference = FirebaseDatabase.DefaultInstance.RootReference;

                // Initialize Firebase Authentication
                auth = FirebaseAuth.DefaultInstance;
            }
            else
            {
                Debug.LogError("Failed to initialize Firebase");
            }
        });
    }

    // Method to add the current player to the matchmaking queue
    public void AddPlayerToMatchmaking()
    {


        if (auth.CurrentUser != null)
        {
            // Get the current user's ID
            string playerId = auth.CurrentUser.UserId;

            // Check if the database reference is valid
            if (databaseReference != null)
            {
                // Add the player to the matchmaking node in the Realtime Database
                DatabaseReference matchmakingRef = databaseReference.Child("matchmaking").Child(playerId);
                matchmakingRef.SetValueAsync("placeholder").ContinueWith(task =>
                {
                    if (task.IsFaulted || task.IsCanceled)
                    {
                        Debug.LogError("Failed to add player to matchmaking: " + task.Exception);
                    }
                    else
                    {
                        Debug.Log("Player added to matchmaking successfully");

                    }
                });
            }
            else
            {
                Debug.LogError("Database reference is not initialized");
            }
        }
        else
        {
            Debug.LogError("Current user is not authenticated");
        }
    }



}
