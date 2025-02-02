﻿using Firebase.Functions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Extensions; 
using Firebase.Database;

public class SpecialMatchmakingManager : MonoBehaviour
{

    [SerializeField] FeedbackManager feedbackManager;
    [SerializeField] GameObject RoomCreatedPanel;
    [SerializeField] RadialProgressBar radialProgressBar;
    


    FirebaseFunctions functions;
    DatabaseReference databaseReference;
    public TMP_Text roomIdText;
    public TMP_InputField roomIdInputField;
    public Button joinRoomButton;
    public Button copyButton;

    private string RoomId;

    void Start()
    {
        Debug.Log("Initializing Firebase...");
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                Debug.Log("Firebase dependencies are available.");
                functions = FirebaseFunctions.DefaultInstance;
                databaseReference = FirebaseDatabase.DefaultInstance.RootReference;

                joinRoomButton.onClick.AddListener(OnJoinRoomButtonClicked);
                Debug.Log("Initialization complete.");
            }
            else
            {
                Debug.LogError("Failed to initialize Firebase: " + task.Result);
            }
        });
    }

    public void CreateSpecialRoom()
    {
        radialProgressBar.StartSpinning();
        if (functions != null)
        {
            functions.GetHttpsCallable("createSpecialRoom2").CallAsync().ContinueWithOnMainThread(task =>
            {
                
                if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
                {
                    radialProgressBar.StopSpinning();
                    var result = (string)task.Result.Data;
                    RoomId = result;

                    Debug.Log("Special room created with ID: " + RoomId);
                    RoomCreatedPanel.SetActive(true);
                    DisplayRoomId(RoomId);
                }
                else
                {
                    radialProgressBar.StopSpinning();
                    Debug.LogError("Error creating special room: " + task.Exception);
                }
            });
        }
        else
        {
            Debug.LogError("Firebase functions instance is null.");
        }
    }

    public void deleteRoom()
    {
        if (databaseReference != null)
        {
            Debug.Log("the room id is to delete : " + RoomId);
            DatabaseReference specialroomsRef = databaseReference.Child("specialrooms").Child(RoomId);
            specialroomsRef.RemoveValueAsync().ContinueWith(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("Failed to remove the specail room from database :  " + task.Exception);
                }
                else
                {
                    Debug.Log("room removed from database successfully");

                    // Optionally re-enable the StartButton if you want to allow rejoining the queue

                    
                }
            });

        }
        else
        {
            Debug.LogError("Database reference is not initialized");
        }
    }

    void OnJoinRoomButtonClicked()
    {
    
        string roomId = roomIdInputField.text;
        if (string.IsNullOrEmpty(roomId))
        {
            Debug.LogError("Room ID cannot be empty.");
            feedbackManager.ShowFeedback("لا يمكن ان يكون رقم الغرفة فارغا");
            return;
        }
        
        Debug.Log("Joining room with ID: " + roomId);
        JoinSpecialRoom(roomId);
    }

    public void JoinSpecialRoom(string roomId)
    {
        radialProgressBar.StartSpinning();
        var data = new Dictionary<string, object> { { "roomId", roomId } };

        functions.GetHttpsCallable("joinSpecialRoom").CallAsync(data).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
            {
                var result = task.Result.Data as Dictionary<string, object>;
                Debug.Log("Joined special room with ID: " + roomId);
                radialProgressBar.StopSpinning();
                feedbackManager.ShowFeedback("لقد انضممت الى الغرفة بنجاح");
                // Handle post-join actions here
            }
            else
            {
                Debug.LogError("Error joining special room: " + task.Exception);
                feedbackManager.ShowFeedback("رقم الغرفة خاطئ او غير موجود");
                radialProgressBar.StopSpinning();
            }
        });
    }

    private void DisplayRoomId(string roomId)
    {
        if (roomIdText != null)
        {
            Debug.Log("Displaying room ID: " + roomId);
            roomIdText.text =  roomId;
            roomIdText.gameObject.SetActive(true);

            // Force a layout rebuild to ensure the UI is updated immediately
            Canvas.ForceUpdateCanvases(); // Use this if you face issues with layout updates
            LayoutRebuilder.ForceRebuildLayoutImmediate(roomIdText.rectTransform);
        }
        else
        {
            Debug.LogWarning("Room ID text object is not assigned.");
        }
    }

    public void CopyRoomIdToClipboard()
    {
        Debug.Log("Copy button clicked.");
        if (!string.IsNullOrEmpty(RoomId))
        {
            GUIUtility.systemCopyBuffer = RoomId;
            Debug.Log("Room ID copied to clipboard: " + RoomId);
            feedbackManager.ShowFeedback("لقد تم نسخ رقم الغرفة");
          
        }
        else
        {
            Debug.LogWarning("No Room ID to copy.");
            feedbackManager.ShowFeedback("لا يوجد رقم غرفة للنسخ");
        }
    }
}
