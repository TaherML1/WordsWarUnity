using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Functions;
using Firebase.Auth;

public class TimerManager : MonoBehaviour
{
    public Text timerText;
    private FirebaseFirestore db;
    private FirebaseFunctions functions;
    private FirebaseAuth auth;
    private DateTime endTime;

    void Start()
    {
        if (FirebaseManager.Instance != null && FirebaseManager.Instance.IsFirebaseInitialized)
        {
            Debug.Log("Firebase is initialized.");
            InitializeFirebaseComponents();
        }
        else
        {
            Debug.Log("Waiting for Firebase initialization...");
            // Wait until Firebase is initialized
            StartCoroutine(WaitForFirebaseInitialization());
        }
    }

    private void InitializeFirebaseComponents()
    {
        db = FirebaseFirestore.DefaultInstance;
        functions = FirebaseFunctions.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;
        Debug.Log("Firebase components initialized.");

        // Uncomment if you want to call the function first
         callFunction();

        //FetchEndTime();
    }

    private IEnumerator WaitForFirebaseInitialization()
    {
        // Wait until Firebase is initialized
        while (!FirebaseManager.Instance.IsFirebaseInitialized)
        {
            yield return null;
        }

        Debug.Log("Firebase is now initialized.");
        // Firebase is now initialized, initialize Firebase components
        InitializeFirebaseComponents();
    }

    public void callFunction()
    {
        if (functions != null)
        {
            functions.GetHttpsCallable("setTimer").CallAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
                {
                    var result = task.Result.Data.ToString();
                    Debug.Log("Function call success: " + result);
                }
                else
                {
                    Debug.LogError("Function call failed: " + task.Exception);
                }
            });
        }
        else
        {
            Debug.LogError("Firebase functions instance is null.");
        }
    }

    void FetchEndTime()
    {
        Debug.Log("Fetching end time from Firestore...");

        db.Collection("users").Document(auth.CurrentUser.UserId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    Debug.Log("Document snapshot exists.");
                    if (snapshot.TryGetValue("endTime", out Timestamp endTimeTimestamp))
                    {
                        endTime = endTimeTimestamp.ToDateTime();
                        Debug.Log("End time fetched successfully: " + endTime);
                        StartCoroutine(UpdateTimer());
                    }
                    else
                    {
                        Debug.LogError("endTime field not found in the document.");
                    }
                }
                else
                {
                    Debug.LogError("Document does not exist.");
                }
            }
            else if (task.IsFaulted)
            {
                Debug.LogError("Failed to fetch document snapshot: " + task.Exception);
            }
            else if (task.IsCanceled)
            {
                Debug.LogError("Fetch document snapshot task was canceled.");
            }
            else
            {
                Debug.LogError("Unexpected error in fetching document snapshot.");
            }
        });
    }


    IEnumerator UpdateTimer()
    {
        while (true)
        {
            TimeSpan remainingTime = endTime - DateTime.Now;
            if (remainingTime.TotalSeconds > 0)
            {
                timerText.text = string.Format("{0:D2}:{1:D2}:{2:D2}", remainingTime.Hours, remainingTime.Minutes, remainingTime.Seconds);
                Debug.Log("Remaining time: " + timerText.text);
            }
            else
            {
                timerText.text = "Time's up!";
                Debug.Log("Time's up!");
                break;
            }
            yield return new WaitForSeconds(1);
        }
    }
}
