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
    [SerializeField] GameObject purchaseTicketPanel;
    [SerializeField] GameObject matchmakingPanel;

    DatabaseReference databaseReference;
    FirebaseAuth auth;

    public Button RemoveButton;
    string playerId;

    int currentTickets;
    int refreshedTickets;
    int totalTickets;

    private void Awake()
    {
        if (FirebaseManager.Instance != null && FirebaseManager.Instance.IsFirebaseInitialized)
        {
            InitializeFirebaseComponents();
        }
        else
        {
            StartCoroutine(WaitForFirebaseInitialization());
        }
    }

    private void InitializeFirebaseComponents()
    {
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        auth = FirebaseAuth.DefaultInstance;
        playerId = auth.CurrentUser.UserId;

        SetupPlayerValueListener();

        UserManager.Instance.OnUserHintsUpdated += UpdateUserTickets;
        UserManager.Instance.CheckUserProfileCompletion();

    }

    private void OnDestroy()
    {
        // Unsubscribe from events to avoid memory leaks
        UserManager.Instance.OnUserHintsUpdated -= UpdateUserTickets;
    }

    private IEnumerator WaitForFirebaseInitialization()
    {
        while (FirebaseManager.Instance == null || !FirebaseManager.Instance.IsFirebaseInitialized)
        {
            yield return null;
        }
        InitializeFirebaseComponents();
    }

    private void UpdateUserTickets(Dictionary<string, object> userHints)
    {
        if (userHints.TryGetValue("tickets", out object TicketsObj))
        {
            currentTickets = Convert.ToInt32(TicketsObj);
        }
        else
        {
            Debug.LogError("Tickets key is missing in hintsData");
        }

        if (userHints.TryGetValue("refreshedTickets", out object refreshedTicketsObj))
        {
            refreshedTickets = Convert.ToInt32(refreshedTicketsObj);
        }
        else
        {
            Debug.LogError("RefreshedTickets key is missing in hintsData");
        }

        // Calculate total tickets as the sum of currentTickets and refreshedTickets
        totalTickets = currentTickets + refreshedTickets;
        Debug.Log("match making total tickets : " + totalTickets);
    }

    private void SetupPlayerValueListener()
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

    private void HandlePlayerValueChanged(object sender, ValueChangedEventArgs e)
    {
        if (e.DatabaseError != null)
        {
            Debug.LogError("Error while receiving value change: " + e.DatabaseError.Message);
            return;
        }

        if (e.Snapshot.Exists)
        {
            string currentValue = e.Snapshot.GetValue(true).ToString();

            if (currentValue != "placeholder")
            {
                if (RemoveButton != null && RemoveButton.gameObject != null)
                {
                    RemoveButton.gameObject.SetActive(false);
                }
                Debug.Log("Player matched. Room ID: " + currentValue);
            }
            else
            {
                Debug.Log("Player is still in the matchmaking queue.");
            }
        }
        else
        {
            Debug.Log("Player value removed or does not exist.");
        }
    }

    public void AddPlayerToMatchmaking()
    {
        if (totalTickets > 0)
        {
            if (auth.CurrentUser != null)
            {
                if (databaseReference != null)
                {
                    matchmakingPanel.SetActive(true);

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
        else
        {
            purchaseTicketPanel.SetActive(true);
            Debug.LogError("There are not enough tickets");
        }
    }

    public void RemovePlayerFromMatchmaking()
    {
        if (auth.CurrentUser != null)
        {
            if (databaseReference != null)
            {
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
