using Firebase.Functions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpecialMatchmakingManager : MonoBehaviour
{
    FirebaseFunctions functions;

    public TMP_InputField roomIdInputField; // Input field for entering the room ID
    public Button joinRoomButton;       // Button to join the room

    void Start()
    {
        functions = FirebaseFunctions.DefaultInstance;

        // Assign the JoinSpecialRoom function to the button's onClick event
        joinRoomButton.onClick.AddListener(OnJoinRoomButtonClicked);
    }

    public void CreateSpecialRoom()
    {
        functions.GetHttpsCallable("createSpecialRoom").CallAsync().ContinueWith(task => {
            if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
            {
                var result = task.Result.Data as Dictionary<string, object>;
                string roomId = result["roomId"].ToString();
                Debug.Log("Special room created with ID: " + roomId);
                // Optionally display the room ID to the player
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
}
