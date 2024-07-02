using Firebase.Functions;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Extensions;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using TMPro;
using Firebase.Analytics;

public class PurchaseItem : MonoBehaviour
{



    private UserManager userManager;
    private HintPricesManager hintPricesManager;
    private FirebaseFunctions functions;

    public FeedbackManager feedbackManager;

    public RadialProgressBar radialProgressBar;

    int playerCoins;
    int playerGems;
    int jokerPrice;
    int extraTimePrice;
    int ticketsPrice;

    private async void Start()
    {
        await FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError($"Firebase Initialization Error: {task.Exception}");
                return;
            }
            FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
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
            FirebaseAnalytics.LogEvent("sufficient_funds", new Parameter("hint_type", "joker"), new Parameter("player_coins", playerCoins), new Parameter("required_coins", jokerPrice));
        }
        else
        {
            feedbackManager.ShowFeedback("Not enough coins to purchase a joker.");
            Debug.Log("Not enough coins to purchase a joker.");
            FirebaseAnalytics.LogEvent("insufficient_funds", new Parameter("hint_type", "joker"), new Parameter("player_coins", playerCoins), new Parameter("required_coins", jokerPrice));

        }
    }

    public void OnClickPurchaseExtraTimes()
    {
        if (playerCoins >= extraTimePrice)
        {
            FirebaseAnalytics.LogEvent("sufficient_funds", new Parameter("hint_type", "extraTime"), new Parameter("player_coins", playerCoins), new Parameter("required_coins", extraTimePrice));
            PurchaseHint("PtgEJZEUyS1zezDD4k0g", "extraTime");
        }
        else
        {
            feedbackManager.ShowFeedback("Not enough coins to purchase extra time.");
            Debug.Log("Not enough coins to purchase extra time.");
            FirebaseAnalytics.LogEvent("insufficient_funds", new Parameter("hint_type", "extraTime"), new Parameter("player_coins", playerCoins), new Parameter("required_coins", extraTimePrice));
        }
    }
    public void OnclickPurchaseTickets()
    {
        if(playerCoins >= ticketsPrice)
        {
            FirebaseAnalytics.LogEvent("sufficient_funds", new Parameter("hint_type", "extraTime"), new Parameter("player_coins", playerCoins), new Parameter("required_coins", extraTimePrice));
            PurchaseHint("tickets4141", "tickets");
        }else
        {
            feedbackManager.ShowFeedback("Not enough coins to purchase tickets.");
            Debug.Log("Not enough coins to purchase tickets.");
            FirebaseAnalytics.LogEvent("insufficient_funds", new Parameter("hint_type", "tickets"), new Parameter("player_coins", playerCoins), new Parameter("required_coins", ticketsPrice));
        }
    }


    public void PurchaseHint(string hintId, string hintType)
    {
        
        radialProgressBar.StartSpinning();
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
                    radialProgressBar.StopSpinning();
                }
                else if (task.IsCompleted)
                {
                    Debug.Log("The purchase was made successfully.");
                    radialProgressBar.StopSpinning();
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
        ticketsPrice = hintPricesManager.getTicketsPrice();

        Debug.Log($"Joker price: {jokerPrice}, Extra time price: {extraTimePrice}");
    }
}
