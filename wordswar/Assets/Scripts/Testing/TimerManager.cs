using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using System;
using UnityEngine;
using UnityEngine.UI;
using Firebase;

public class TimerManager : MonoBehaviour
{
    FirebaseFirestore db;
    FirebaseAuth auth;
    [SerializeField] Text timerText;
    [SerializeField] GameObject Panel;

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
                Debug.Log("Firebase dependencies are available.");

                FirebaseUser user = auth.CurrentUser;
                if (user != null)
                {
                    Debug.Log($"User is authenticated: {user.UserId}");
                    RetrieveServerTime(user.UserId);
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
                    Debug.Log("server time : " + serverStartTime);
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
                    Debug.Log($"Timer set for: {targetTime.ToDateTime()}");
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

    System.Collections.IEnumerator StartCountdown()
    {
        while (true)
        {
            TimeSpan remainingTime = targetTime.ToDateTime() - serverStartTime.ToDateTime();

            Debug.Log($"Remaining time: {remainingTime}");

            if (remainingTime.TotalSeconds <= 0)
            {
                Debug.Log("Timer ended.");
                IncreaseTickets(); // Call function to increase tickets
                if (timerText != null)
                {
                    Panel.SetActive(false);
                    timerText.text = "Timer ended.";
                }
                yield break;
            }

            string remainingTimeString;
            if (remainingTime.TotalHours >= 1)
            {
                remainingTimeString = $"{(int)remainingTime.TotalHours:D2}:{remainingTime.Minutes:D2}:{remainingTime.Seconds:D2}";
            }
            else
            {
                remainingTimeString = $"{remainingTime.Minutes:D2}:{remainingTime.Seconds:D2}";
            }

            if (timerText != null)
            {
                timerText.text = remainingTimeString;
            }

            Debug.Log("Time remaining: " + remainingTimeString);

            // Update serverStartTime
            serverStartTime = Timestamp.FromDateTime(serverStartTime.ToDateTime().AddSeconds(1));

            yield return new WaitForSeconds(1);
        }
    }

    void IncreaseTickets()
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
                            docRef.UpdateAsync("tickets", FieldValue.Increment(1)).ContinueWithOnMainThread(updateTask =>
                            {
                                if (updateTask.IsCompleted)
                                {
                                    Debug.Log("Tickets increased successfully.");
                                }
                                else if (updateTask.IsFaulted)
                                {
                                    Debug.LogError("Failed to increase tickets: " + updateTask.Exception);
                                }
                            });
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
}
