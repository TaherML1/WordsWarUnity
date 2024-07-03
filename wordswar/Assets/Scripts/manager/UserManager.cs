using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Auth;
using System;

public class UserManager : MonoBehaviour
{
    public ShadowPanel shadowPanel;


    public static UserManager Instance { get; private set; }

    private FirebaseFirestore db;
    private FirebaseAuth auth;

    public GameObject setUserPanel;

    private string playerId;
    private Dictionary<string, object> userProfile;
    private Dictionary<string, object> userHints;
    private Dictionary<string, object> hintsPrices;

    public event Action<Dictionary<string, object>> OnUserProfileUpdated; // Event to notify profile updates

    public event Action<Dictionary<string, object>> OnUserHintsUpdated;

   
    public event Action OnInitialUserProfileFetched; // Event to notify initial profile fetch
   
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

    private void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError($"Failed to initialize Firebase with {task.Exception}");
                return;
            }

            db = FirebaseFirestore.DefaultInstance;
            auth = FirebaseAuth.DefaultInstance;

            if (auth.CurrentUser != null)
            {
                playerId = auth.CurrentUser.UserId;
                CheckUserProfileCompletion();
            }
            else
            {
                Debug.LogError("No user is currently logged in.");
            }
        });
    }

    public async void CheckUserProfileCompletion()
    {
        if (auth.CurrentUser != null)
        {
            string userId = auth.CurrentUser.UserId;
            Debug.Log(userId);
            DocumentReference userRef = db.Collection("users").Document(userId);

            DocumentSnapshot snapshot = await userRef.GetSnapshotAsync();
            if (snapshot.Exists)
            {
                bool profileComplete = snapshot.ContainsField("profileComplete") && snapshot.GetValue<bool>("profileComplete");
                if (profileComplete)
                {
                    ListenForUserDataChanges();
                    ListenForUserHintsChanges();
                    Debug.Log("User profile is complete.");
                    OnInitialUserProfileFetched?.Invoke(); // Trigger event when initial profile is fetched
                }
                else
                {
                    Debug.Log("User profile is incomplete.");
                    // Handle incomplete profile scenario
                    if (FetchUserProfile.instance != null)
                    {
                        FetchUserProfile.instance.setUserPanel.SetActive(true);
                        shadowPanel.showShadowPanel();
                    }
                }
            }
            else
            {
                Debug.Log("User document does not exist.");
                // Handle new user scenario
                if (FetchUserProfile.instance != null)
                {
                    FetchUserProfile.instance.setUserPanel.SetActive(true);
                    shadowPanel.showShadowPanel();
                }
            }
        }
        else
        {
            Debug.LogError("No user is currently logged in.");
        }
    }

    public void ListenForUserDataChanges()
    {
        DocumentReference docRef = db.Collection("users").Document(playerId);
        docRef.Listen(snapshot =>
        {
            if (snapshot.Exists)
            {
                userProfile = snapshot.ToDictionary();
                OnUserProfileUpdated?.Invoke(userProfile);
            }
            else
            {
                Debug.Log("User data document does not exist.");
            }
        });
    }
    public void ListenForUserHintsChanges()
    {
        DocumentReference docRef = db.Collection("users").Document(playerId).Collection("hints").Document("hintsData");
        docRef.Listen(snapshot =>
        {
            if (snapshot.Exists) // Changed condition to check if the document exists
            {
                userHints = snapshot.ToDictionary();
                OnUserHintsUpdated?.Invoke(userHints);
            }
        });
    }

    

    public Dictionary<string, object> GetUserProfile()
    {
        return userProfile;
    }

    public Dictionary<string,object> GetUserHints()
    {
        return userHints;
    }

    public int GetCoins()
    {
        return userProfile != null && userProfile.TryGetValue("coins", out object coinsObj) ? Convert.ToInt32(coinsObj) : 0;
    }

    public int GetGems()
    {
        return userProfile != null && userProfile.TryGetValue("gems", out object gemsObj) ? Convert.ToInt32(gemsObj) : 0;
    }

    public int GetJoker()
    {
        return userHints != null && userHints.TryGetValue("joker",out object gameObj) ? Convert.ToInt32(gameObj) : 0;
    }
    public int GetExtraTime()
    {
        return userHints != null && userHints.TryGetValue("extraTime",out object gameObj) ? Convert.ToInt32((string)gameObj) : 0;
    }
    public int GetTickets()
    {
        return userHints != null && userHints.TryGetValue("tickets", out object gameObj) ? Convert.ToInt32((string)gameObj) : 0;
    }

    public void UpdateUserHints(Dictionary<string, object> userHints)
    {
        DocumentReference docRef = db.Collection("users").Document(playerId).Collection("hints").Document("hintsData");
        docRef.SetAsync(userHints).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("User hints updated in Firestore.");
            }
            else
            {
                Debug.LogError("Failed to update user hints in Firestore: " + task.Exception);
            }
        });
    }


}
