using UnityEngine;
using Firebase;
using Firebase.Extensions;
using Firebase.Analytics;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }
    public bool IsFirebaseInitialized { get; private set; }

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
                // You can call any further initialization methods here if needed
            }
            else
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {task.Result}");
                IsFirebaseInitialized = false;
            }
        });
    }
}
