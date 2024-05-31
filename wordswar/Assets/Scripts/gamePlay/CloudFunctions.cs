using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using Firebase.Functions;
using Firebase.Auth;
using ArabicSupport;
using TMPro;
using System.Threading.Tasks;
using UnityEngine.UI;
using System;
public class CloudFunctions : MonoBehaviour
{
    public static CloudFunctions Instance;
    public GameController GameControllerScript;
    private FirebaseDatabase database;
    private DatabaseReference databaseReference;
    private DatabaseReference turnReference;
    FirebaseAuth auth;
    private FirebaseFunctions functions;


    private string localPlayerId;

    string roomId;

    // Start is called before the first frame update
    void Start()
    {
        roomId = PlayerPrefs.GetString("roomId");
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        database = FirebaseDatabase.DefaultInstance;
        functions = FirebaseFunctions.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;
        localPlayerId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;

    }
    public void newIncrementPlayerScore(string gameId, string playerId)
    {
        DatabaseReference playerScoreRef = databaseReference.Child("games").Child(gameId).Child("gameInfo").Child("scores").Child(playerId);

        playerScoreRef.RunTransaction(mutableData =>
        {
            int currentScore = mutableData.Value != null ? int.Parse(mutableData.Value.ToString()) : 0;
            mutableData.Value = currentScore + 1;
            return TransactionResult.Success(mutableData);
        }).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Failed to increment player score: " + task.Exception);
            }
            else if (task.IsCompleted)
            {
                Debug.Log("Player score incremented successfully.");
            }
        });
    }

    public void newnewSwitchTurn(string gameId, string currentPlayerId)
    {
        // Get the game reference
        DatabaseReference gameRef = databaseReference.Child("games").Child(gameId);

        // Get the current turn from the database
        gameRef.Child("turn").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Failed to read turn from database: " + task.Exception);
                return;
            }

            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                string currentTurn = snapshot.Value.ToString(); // Use .Value instead of .GetValue(true)

                // Check if it's the current player's turn
                if (currentTurn == currentPlayerId)
                {
                    // Get the index of the current player
                    gameRef.Child("gameInfo").Child("playersIds").GetValueAsync().ContinueWith(playersTask =>
                    {
                        if (playersTask.IsFaulted)
                        {
                            Debug.LogError("Failed to read playersIds from database: " + playersTask.Exception);
                            return;
                        }

                        if (playersTask.IsCompleted)
                        {
                            DataSnapshot playersSnapshot = playersTask.Result;
                            var playerIds = playersSnapshot.Value as List<object>;
                            if (playerIds != null)
                            {
                                int currentPlayerIndex = playerIds.IndexOf(currentPlayerId);
                                // Calculate the index of the next player
                                int nextPlayerIndex = (currentPlayerIndex + 1) % playerIds.Count;

                                // Get the next player's ID
                                string nextPlayerId = playerIds[nextPlayerIndex].ToString();

                                // Update the turn in the database
                                gameRef.Child("turn").SetValueAsync(nextPlayerId);
                            }
                            else
                            {
                                Debug.LogError("Failed to parse playerIds from database.");
                            }
                        }
                    });
                }
                else
                {
                    Debug.LogWarning("It's not your turn to switch.");
                }
            }
        });
    }
}


