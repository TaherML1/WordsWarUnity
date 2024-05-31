using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Auth;
using TMPro;
using UnityEngine.UI;
using System;
using Firebase.Functions;
using UnityEngine.SceneManagement;
using Unity.Mathematics;

public class UserValues : MonoBehaviour
{
    public static UserValues Instance { get; private set; }

    private FirebaseFirestore db;
    private FirebaseFunctions functions;
    private FirebaseAuth auth;
    

    private string playerId;
    private string username;
    private int coins;
    private int gems;
    private int xp;
    private int level;
    private int jokerHint;
    private int extraTimeHint;


    private int currentXP;
    private int requiredXPForNextLevel;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError($"Failed to initialize Firebase with {task.Exception}");
                return;
            }

            db = FirebaseFirestore.DefaultInstance;
            playerId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
            functions = FirebaseFunctions.DefaultInstance;
            auth = FirebaseAuth.DefaultInstance;

            Debug.Log("fetching");
            CheckUserProfileCompletion();
            // Call fetchUserData after Firebase initialization
            fetchUserData();
            fetchHints();
        });
    }

    async void fetchUserData()
    {
        try
        {
            DocumentReference docRef = db.Collection("users").Document(playerId);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
            if (snapshot.Exists)
            {
                Dictionary<string, object> userProfile = snapshot.ToDictionary();
                username = userProfile["username"].ToString();
                coins = int.Parse(userProfile["coins"].ToString());
                gems = int.Parse(userProfile["gems"].ToString());
             
                xp = int.Parse(userProfile["xp"].ToString());
                level = int.Parse(userProfile["level"].ToString());
                Debug.Log("xp : " + xp);
                Debug.Log("username : " + username);

                // Call the UpdateProgressBar method of XPProgressBar
              
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to fetch user data: " + ex.Message);
        }
    }
    public int GetPlayerCoins()
    {
        return coins;
    }

    public int GetPlayerGems()
    {
        return gems;
    }
    public int GetJokerHint()
    {
        return jokerHint;
    }
    async void fetchHints()
    {
        DocumentReference hintRef = db.Collection("users").Document(playerId).Collection("hints").Document("hintsData");
        DocumentSnapshot snapshot = await hintRef.GetSnapshotAsync();
        if (snapshot.Exists)
        {
            Dictionary<string, object> userHints = snapshot.ToDictionary();
            jokerHint = int.Parse(userHints["joker"].ToString());
            extraTimeHint = int.Parse(userHints["extraTime"].ToString());
        


        }
    }
    private async void CheckUserProfileCompletion()
    {
        if (auth.CurrentUser != null)
        {
            string userId = auth.CurrentUser.UserId;
            Debug.Log(userId);
            DocumentReference userRef = db.Collection("users").Document(userId);

            DocumentSnapshot snapshot = await userRef.GetSnapshotAsync();
            if (snapshot.Exists)
            {
                // Access the value of the 'profileComplete' field directly from the snapshot's data
                bool profileComplete = snapshot.ContainsField("profileComplete") && snapshot.GetValue<bool>("profileComplete");
                if (profileComplete)
                {
                    Debug.Log("User profile is complete.");
                    // Proceed with your app logic for logged-in users with complete profiles
                }
                else
                {
                    Debug.Log("User profile is incomplete.");
                    // Redirect user to complete profile or perform necessary actions
                }
            }
            else
            {
                Debug.Log("User document does not exist.");
                // User profile is incomplete, load user profile scene
                SceneManager.LoadScene("UserProfile");

            }
        }
        else
        {
            Debug.LogError("No user is currently logged in.");
        }
    }

 

}
