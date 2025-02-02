using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using TMPro;
using System;

public class HintPricesManager : MonoBehaviour
{
    public static HintPricesManager Instance { get; private set; }
    private FirebaseFirestore db;
    private string id = "UT2nYZ0MYCzc99PkX8BR"; // Document ID in Firestore
    public TextMeshProUGUI jokerText;
    public TextMeshProUGUI extraTimeText;
    public TextMeshProUGUI ticketsText;
    public TextMeshProUGUI TicketsTextPrice;
    public TextMeshProUGUI threeOfTicketsText;

    private int joker;
    private int extraTime;
    private int tickets;
    private int threeOfTickets;
    public static event Action PricesFetched;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroy duplicate instances
            return;
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);

        if (FirebaseManager.Instance != null && FirebaseManager.Instance.IsFirebaseInitialized)
        {
            InitializeFirebaseComponents();
        }
        else
        {
            StartCoroutine(WaitForFirebaseInitialization());
        }
    }

    private void InitializeFirebaseComponents()
    {
        db = FirebaseFirestore.DefaultInstance;
        RetrieveHintPrices();
    }

    private IEnumerator WaitForFirebaseInitialization()
    {
        while (FirebaseManager.Instance == null || !FirebaseManager.Instance.IsFirebaseInitialized)
        {
            yield return null;
        }
        InitializeFirebaseComponents();
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
                tickets = int.Parse(hintData["tickets"].ToString());
                threeOfTickets = int.Parse(hintData["3tickets"].ToString());

                extraTimeText.text = extraTime.ToString();
                jokerText.text = joker.ToString();
                ticketsText.text = tickets.ToString();
                TicketsTextPrice.text = tickets.ToString();
                threeOfTicketsText.text = threeOfTickets.ToString();

                PricesFetched?.Invoke();

                Debug.Log("Joker price: " + joker);
                Debug.Log("Extra time price: " + extraTime);
                Debug.Log("Tickets price : " + tickets);
                Debug.Log("3 tickets price is : " + threeOfTickets);
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

    public int getTicketsPrice()
    {
        return tickets;
    }

    public int getGroupOfTickets()
    {
        return threeOfTickets;
    }
}
