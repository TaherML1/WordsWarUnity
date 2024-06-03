using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using TMPro;
using System;

public class HintPricesManager : MonoBehaviour
{
    public static HintPricesManager Instance { get; private set; }
    private FirebaseFirestore db;
    private string id = "UT2nYZ0MYCzc99PkX8BR";
    public TextMeshProUGUI jokerText;
    public TextMeshProUGUI extraTimeText;
    private int joker;
    private int extraTime;
    public static event Action PricesFetched;

    private void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroy duplicate instances
            return;
        }
        Instance = this;

        // Check if there's a parent GameObject
        if (transform.parent == null)
        {
            GameObject parentObject = new GameObject("HintPricesManagerParent");
            transform.SetParent(parentObject.transform);
            DontDestroyOnLoad(parentObject);
        }

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                db = FirebaseFirestore.DefaultInstance;
                RetrieveHintPrices();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    private async void RetrieveHintPrices()
    {
        try
        {
            Debug.Log("Retrieving hint prices...");
            DocumentReference hintPricesRef = db.Collection("hintPrices").Document(id);
            DocumentSnapshot snapshot = await hintPricesRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                Dictionary<string, object> hintData = snapshot.ToDictionary();
                joker = int.Parse(hintData["joker"].ToString());
                extraTime = int.Parse(hintData["extraTime"].ToString());

                // Perform UI updates on the main thread
                extraTimeText.text = extraTime.ToString();
                jokerText.text = joker.ToString();
                PricesFetched?.Invoke();
                Debug.Log("Joker price: " + joker);
                Debug.Log("Extra time price: " + extraTime);
            }
            else
            {
                Debug.Log("Hint prices snapshot does not exist.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to fetch hint prices: " + ex.Message);
        }
    }

    public int getJokerPrice()
    {
        return joker;
    }

    public int getExtraTimePrice()
    {
        return extraTime;
    }
}
