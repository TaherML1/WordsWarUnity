using Firebase;
using Firebase.Functions;
using Firebase.Extensions;
using System.Collections.Generic;
using System;
using UnityEngine;

public class TicketManager : MonoBehaviour
{
    private FirebaseFunctions functions;

    void Start()
    {
        functions = FirebaseFunctions.DefaultInstance;
        // Call this periodically or when the player opens the app
        
    }

    public void RefreshTickets()
    {
        var refreshTicketsFunction = functions.GetHttpsCallable("refreshTickets");
        refreshTicketsFunction.CallAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                var result = task.Result.Data as Dictionary<string, object>;
                int newTicketCount = Convert.ToInt32(result["tickets"]);
                Debug.Log("Tickets refreshed: " + newTicketCount);
                // Update local ticket count and UI accordingly
            }
            else
            {
                Debug.LogError("Failed to refresh tickets: " + task.Exception);
            }
        });
    }
}
