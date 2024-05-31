using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
public class FetchUserProfile : MonoBehaviour
{
   public static FetchUserProfile instance;
    public TextMeshProUGUI usernameText;
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI gemsText;
    public TextMeshProUGUI xpText;
    public TextMeshProUGUI levelText;
    public GameObject setUserPanel;
    public Image progressBarFill;
    private int coins;
    private int gems;
    private int xp;
    private int level;
    private int requiredXPForNextLevel;

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

        UpdateProgressBar(xp, requiredXPForNextLevel);
        OnBalanceChanged?.Invoke(coins, gems); // Notify listeners about the balance change
    }

    public void UpdateProgressBar(int playerXP, int requiredXP)
    {
        requiredXP = (level * 25) + 50;
        float fillAmount = (float)playerXP / requiredXP;
        progressBarFill.fillAmount = fillAmount;
        levelText.text = level.ToString();
        xpText.text = $"your xp is : {playerXP} / {requiredXP}";
    }
}
