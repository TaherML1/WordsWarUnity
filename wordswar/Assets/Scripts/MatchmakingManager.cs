using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Extensions;
using System;
using Firebase.Functions;
using System.Collections;
using UnityEngine.Networking;


public class MatchmakingManager : MonoBehaviour
{
    public static MatchmakingManager instance;
    private const string cloudFunctionUrl = "https://us-central1-finalwordswar2.cloudfunctions.net/getRandomTopic";
 

    public GameObject searchPanel;
    public Button cancelButton;

    private DatabaseReference databaseReference;
    private FirebaseFunctions functions;
    private FirebaseAuth auth;
    private FirebaseUser currentUser;
    private bool isMatchmaking = false;

    private string selectedTopic;



    public string randomTopic;
    public string player2Id { get; private set; }
    public static string CurrentRoomId { get; private set; }

    public static string SelectedTopic { get; private set; }

    void Start()
    { 
        // Initialize Firebase components
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        auth = FirebaseAuth.DefaultInstance;
        currentUser = auth.CurrentUser;
        functions = FirebaseFunctions.DefaultInstance;

        if (currentUser != null)
        {
            // Check if the current user is already in matchmaking
            CheckMatchmakingStatus();

        }
        else
        {
            Debug.LogError("User is not logged in.");
        }
    }

    private void CheckMatchmakingStatus()
    {
        // Check if the current user is already in matchmaking
        databaseReference.Child("matchmaking").Child(currentUser.UserId).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
            {
                DataSnapshot snapshot = task.Result;
                isMatchmaking = snapshot != null && snapshot.Exists;
               
            }
            else
            {
                Debug.LogError("Failed to check matchmaking status: " + task.Exception);
            }
        });
    }

    public void StartMatchmaking()
    {
        if (!isMatchmaking)
        {
            // Add the current user to matchmaking
            databaseReference.Child("matchmaking").Child(currentUser.UserId).SetValueAsync(true).ContinueWith(task =>
            {
                if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
                {
                    Debug.Log("find matchinhgg");
 
                    FindMatch();
                }
                else
                {
                    Debug.LogError("Failed to start matchmaking: " + task.Exception);
                }
            });
        }
        else
        {
            Debug.LogWarning("User is already in matchmaking.");
        }
    }


    public void CancelMatchmaking()
    {
        // Remove the player from the matchmaking pool
        string playerId = auth.CurrentUser.UserId;
        databaseReference.Child("matchmaking").Child(playerId).RemoveValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
            {
                Debug.Log("Player removed from matchmaking");

                // Check if the player has created a room
                databaseReference.Child("gameRooms").Child(playerId).GetValueAsync().ContinueWith(roomTask =>
                {
                    if (roomTask.IsCompleted && !roomTask.IsFaulted && !roomTask.IsCanceled)
                    {
                        DataSnapshot roomSnapshot = roomTask.Result;
                        if (roomSnapshot != null && roomSnapshot.Exists)
                        {
                            // Delete the room
                            databaseReference.Child("gameRooms").Child(playerId).RemoveValueAsync().ContinueWith(roomDeleteTask =>
                            {
                                if (roomDeleteTask.IsCompleted && !roomDeleteTask.IsFaulted && !roomDeleteTask.IsCanceled)
                                {
                                    Debug.Log("Room deleted");
                                }
                                else
                                {
                                    Debug.LogError("Failed to delete room: " + roomDeleteTask.Exception);
                                }
                            });
                        }
                        else
                        {
                            Debug.Log("Player has not created a room.");
                        }
                    }
                    else
                    {
                        Debug.LogError("Failed to retrieve room information: " + roomTask.Exception);
                    }
                });
            }
            else
            {
                Debug.LogError("Failed to remove player from matchmaking: " + task.Exception);
            }
        });
    }


    private void FindMatch()
    {
        Debug.Log("Finding match...");

        // Check if there are available rooms
        databaseReference.Child("gameRooms").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot != null && snapshot.HasChildren)
                {
                    Debug.Log("Available rooms found:");
                    foreach (DataSnapshot roomSnapshot in snapshot.Children)
                    {
                        string roomId = roomSnapshot.Key;
                        int playerCount = (int)roomSnapshot.ChildrenCount;
                        Debug.Log("Room ID: " + roomId + ", Player count: " + playerCount);

                        // Check if the room is not full
                        if (playerCount < 2)
                        {
                            Debug.Log("Room is not full, joining existing room: " + roomId);
                            JoinRoom(roomId);
                            return;
                        }
                    }
                }
                else
                {
                    Debug.Log("No available rooms found, creating new room...");
                }

                // No available rooms found, create a new room
                CreateRoom();
            }
            else
            {
                Debug.LogError("Failed to find match: " + task.Exception);
            }
        });
    }

    private void JoinRoom(string roomId)
    {
        string playerId = currentUser.UserId;
        DatabaseReference roomRef = databaseReference.Child("gameRooms").Child(roomId);
        roomRef.Child("players").Child("player2").SetValueAsync(playerId).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
            {
                player2Id = playerId; // Set the player 2 ID
                Debug.Log("NEW PLAYER2 ID IS : " + player2Id);
                // Store room ID in player's node
                databaseReference.Child("users").Child(playerId).Child("roomId").SetValueAsync(roomId);
                // Listen for changes in the room
                roomRef.ValueChanged += HandleRoomValueChanged;
            }
            else
            {
                Debug.LogError("Failed to join room: " + task.Exception);
            }
        });
    }

    // Add a public method to get the player 2 ID
    public string GetPlayer2Id()
    {
        return player2Id;

    }

    public void CreateRoom()
    {
        // Generate a unique room ID based on player ID
        string playerId = currentUser.UserId;
        DatabaseReference roomRef = databaseReference.Child("gameRooms").Push();
        RoomManager.CurrentRoomId = roomRef.Key; // Store the room ID

        // Create the room with the current player as player1
        roomRef.Child("players").Child("player1").SetValueAsync(playerId).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
            {
                Debug.Log("GameRoom Created: " + roomRef.Key);

                // Store room ID in player's node
                databaseReference.Child("users").Child(playerId).Child("roomId").SetValueAsync(roomRef.Key);

                // Listen for changes in the room
                roomRef.ValueChanged += HandleRoomValueChanged;
            }
            else
            {
                Debug.LogError("Failed to create room: " + task.Exception);
            }
        });
    }



    private void HandleRoomValueChanged(object sender, ValueChangedEventArgs args)
    {
        // Check if both players have joined the room
        if (args.Snapshot.ChildrenCount == 2)
        {
           
            // Both players have joined, transition to the gameplay scene


            // Remove both players from the matchmaking node
            string player1Id = args.Snapshot.Child("players").Child("player1").Value as string;
            string player2Id = args.Snapshot.Child("players").Child("player2").Value as string;

            RemoveFromMatchmaking(player1Id);
            RemoveFromMatchmaking(player2Id);
            //FetchRandomTopic(); 
           
           
            //FetchRandomTopicAndSetInGameRoom(RoomManager.CurrentRoomId);

            // Set the selected topic in the database

            //  SetSelectedTopicInDatabase(RoomManager.CurrentRoomId);

            Invoke("StartGameplayScene", 3f);
        
        }
    }
    private void RemoveFromMatchmaking(string playerId)
    {
        if (!string.IsNullOrEmpty(playerId))
        {
            databaseReference.Child("matchmaking").Child(playerId).RemoveValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
                {
                    Debug.Log("Player " + playerId + " removed from matchmaking");
                }
                else
                {
                    Debug.LogError("Failed to remove player " + playerId + " from matchmaking: " + task.Exception);
                }
            });
        }
    } 
    private void StartGameplayScene()
    {
        // Load the gameplay scene
        SceneManager.LoadScene("GamePlay"); // Replace "GameplayScene" with your actual scene name
    }
  

    private IEnumerator GetRandomTopicCoroutine()
    {

        using (UnityWebRequest webRequest = UnityWebRequest.Get(cloudFunctionUrl))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string randomTopic = webRequest.downloadHandler.text;
                selectedTopicManager.selectedTopic = randomTopic;
             
                Debug.Log("Randomly selected topic: " + randomTopic);
                // Use the randomly selected topic in your game

                SetSelectedTopicInGameRoom(RoomManager.CurrentRoomId, randomTopic);
            }
            else
            {
                Debug.LogError("Failed to fetch random topic: " + webRequest.error);
            }
        }
    }


    private void SetSelectedTopicInGameRoom(string roomId, string selectedTopic)
    {
        if (databaseReference != null)
        {
            DatabaseReference gameRoomRef = databaseReference.Child("gameRooms").Child(roomId).Child("selectedTopic");
            gameRoomRef.SetValueAsync(selectedTopic)
              .ContinueWith(task =>
              {
                  if (task.IsFaulted)
                  {
                      Debug.LogError("Failed to update selected topic in the database: " + task.Exception);
                  }
                  else if (task.IsCompleted)
                  {
                      Debug.Log("Selected topic updated successfully in the database: " + selectedTopic);
                  }
              });
        }
        else
        {
            Debug.LogError("Database reference is not initialized.");
        }
    }
    public void FetchRandomTopic()
    {
        StartCoroutine(GetRandomTopicCoroutine());
    }
}
public static class RoomManager
{
    public static string CurrentRoomId { get; set; }
}
