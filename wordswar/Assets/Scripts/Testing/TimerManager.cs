using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.UI;
using System;

public class TimerManager : MonoBehaviour
{
    FirebaseFirestore db;
    FirebaseAuth auth;
    [SerializeField] Text timerText;
    [SerializeField] GameObject Panel;


    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                db = FirebaseFirestore.DefaultInstance;

                FirebaseUser user = auth.CurrentUser;
                if (user != null)
                {
                    RetrieveTimer(user.UserId);
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

    void RetrieveTimer(string userId)
    {
        DocumentReference docRef = db.Collection("users").Document(userId);
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    Timestamp timerTimestamp = snapshot.GetValue<Timestamp>("refreshTime");
                    DateTime timerDateTimeUtc = timerTimestamp.ToDateTime();
                    DateTime timerDateTimeLocal = timerDateTimeUtc.ToLocalTime();
                    Debug.Log($"Timer set for (local time): {timerDateTimeLocal}");
                    StartCoroutine(StartCountdown(timerDateTimeLocal));
                }
                else
                {
                    Debug.Log("No such document.");
                }
            }
            else
            {
                Debug.LogError("Error getting document: " + task.Exception);
            }
        });
    }

    System.Collections.IEnumerator StartCountdown(DateTime targetTime)
    {
        while (true)
        {
            TimeSpan remainingTime = targetTime - DateTime.Now;
            Debug.Log($"Current time: {DateTime.Now}, Target time: {targetTime}, Remaining time: {remainingTime}");

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

            if (timerText != null)
            {
                timerText.text = remainingTime.ToString(@"hh\:mm\:ss");
            }

            Debug.Log("Time remaining: " + remainingTime.ToString(@"hh\:mm\:ss"));
            yield return new WaitForSeconds(1);
        }
    }

    void IncreaseTickets()
    {
        FirebaseUser user = auth.CurrentUser;
        if (user != null)
        {
            DocumentReference docRef = db.Collection("users").Document(user.UserId).Collection("hints").Document("hintsData");
            docRef.UpdateAsync("tickets", FieldValue.Increment(1)).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("Tickets increased successfully.");
                }
                else if (task.IsFaulted)
                {
                    Debug.LogError("Failed to increase tickets: " + task.Exception);
                }
            });
        }
        else
        {
            Debug.LogError("User is not authenticated.");
        }
    }
}
