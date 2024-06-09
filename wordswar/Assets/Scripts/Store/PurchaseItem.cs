using Firebase.Functions;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Extensions;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using TMPro;

public class PurchaseItem : MonoBehaviour
{
    private UserManager userManager;
    private HintPricesManager hintPricesManager;
    private FirebaseFunctions functions;

    public FeedbackManager feedbackManager;

    int playerCoins;
    int playerGems;
    int jokerPrice;
    int extraTimePrice;

    private async void Start()
    {
        await FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError($"Firebase Initialization Error: {task.Exception}");
                return;
            }

            functions = FirebaseFunctions.DefaultInstance;
            userManager = UserManager.Instance;

            if (userManager == null)
            {
                Debug.LogError("UserManager is not initialized.");
                return;
            }

            // Subscribe to the balance change event
            userManager.OnUserProfileUpdated += OnUserProfileUpdated;
            getBalance();

            hintPricesManager = HintPricesManager.Instance;
            if (hintPricesManager == null)
            {
                Debug.LogError("HintPricesManager is not initialized.");
                return;
            }

            // Subscribe to the PricesFetched event
            HintPricesManager.PricesFetched += OnPricesFetched;
        });
    }

    private void OnUserProfileUpdated(Dictionary<string, object> userProfile)
    {
        playerCoins = userManager.GetCoins();
        playerGems = userManager.GetGems();
        Debug.Log($"Updated Player coins: {playerCoins}, Player gems: {playerGems}");
    }

    private void OnPricesFetched()
    {
        getPrices();
    }

    private void OnDestroy()
    {
        if (userManager != null)
        {
            userManager.OnUserProfileUpdated -= OnUserProfileUpdated;
        }
        HintPricesManager.PricesFetched -= OnPricesFetched;
    }

    public void OnClickPurchaseJoker()
    {
        if (playerCoins >= jokerPrice)
        {
            PurchaseHint("fMjt0FLcHYNp06mmHnFI", "joker");
        }
        else
        {
            feedbackManager.ShowFeedback("Not enough coins to purchase a joker.");
            Debug.Log("Not enough coins to purchase a joker.");
        }
    }

    public void OnClickPurchaseExtraTimes()
    {
        if (playerCoins >= extraTimePrice)
        {
            PurchaseHint("PtgEJZEUyS1zezDD4k0g", "extraTime");
        }
        else
        {
            feedbackManager.ShowFeedback("Not enough coins to purchase extra time.");
            Debug.Log("Not enough coins to purchase extra time.");
        }
    }

    public void PurchaseHint(string hintId, string hintType)
    {
        Debug.Log("You called the function.");

        // Create the data payload to send to the Cloud Function
        Dictionary<string, object> data = new Dictionary<string, object>
        {
            { "hintId", hintId },
            { "hintType", hintType }
        };

        // Call the Cloud Function
        functions.GetHttpsCallable("purchaseHint2")
            .CallAsync(data)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("Purchase failed: you don't have enough coins.");
                }
                else if (task.IsCompleted)
                {
                    Debug.Log("The purchase was made successfully.");
                }
            });
    }

    private void getBalance()
    {
        playerCoins = userManager.GetCoins();
        playerGems = userManager.GetGems();
        Debug.Log($"Initial Player coins: {playerCoins}, Player gems: {playerGems}");
    }

    private void getPrices()
    {
        jokerPrice = hintPricesManager.getJokerPrice();
        extraTimePrice = hintPricesManager.getExtraTimePrice();

        Debug.Log($"Joker price: {jokerPrice}, Extra time price: {extraTimePrice}");
    }
}
