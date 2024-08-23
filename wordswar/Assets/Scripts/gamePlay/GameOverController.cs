using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using Firebase.Functions;
using Firebase.Extensions;
using System.Collections.Generic;
using Firebase.Auth;

public class GameOverController : MonoBehaviour
{
    public GameObject gameOverPanel;
    public TextMeshProUGUI winnerText;
    public TextMeshProUGUI gameOverCoinsText;
    public TextMeshProUGUI gameOverXPText;
    public AdManager adManager;
    private int coinsEarned;
    private bool coinsDoubled = false;
    [SerializeField] Button watchadButton;

    private FirebaseAuth auth;
    private FirebaseUser user;

    void Start()
    {
        // Initialize Firebase Auth
        auth = FirebaseAuth.DefaultInstance;
        user = auth.CurrentUser;
    }

    public void AnimateNumber(TextMeshProUGUI targetText, int targetNumber, float duration)
    {
        StartCoroutine(CountToTarget(targetText, targetNumber, duration));
    }

    public void ShowGameOver(bool isWinner)
    {
        winnerText.text = isWinner ? "انت الفائز" : "انت الخاسر";
        coinsEarned = isWinner ? 20 : 10;

        // Activate the game over panel
        gameOverPanel.SetActive(true);

        // Animate the earned coins
        AnimateNumber(gameOverCoinsText, coinsEarned, 0.8f);
    }

    public void ReturnToMainMenu()
    {
        // Load the main menu scene
        if (SceneController.Instance != null)
        {
            SceneController.Instance.LoadMainMenu();
        }
        else
        {
            Debug.LogError("SceneController instance not found. Cannot load MainMenu.");
        }
    }

    public void DoubleCoins()
    {
        Debug.Log("DoubleCoins button clicked.");
        if (!coinsDoubled)
        {
            adManager.ShowRewardedAd(() =>
            {
                coinsEarned *= 2;
                AnimateNumber(gameOverCoinsText, coinsEarned, 0.8f);
                coinsDoubled = true; // Prevent further doubling

                // Call the cloud function to update the user's coins
                IncrementCoinsInCloud(coinsEarned);
            });
        }
        else
        {
            Debug.Log("Coins have already been doubled.");
        }
    }

    private void IncrementCoinsInCloud(int coinsToAdd)
    {
        if (user == null)
        {
            Debug.LogError("User is not authenticated.");
            return;
        }

        // Make sure to set coinsToAdd within the valid range before sending
        coinsToAdd = Mathf.Clamp(coinsToAdd / 2, 10, 20);
        Debug.Log("coins to add : " + coinsToAdd);

        var functions = FirebaseFunctions.DefaultInstance;
        var incrementCoins = functions.GetHttpsCallable("incrementCoinsAD");

        var data = new Dictionary<string, object>
    {
        { "userId", user.UserId }, // Use the actual user ID from Firebase Auth
        { "coinsToAdd", coinsToAdd }
    };

        incrementCoins.CallAsync(data).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log("Coins incremented successfully.");
            }
            else
            {
                Debug.LogError("Failed to increment coins: " + task.Exception);
            }
        });
    }


    private IEnumerator CountToTarget(TextMeshProUGUI targetText, int targetNumber, float duration)
    {
        float elapsed = 0f;
        int startingNumber = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            int currentNumber = Mathf.FloorToInt(Mathf.Lerp(startingNumber, targetNumber, progress));
            targetText.text = $"{currentNumber} x";
            yield return null;
        }

        // Ensure the final value is set
        targetText.text = $"{targetNumber} x";
    }

    public void SetCoinsEarned(int coins)
    {
        coinsEarned = coins;
    }
}
