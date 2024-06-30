using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using TMPro;
using UnityEngine.UI;
using System;

public class MatchmakingManager : MonoBehaviour
{
    DatabaseReference databaseReference;
    FirebaseAuth auth;
   
    public Button RemoveButton;
    string playerId;

    int currentTickets;
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
                playerId = auth.CurrentUser.UserId;

                // Set up listener after initializing Firebase
                SetupPlayerValueListener();

                UserManager.Instance.OnUserHintsUpdated += updateUserTickets;
                UserManager.Instance.CheckUserProfileCompletion();
            }
            else
            {
                Debug.LogError("Failed to initialize Firebase");
            }
        });

       void updateUserTickets(Dictionary<string, object> userHints)
        {
            if (userHints.TryGetValue("tickets", out object TicketsObj))
            {
                currentTickets = Convert.ToInt32(TicketsObj);
                Debug.Log("current tickets " + currentTickets);
            }
            else
            {
                Debug.LogError("tickets key is missing in hintsData");
            }
        }

        // Set up the remove button click listener
        if (RemoveButton != null)
        {
            RemoveButton.onClick.AddListener(RemovePlayerFromMatchmaking);
        }
    }

    // Set up listener for changes in the player's matchmaking data
    void SetupPlayerValueListener()
    {
        if (databaseReference != null && !string.IsNullOrEmpty(playerId))
        {
            DatabaseReference playerRef = databaseReference.Child("matchmaking").Child(playerId);

            playerRef.ValueChanged += HandlePlayerValueChanged;
        }
        else
        {
            Debug.LogError("Database reference is not initialized or playerId is empty.");
        }
    }

    // Handler for value changes
    void HandlePlayerValueChanged(object sender, ValueChangedEventArgs e)
    {
        if (e.DatabaseError != null)
        {
            Debug.LogError("Error while receiving value change: " + e.DatabaseError.Message);
            return;
        }

        if (e.Snapshot.Exists)
        {
            string currentValue = e.Snapshot.GetValue(true).ToString();

            // Check if the value is no longer the placeholder
            if (currentValue != "placeholder")
            {
                // Value has changed from "placeholder" to something else (likely a room ID)
                RemoveButton.gameObject.SetActive(false); // Hide the RemoveButton
          
                Debug.Log("Player matched. Room ID: " + currentValue);
            }
            else
            {
                Debug.Log("Player is still in the matchmaking queue.");
            }
        }
        else
        {
            // Optionally, handle the case where the value does not exist anymore
            Debug.Log("Player value removed or does not exist.");
        }
    }

    // Method to add the current player to the matchmaking queue
    public void AddPlayerToMatchmaking()
    {
       // if(currentTickets > 0)
        {
            if (auth.CurrentUser != null)
            {

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
        /*else
        {
            Debug.LogError("there is no enough tickets");
        }*/
        
    }

    // Method to remove the current player from the matchmaking queue
    public void RemovePlayerFromMatchmaking()
    {
        
        if (auth.CurrentUser != null)
        {
            // Check if the database reference is valid
            if (databaseReference != null)
            {
                // Remove the player from the matchmaking node in the Realtime Database
                DatabaseReference matchmakingRef = databaseReference.Child("matchmaking").Child(playerId);
                matchmakingRef.RemoveValueAsync().ContinueWith(task =>
                {
                    if (task.IsFaulted || task.IsCanceled)
                    {
                        Debug.LogError("Failed to remove player from matchmaking: " + task.Exception);
                    }
                    else
                    {
                        Debug.Log("Player removed from matchmaking successfully");

                        // Optionally re-enable the StartButton if you want to allow rejoining the queue
                   
                        RemoveButton.gameObject.SetActive(true);
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
