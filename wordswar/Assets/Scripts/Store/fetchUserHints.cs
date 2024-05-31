using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;

public class FetchUserHints : MonoBehaviour
{
    public TextMeshProUGUI jokerHintsText;
    public TextMeshProUGUI extraTimeText;

    void Start()
    {
        // Listen for changes in user hints data
        UserManager.Instance.OnUserHintsUpdated += UpdateUserHints;

        // Fetch initial user hints data
        UserManager.Instance.CheckUserProfileCompletion();
    }

    private void OnDestroy()
    {
        // Unsubscribe from events to avoid memory leaks
        UserManager.Instance.OnUserHintsUpdated -= UpdateUserHints;
    }

    private void UpdateUserHints(Dictionary<string, object> userHints)
    {
        // Update UI elements with user hints data
        if (userHints.TryGetValue("joker", out object jokerObj) && userHints.TryGetValue("extraTime", out object extraTimeObj))
        {
            int jokerHints = Convert.ToInt32(jokerObj);
            int extraTimeHints = Convert.ToInt32(extraTimeObj);
            UpdateUI(jokerHints, extraTimeHints);
        }
        else
        {
            Debug.LogError("joker or extraTime key is missing in hintsData");
        }
    }

    void UpdateUI(int jokerHints, int extraTimeHints)
    {
        jokerHintsText.text = jokerHints.ToString();
        extraTimeText.text = extraTimeHints.ToString();
        Debug.Log("joker hints : " + jokerHints);
        Debug.Log("extra time hints : " + extraTimeHints);
    }
}
