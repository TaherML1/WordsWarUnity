using Firebase.Functions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Extensions; // Make sure to use this namespace for ContinueWithOnMainThread

public class SpecialMatchmakingManager : MonoBehaviour
{
    FirebaseFunctions functions;

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
        Debug.Log("Creating a special room...");
        if (functions != null)
        {
            functions.GetHttpsCallable("createSpecialRoom").CallAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
                {
                    var result = (string)task.Result.Data;
                    RoomId = result;

                    Debug.Log("Special room created with ID: " + RoomId);
                    DisplayRoomId(RoomId);
                }
                else
                {
                    Debug.LogError("Error creating special room: " + task.Exception);
                }
            });
        }
        else
        {
            Debug.LogError("Firebase functions instance is null.");
        }
    }

    void OnJoinRoomButtonClicked()
    {
        Debug.Log("Join room button clicked.");
        string roomId = roomIdInputField.text;
        if (string.IsNullOrEmpty(roomId))
        {
            Debug.LogError("Room ID cannot be empty.");
            return;
        }

        Debug.Log("Joining room with ID: " + roomId);
        JoinSpecialRoom(roomId);
    }

    public void JoinSpecialRoom(string roomId)
    {
        var data = new Dictionary<string, object> { { "roomId", roomId } };

        functions.GetHttpsCallable("joinSpecialRoom").CallAsync(data).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
            {
                var result = task.Result.Data as Dictionary<string, object>;
                Debug.Log("Joined special room with ID: " + roomId);
                // Handle post-join actions here
            }
            else
            {
                Debug.LogError("Error joining special room: " + task.Exception);
            }
        });
    }

    private void DisplayRoomId(string roomId)
    {
        if (roomIdText != null)
        {
            Debug.Log("Displaying room ID: " + roomId);
            roomIdText.text = "Room ID: " + roomId;
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
        }
        else
        {
            Debug.LogWarning("No Room ID to copy.");
        }
    }
}
