using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using System.Collections;

public class FetchUserProfile : MonoBehaviour
{
    public static FetchUserProfile instance;
    [SerializeField] TextMeshProUGUI usernameText;
    [SerializeField] TextMeshProUGUI playerIdText;
    [SerializeField] TextMeshProUGUI coinsText;
    [SerializeField] TextMeshProUGUI gemsText;
    [SerializeField] TextMeshProUGUI xpText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI matchesLostText;
    [SerializeField] TextMeshProUGUI matchesWonText;
    [SerializeField] TextMeshProUGUI ScoresText;
    [SerializeField] TextMeshProUGUI PlayerId2Text;
    [SerializeField] TextMeshProUGUI textCopiedText;
    [SerializeField] FeedbackManager feedbackManager;

    
    public GameObject setUserPanel;
    [SerializeField] Image progressBarFill;

    private int coins;
    private int gems;
    private int xp;
    private int level;
    private int requiredXPForNextLevel;
    private int matcheslost;
    private int matchesWon;
    private int scores;
    private string playerId;

    public static event Action<int, int> OnBalanceChanged; // Event to notify balance changes

    private void Start()
    {
        instance = this;
        UserManager.Instance.OnUserProfileUpdated += UpdateUserProfileUI;
        UserManager.Instance.OnInitialUserProfileFetched += OnInitialUserProfileFetched;

        // Optionally, initialize the UI if data is already available
        var initialProfile = UserManager.Instance.GetUserProfile();
        if (initialProfile != null)
        {
            UpdateUserProfileUI(initialProfile);
        }
    }

    private void OnDestroy()
    {
        if (UserManager.Instance != null)
        {
            UserManager.Instance.OnUserProfileUpdated -= UpdateUserProfileUI;
            UserManager.Instance.OnInitialUserProfileFetched -= OnInitialUserProfileFetched;
        }
    }

    private void OnInitialUserProfileFetched()
    {
        var userProfile = UserManager.Instance.GetUserProfile();
        if (userProfile != null)
        {
            UpdateUserProfileUI(userProfile);
        }
    }

    private void UpdateUserProfileUI(Dictionary<string, object> userProfile)
    {
        if (userProfile == null)
        {
            Debug.LogError("UserProfile dictionary is null");
            return;
        }

        if (userProfile.TryGetValue("username", out object usernameObj))
        {
            usernameText.text = usernameObj?.ToString() ?? "N/A";
        }

        if (userProfile.TryGetValue("playerId", out object playerIdObj))
        {
            playerIdText.text = "# " + playerIdObj?.ToString() ?? "N/A";
            PlayerId2Text.text = playerIdObj.ToString();
            playerId  = Convert.ToString(playerIdObj);

        }

        if (userProfile.TryGetValue("coins", out object coinsObj))
        {
            coins = Convert.ToInt32(coinsObj);
            coinsText.text = coins.ToString();
        }

        if (userProfile.TryGetValue("gems", out object gemsObj))
        {
            gems = Convert.ToInt32(gemsObj);
            gemsText.text = gems.ToString();
        }

        if (userProfile.TryGetValue("xp", out object xpObj))
        {
            xp = Convert.ToInt32(xpObj);
            xpText.text = xp.ToString();
        }

        if (userProfile.TryGetValue("level", out object levelObj))
        {
            level = Convert.ToInt32(levelObj);
            levelText.text = level.ToString();
        }

        if (userProfile.TryGetValue("matchesLost", out object matchesLostObj))
        {
            matcheslost = Convert.ToInt32(matchesLostObj);
            matchesLostText.text = matcheslost.ToString();
            Debug.Log($"Matches Lost: {matcheslost}");
        }

        if (userProfile.TryGetValue("matchesWon", out object matchesWonObj))
        {
            matchesWon = Convert.ToInt32(matchesWonObj);
            matchesWonText.text = matchesWon.ToString();
            Debug.Log($"Matches Won: {matchesWon}");
        }

        if (userProfile.TryGetValue("scores", out object scoresObj))
        {
            scores = Convert.ToInt32(scoresObj);
            ScoresText.text = scores.ToString();
        }

        UpdateProgressBar(xp, requiredXPForNextLevel);
        OnBalanceChanged?.Invoke(coins, gems); // Notify listeners about the balance change
    }

    public void UpdateProgressBar(int playerXP, int requiredXP)
    {
        requiredXP = (level * 25) + 50;
        float fillAmount = (float)playerXP / requiredXP;
        progressBarFill.fillAmount = fillAmount;
        levelText.text = level.ToString();
        xpText.text = $" {playerXP} / {requiredXP}";
    }

    public void CopyPlayerIdToClipboard()
    {
        Debug.Log("Copy button clicked.");
        if (!string.IsNullOrEmpty(playerId))
        {
            GUIUtility.systemCopyBuffer = playerId;
            Debug.Log("Room ID copied to clipboard: " + playerId);
            feedbackManager.ShowFeedback("لقد تم نسخ رقم الاعب");
            textCopiedText.text = "لقد تم نسخ رقم الاعب";
            StartCoroutine(HideCopiedTextAfterDelay(2f));

        }
        else
        {
            Debug.LogWarning("No Room ID to copy.");
            feedbackManager.ShowFeedback("لا يوجد رقم لاعب للنسخ");
        }
    }
    private IEnumerator HideCopiedTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        textCopiedText.text = "";  // Clear the text after the delay
    }

    public void ShowTicketAd()
    {
        AdManager.Instance.ShowRewardedAdTicket();
    }
    public void ShowSpinTicketAd()
    {
        AdManager.Instance.ShowRewardedAdSpin();
    }

}
