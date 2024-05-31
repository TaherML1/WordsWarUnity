using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;

public class Testtt : MonoBehaviour
{
    // Firebase variables
    FirebaseAuth auth;
    DatabaseReference databaseRef;

    // Current user ID
    string currentUserID;
    string localPlayerName;
    string roomId;

    void Start()
    {
        roomId = PlayerPrefs.GetString("roomId");
        InitializeFirebase();
    }

    void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError($"Firebase initialization failed: {task.Exception}");
                return;
            }

            auth = FirebaseAuth.DefaultInstance;
            databaseRef = FirebaseDatabase.DefaultInstance.RootReference;

            if (auth.CurrentUser != null)
            {
                currentUserID = auth.CurrentUser.UserId;
                localPlayerName = auth.CurrentUser.DisplayName;
                Debug.Log("Local player's name: " + localPlayerName);
            }

            FetchOtherPlayerName();
        });
    }

    void FetchOtherPlayerName()
    {
        Debug.Log("ROOM ID IS :" + roomId);
        DatabaseReference gameInfoRef = databaseRef.Child("games").Child(roomId).Child("gameInfo");

        FetchCurrentPlayerId(gameInfoRef);
    }

    void FetchCurrentPlayerId(DatabaseReference gameInfoRef)
    {
        gameInfoRef.Child("turn").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"Failed to fetch current player ID: {task.Exception}");
                return;
            }

            string currentPlayerId = task.Result.GetValue(true).ToString();
            FetchOtherPlayerId(gameInfoRef, currentPlayerId);
        });
    }

    void FetchOtherPlayerId(DatabaseReference gameInfoRef, string currentPlayerId)
    {
        gameInfoRef.Child("playersIds").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"Failed to fetch player IDs: {task.Exception}");
                return;
            }

            DataSnapshot playersIdsSnapshot = task.Result;
            string otherPlayerId = GetOtherPlayerId(playersIdsSnapshot, currentPlayerId);

            if (!string.IsNullOrEmpty(otherPlayerId))
            {
                FetchOtherPlayerName(otherPlayerId);
            }
        });
    }

    string GetOtherPlayerId(DataSnapshot playersIdsSnapshot, string currentPlayerId)
    {
        foreach (DataSnapshot playerIDSnapshot in playersIdsSnapshot.Children)
        {
            string playerID = playerIDSnapshot.GetValue(true).ToString();
            if (playerID != currentPlayerId)
            {
                return playerID;
            }
        }

        return null;
    }

    void FetchOtherPlayerName(string otherPlayerId)
    {
        DatabaseReference otherPlayerRef = databaseRef.Child("users").Child(otherPlayerId);

        otherPlayerRef.Child("displayName").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"Failed to fetch other player's name: {task.Exception}");
                return;
            }

            string otherPlayerName = task.Result.GetValue(true).ToString();
            Debug.Log("Other player's name: " + otherPlayerName);
        });
    }
}