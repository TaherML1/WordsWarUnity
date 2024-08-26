using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;

public class FetchUserHints : MonoBehaviour
{
    public TextMeshProUGUI jokerHintsText;
    public TextMeshProUGUI extraTimeText;
    public TextMeshProUGUI ticketsText;

    private int jokerHints;
    private int extraTimeHints;
    private int tickets;
    private int refreshedTickets;
    public static int totalTickets { get; private set; }

    public static FetchUserHints Instance { get; private set; }
    void Start()
    {
        // Listen for changes in user hints data
        UserManager.Instance.OnUserHintsUpdated += UpdateUserHints;

        // Fetch initial user hints data
        UserManager.Instance.CheckUserProfileCompletion();

        // Fetch initial user hints data
        var initialHints = UserManager.Instance.GetUserHints();
        if (initialHints != null)
        {
            UpdateUserHints(initialHints);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events to avoid memory leaks
        UserManager.Instance.OnUserHintsUpdated -= UpdateUserHints;
    }

    private void UpdateUserHints(Dictionary<string, object> userHints)
    {
        // Update UI elements with user hints data
        if (userHints.TryGetValue("joker", out object jokerObj) && userHints.TryGetValue("extraTime", out object extraTimeObj) && userHints.TryGetValue("tickets",out object ticketsObj) && userHints.TryGetValue("refreshedTickets", out object refreshedTicketsObj))
        {
            jokerHints = Convert.ToInt32(jokerObj);
            extraTimeHints = Convert.ToInt32(extraTimeObj);
            tickets = Convert.ToInt32(ticketsObj);
            refreshedTickets = Convert.ToInt32(refreshedTicketsObj);

            totalTickets = tickets + refreshedTickets;
            UpdateUI(jokerHints, extraTimeHints,totalTickets);
        }
        else
        {
            Debug.LogError("joker or extraTime key is missing in hintsData");
        }
    }

    void UpdateUI(int jokerHints, int extraTimeHints,int tickets)
    {
        jokerHintsText.text = jokerHints.ToString();
        extraTimeText.text = extraTimeHints.ToString();
        ticketsText.text = tickets.ToString() + "/3";
        Debug.Log("joker hints : " + jokerHints);
        Debug.Log("extra time hints : " + extraTimeHints);
        Debug.Log("Total tickets : " + tickets);
    }
    public int GetTickets()
    {
        return tickets;
    }
    public bool HasTickets()
    {
        return tickets > 0;
    }

}