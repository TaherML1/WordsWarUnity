using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Firestore;

public class firestoretesttt : MonoBehaviour
{
    // Reference to Firestore
    FirebaseFirestore db;

    void Start()
    {
        // Initialize Firestore
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                // Access Firestore instance
                db = FirebaseFirestore.DefaultInstance;
                Debug.Log("Firestore initialized successfully.");
                SetUser("321", "abdo", 233);
            }
            else
            {
                Debug.LogError("Failed to initialize Firestore!");
            }
        });
    }

    public void SetUser(string userId, string username, int score)
    {
        // Create a dictionary to hold user data
        Dictionary<string, object> user = new Dictionary<string, object>
        {
            {"username", username},
            {"score", score}
        };

        // Set user data in Firestore under 'users' collection with userId as document ID
        db.Collection("users").Document(userId).SetAsync(user)
            .ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("User data set successfully!");
                }
                else
                {
                    Debug.LogError("Failed to set user data: " + task.Exception);
                }
            });
    }
}
