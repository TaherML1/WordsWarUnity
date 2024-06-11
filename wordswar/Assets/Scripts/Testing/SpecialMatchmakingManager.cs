using Firebase.Functions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;


public class SpecialMatchmakingManager : MonoBehaviour
{
    FirebaseFunctions functions;


    public TMP_Text roomIdText;
    public TMP_InputField roomIdInputField; // Input field for entering the room ID
    public Button joinRoomButton;       // Button to join the room
    public Button copyButton;

    private string currentRoomId; // Store the current Room ID

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                functions = FirebaseFunctions.DefaultInstance;
                joinRoomButton.onClick.AddListener(OnJoinRoomButtonClicked);
                Debug.Log("hellloo");
            }
            else
            {
                Debug.LogError("Failed to initialize Firebase");
            }
        });

       
    }

    public void CreateSpecialRoom()
    {
        if (functions != null)

        functions.GetHttpsCallable("createSpecialRoom").CallAsync().ContinueWith(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
            {
                var result = task.Result.Data as Dictionary<string, object>;
                currentRoomId = result["roomId"].ToString();
                Debug.Log("Special room created with ID: " + currentRoomId);
                DisplayRoomId(currentRoomId);
            }
            else
            {
                Debug.LogError("Error creating special room: " + task.Exception);
            }
        });
    }

    void OnJoinRoomButtonClicked()
    {
        string roomId = roomIdInputField.text; // Get the room ID from the input field

        if (string.IsNullOrEmpty(roomId))
        {
            Debug.LogError("Room ID cannot be empty");
            return;
        }

        JoinSpecialRoom(roomId);
    }

    public void JoinSpecialRoom(string roomId)
    {
        var data = new Dictionary<string, object> {
            { "roomId", roomId }
        };

        functions.GetHttpsCallable("joinSpecialRoom").CallAsync(data).ContinueWith(task => {
            if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
            {
                var result = task.Result.Data as Dictionary<string, object>;
                Debug.Log("Joined special room with ID: " + roomId);
                // Optionally handle post-join actions like loading the game scene
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
            roomIdText.text = "Room ID: " + roomId;
            roomIdText.gameObject.SetActive(true); // Ensure the text is visible
        }
        else
        {
            Debug.LogWarning("Room ID text object is not assigned.");
        }
    }

    // Method to copy the Room ID to the clipboard
   public void CopyRoomIdToClipboard()
    {
        Debug.Log("bbutton clicked");
        if (!string.IsNullOrEmpty(currentRoomId))
        {
            GUIUtility.systemCopyBuffer = currentRoomId; // Copy to clipboard
            Debug.Log("Room ID copied to clipboard: " + currentRoomId);
        }
        else
        {
            Debug.LogWarning("No Room ID to copy.");
        }
    }

}
