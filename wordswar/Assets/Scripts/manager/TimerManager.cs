using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Functions;
using System;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using System.Collections.Generic;
using System.Collections;

public class TimerManager : MonoBehaviour
{
    public static TimerManager instance;
    FirebaseFirestore db;
    FirebaseAuth auth;
    FirebaseFunctions functions;
    [SerializeField] Text timerText;
    [SerializeField] GameObject Panel;

    int currentTickets;

    private Timestamp serverStartTime;
    private Timestamp targetTime;

    void Start()
    {
        Debug.Log("Starting Firebase dependency check.");
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                db = FirebaseFirestore.DefaultInstance;
                functions = FirebaseFunctions.DefaultInstance;
                Debug.Log("Firebase dependencies are available.");

                FirebaseUser user = auth.CurrentUser;
                RetrieveServerTime(user.UserId);
                UserManager.Instance.OnUserHintsUpdated += updateUserTickets;
                UserManager.Instance.CheckUserProfileCompletion();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + task.Result);
            }
        });
    }

    void RetrieveServerTime(string userId)
    {
        Debug.Log("Retrieving server time.");
        db.Collection("serverTime").Document("currentTime").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    serverStartTime = snapshot.GetValue<Timestamp>("time");
               //     Debug.Log("server time : " + serverStartTime);
                    RetrieveTimer(userId);
                }
                else
                {
                    Debug.LogError("No such document in serverTime/currentTime.");
                }
            }
            else
            {
                Debug.LogError("Error getting server time document: " + task.Exception);
            }
        });
    }

    void updateUserTickets(Dictionary<string, object> userHints)
    {
        if (userHints.TryGetValue("tickets", out object TicketsObj))
        {
            currentTickets = Convert.ToInt32(TicketsObj);
            Debug.Log("current ticketsssss " + currentTickets);
        }
        else
        {
            Debug.LogError("tickets key is missing in hintsData");
        }
    }

    void RetrieveTimer(string userId)
    {
        Debug.Log("Retrieving user timer.");
        DocumentReference docRef = db.Collection("users").Document(userId);
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    targetTime = snapshot.GetValue<Timestamp>("refreshTime");
                 //   Debug.Log($"Timer set for: {targetTime.ToDateTime()}");
                    StartCoroutine(StartCountdown());
                }
                else
                {
                    Debug.LogError("No such document in users/userId.");
                }
            }
            else
            {
                Debug.LogError("Error getting user timer document: " + task.Exception);
            }
        });
    }

    IEnumerator StartCountdown()
    {
        while (true)
        {
            if (currentTickets >= 3)
            {
                // If the player has 3 or more tickets, don't start the timer
                Debug.Log("You already have 3 tickets. No need to wait.");
                yield break;
            }

            TimeSpan remainingTime = targetTime.ToDateTime() - serverStartTime.ToDateTime();

            // Adjust the remaining time based on the number of tickets
            float ticketMultiplier = 1f;
            switch (currentTickets)
            {
                case 0:
                    ticketMultiplier = 3f;
                    break;
                case 1:
                    ticketMultiplier = 2f;
                    break;
                default:
                    ticketMultiplier = 1f;
                    break;
            }
            TimeSpan adjustedRemainingTime = TimeSpan.FromSeconds(remainingTime.TotalSeconds / ticketMultiplier);

            if (adjustedRemainingTime.TotalSeconds <= 0)
            {
                Debug.Log("Timer ended.");

                // Increase tickets and reset timer
                IncreaseTicketsLocally();
                currentTickets++;

                if (currentTickets < 3)
                {
                    // Reset timer to 30 minutes (or any other desired duration)
                    targetTime = Timestamp.FromDateTime(serverStartTime.ToDateTime().AddMinutes(30));

                    // Restart the timer
                    serverStartTime = Timestamp.FromDateTime(DateTime.Now);
                }
                else
                {
                    // If the player has 3 tickets, don't restart the timer
                    yield break;
                }
            }
            else
            {
                string remainingTimeString;
                if (adjustedRemainingTime.TotalHours >= 1)
                {
                    remainingTimeString = $"{(int)adjustedRemainingTime.TotalHours:D2}:{adjustedRemainingTime.Minutes:D2}:{adjustedRemainingTime.Seconds:D2}";
                }
                else
                {
                    remainingTimeString = $"{adjustedRemainingTime.Minutes:D2}:{adjustedRemainingTime.Seconds:D2}";
                }

                if (timerText != null)
                {
                    timerText.text = remainingTimeString;
                }

                // Decrement serverStartTime by 1 second
                serverStartTime = Timestamp.FromDateTime(serverStartTime.ToDateTime().AddSeconds(1*ticketMultiplier));

                yield return new WaitForSeconds(1);
            }
        }
    }

    public void IncreaseTicketsLocally()
    {
        FirebaseUser user = auth.CurrentUser;
        if (user != null)
        {
            DocumentReference docRef = db.Collection("users").Document(user.UserId).Collection("hints").Document("hintsData");
            docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    DocumentSnapshot snapshot = task.Result;
                    if (snapshot.Exists)
                    {
                        int currentTickets = snapshot.GetValue<int>("tickets");

                        if (currentTickets < 3)
                        {
                            // Update the tickets locally
                            currentTickets += 1;
                            // Reflect the updated tickets in UI or local state
                            Debug.Log("Tickets increased locally.");
                            // Call Cloud Function to validate and finalize the update
                            CallCloudFunctionToUpdateTickets();
                        }
                        else
                        {
                            Debug.LogWarning("Ticket limit reached. Cannot increase tickets.");
                        }
                    }
                    else
                    {
                        Debug.LogError("Document does not exist.");
                    }
                }
                else
                {
                    Debug.LogError("Failed to retrieve document: " + task.Exception);
                }
            });
        }
        else
        {
            Debug.LogError("User is not authenticated.");
        }
    }

    void CallCloudFunctionToUpdateTickets()
    {
        functions.GetHttpsCallable("increaseTickets").
      CallAsync().ContinueWithOnMainThread(task =>
      {
          if (task.IsCompleted)
          {
              Debug.Log("Cloud Function called successfully.");
          }
          else
          {
              Debug.LogError("Failed to call Cloud Function: " + task.Exception);
          }
      });
    }
}