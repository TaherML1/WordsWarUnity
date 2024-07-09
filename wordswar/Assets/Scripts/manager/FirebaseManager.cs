using UnityEngine;
using Firebase;
using Firebase.Extensions;
using Firebase.Analytics;
using System;
using Firebase.Auth;

public class FirebaseManager : MonoBehaviour

    
{
    FirebaseAuth auth;

    public static FirebaseManager Instance { get; private set; }
    public bool IsFirebaseInitialized { get; private set; }

    public event Action OnFirebaseInitialized; // Event to notify when Firebase is initialized

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeFirebase();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                IsFirebaseInitialized = true;
                Debug.Log("Firebase is initialized successfully.");
                OnFirebaseInitialized?.Invoke(); // Trigger the event
                 auth = FirebaseAuth.DefaultInstance;
                Debug.Log("current user id is  : " + auth.CurrentUser.UserId);
            }
            else
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {task.Result}");
                IsFirebaseInitialized = false;
            }
        });
    }
}
