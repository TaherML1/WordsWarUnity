using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Functions;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using Firebase;

public class TimerManager : MonoBehaviour
{
    public static TimerManager instance;
    FirebaseFirestore db;
    FirebaseAuth auth;
    FirebaseFunctions functions;
    [SerializeField] Text timerText;
    [SerializeField] Text ticketsText;
    [SerializeField] GameObject Panel;

    int currentTickets;
    int refreshedTickets;

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
                if (user != null)
                {
                    RetrieveServerTime(user.UserId);
                    UserManager.Instance.OnUserHintsUpdated += UpdateUserTicketsAndRefreshedTickets;
                    UserManager.Instance.CheckUserProfileCompletion();
                }
                else
                {
                    Debug.LogError("User is not authenticated.");
                }
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

    void UpdateUserTicketsAndRefreshedTickets(Dictionary<string, object> userHints)
    {
        if (userHints.TryGetValue("tickets", out object ticketsObj))
        {
            currentTickets = Convert.ToInt32(ticketsObj);
        }
        else
        {
            Debug.LogError("tickets key is missing in hintsData");
        }

        if (userHints.TryGetValue("refreshedTickets", out object refreshedTicketsObj))
        {
            refreshedTickets = Convert.ToInt32(refreshedTicketsObj);
        }
        else
        {
            Debug.LogError("refreshedTickets key is missing in hintsData");
        }

        // Update UI to display tickets and refreshedTickets
        if (ticketsText != null)
        {
            ticketsText.text = $"Tickets: {currentTickets} / Refreshed: {refreshedTickets}";
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
            if (refreshedTickets >= 3)
            {
                Debug.Log("You already have 3 refreshed tickets. No need to wait.");
                yield break;
            }

            TimeSpan remainingTime = targetTime.ToDateTime() - serverStartTime.ToDateTime();

            // Adjust remaining time based on the number of refreshed tickets
            float ticketMultiplier = 1f;
            switch (refreshedTickets)
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

                CallCloudFunctionToUpdatefreshedTickets();

                refreshedTickets++;

                if (refreshedTickets < 3)
                {
                    // Reset timer for the next interval
                    targetTime = Timestamp.FromDateTime(serverStartTime.ToDateTime().AddMinutes(30));
                    serverStartTime = Timestamp.FromDateTime(DateTime.Now);
                }
                else
                {
                    yield break;
                }
            }
            else
            {
                string remainingTimeString = adjustedRemainingTime.TotalHours >= 1
                    ? $"{(int)adjustedRemainingTime.TotalHours:D2}:{adjustedRemainingTime.Minutes:D2}:{adjustedRemainingTime.Seconds:D2}"
                    : $"{adjustedRemainingTime.Minutes:D2}:{adjustedRemainingTime.Seconds:D2}";

                if (timerText != null)
                {
                    timerText.text = remainingTimeString;
                }

                serverStartTime = Timestamp.FromDateTime(serverStartTime.ToDateTime().AddSeconds(1 * ticketMultiplier));
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
                        int refreshedTickets = snapshot.GetValue<int>("refreshedTickets");

                        if (refreshedTickets < 3)
                        {
                            refreshedTickets += 1;
                            Debug.Log("Refreshed tickets increased locally.");
                            CallCloudFunctionToUpdateTickets();
                        }
                        else
                        {
                            Debug.LogWarning("Refreshed ticket limit reached. Cannot increase refreshed tickets.");
                        }

                        // Update the tickets locally
                        if (currentTickets < 3)
                        {
                            currentTickets += 1;
                            Debug.Log("Tickets increased locally.");
                        }

                        // Update UI
                        if (ticketsText != null)
                        {
                            ticketsText.text = $"Tickets: {currentTickets} / Refreshed: {refreshedTickets}";
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
        functions.GetHttpsCallable("increaseTickets").CallAsync().ContinueWithOnMainThread(task =>
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

    void CallCloudFunctionToUpdatefreshedTickets()
    {
        functions.GetHttpsCallable("increaserefreshedTickets").CallAsync().ContinueWithOnMainThread(task =>
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
