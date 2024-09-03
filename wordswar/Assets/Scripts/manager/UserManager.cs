using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Auth;
using System;
using System.Collections;

public class UserManager : MonoBehaviour
{
  
    public static UserManager Instance { get; private set; }

    private FirebaseFirestore db;
    private FirebaseAuth auth;

    public GameObject setUserPanel;

    private string playerId;
    private Dictionary<string, object> userProfile;
    private Dictionary<string, object> userHints;

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
        // Check if Firebase is initialized
        if (FirebaseManager.Instance != null && FirebaseManager.Instance.IsFirebaseInitialized)
        {
            InitializeFirebaseComponents();
        }
        else
        {
            // Wait until Firebase is initialized
            StartCoroutine(WaitForFirebaseInitialization());
        }
    }

    private void InitializeFirebaseComponents()
    {
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
    }

    private IEnumerator WaitForFirebaseInitialization()
    {
        // Wait until Firebase is initialized
        while (!FirebaseManager.Instance.IsFirebaseInitialized)
        {
            yield return null;
        }

        // Firebase is now initialized, initialize Firebase components
        InitializeFirebaseComponents();
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
                if (snapshot.TryGetValue("profileComplete", out bool profileComplete) && profileComplete)
                {
                    ListenForUserDataChanges();
                    ListenForUserHintsChanges();
                    Debug.Log("User profile is complete.");
                    OnInitialUserProfileFetched?.Invoke(); // Trigger event when initial profile is fetched
                }
                else
                {
                    Debug.Log("User profile is incomplete.");
                    ShowSetUserPanel();
                }
            }
            else
            {
                Debug.Log("User document does not exist.");
                ShowSetUserPanel();
            }
        }
        else
        {
            Debug.LogError("No user is currently logged in.");
        }
    }

    private void ShowSetUserPanel()
    {
        if (FetchUserProfile.instance != null)
        {
            FetchUserProfile.instance.setUserPanel.SetActive(true);
     
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
            if (snapshot.Exists)
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

    public Dictionary<string, object> GetUserHints()
    {
        return userHints;
    }

    public int GetCoins()
    {
        return userProfile != null && userProfile.TryGetValue("coins", out object coinsObj) ? Convert.ToInt32(coinsObj) : 0;
    }

    

    public int GetJoker()
    {
        return userHints != null && userHints.TryGetValue("joker", out object gameObj) ? Convert.ToInt32(gameObj) : 0;
    }

    public int GetExtraTime()
    {
        return userHints != null && userHints.TryGetValue("extraTime", out object gameObj) ? Convert.ToInt32((string)gameObj) : 0;
    }

    public int GetTickets()
    {
        return userHints != null && userHints.TryGetValue("tickets", out object gameObj) ? Convert.ToInt32((string)gameObj) : 0;
    }
    public int GetSpinTickets()
    {
        return userProfile != null && userProfile.TryGetValue("spinTicket", out object spinTicketObj) ? Convert.ToInt32(spinTicketObj) : 0;
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
