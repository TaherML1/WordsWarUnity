using System.Collections;
using UnityEngine;
using GoogleMobileAds.Api;

public class AdManager : MonoBehaviour
{
    public static AdManager Instance { get; private set; }

    private RewardedAd _rewardedAdTicket;
    private RewardedAd _rewardedAdSpin;
    private RewardedAd _rewardedAdCoins;

    public string adUnitIdRewardedTicket;
    public string adUnitIdRewardedSpin;
    public string adUnitIdRewardedCoins;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize((InitializationStatus initStatus) =>
        {
            Debug.Log("Google Mobile Ads SDK initialized.");
            RequestRewardedAds();
        });
    }

    private void RequestRewardedAds()
    {
        RequestRewardedAdTicket();
        RequestRewardedAdSpin();
        RequestRewardedAdCoins();
    }

    private void RequestRewardedAdTicket()
    {
        var adRequest = new AdRequest();
        RewardedAd.Load(adUnitIdRewardedTicket, adRequest, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("Rewarded ad for ticket failed to load with error: " + error);
            }
            else
            {
                Debug.Log("Rewarded ad for ticket loaded with response: " + ad.GetResponseInfo());
                _rewardedAdTicket = ad;
            }
        });
    }

    private void RequestRewardedAdSpin()
    {
        var adRequest = new AdRequest();
        RewardedAd.Load(adUnitIdRewardedSpin, adRequest, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("Rewarded ad for spin failed to load with error: " + error);
            }
            else
            {
                Debug.Log("Rewarded ad for spin loaded with response: " + ad.GetResponseInfo());
                _rewardedAdSpin = ad;
            }
        });
    }

    private void RequestRewardedAdCoins()
    {
        var adRequest = new AdRequest();
        RewardedAd.Load(adUnitIdRewardedCoins, adRequest, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("Rewarded ad for coins failed to load with error: " + error);
            }
            else
            {
                Debug.Log("Rewarded ad for coins loaded with response: " + ad.GetResponseInfo());
                _rewardedAdCoins = ad;
            }
        });
    }

    public void ShowRewardedAdTicket(System.Action onAdCompleted =null)
    {
        if (_rewardedAdTicket != null && _rewardedAdTicket.CanShowAd())
        {
            _rewardedAdTicket.Show(reward =>
            {
                Debug.Log($"Rewarded ad for ticket rewarded the user. Type: {reward.Type}, amount: {reward.Amount}");
                onAdCompleted?.Invoke();
                RequestRewardedAdTicket();
            });
        }
        else
        {
            Debug.LogError("Rewarded ad for ticket is not ready yet.");
        }
    }

    public void ShowRewardedAdSpin(System.Action onAdCompleted = null)
    {
        if (_rewardedAdSpin != null && _rewardedAdSpin.CanShowAd())
        {
            _rewardedAdSpin.Show(reward =>
            {
                Debug.Log($"Rewarded ad for spin rewarded the user. Type: {reward.Type}, amount: {reward.Amount}");
                onAdCompleted?.Invoke(); // Call the additional action after the ad is shown
                RequestRewardedAdSpin();
            });
        }
        else
        {
            Debug.LogError("Rewarded ad for spin is not ready yet.");
        }
    }


    public void ShowRewardedAd(System.Action onAdCompleted)
    {
        if (_rewardedAdCoins != null && _rewardedAdCoins.CanShowAd())
        {
            _rewardedAdCoins.Show(reward =>
            {
                Debug.Log("Rewarded ad completed.");
                onAdCompleted?.Invoke();
                RequestRewardedAdCoins();
            });
        }
        else
        {
            Debug.LogError("Rewarded ad is not ready yet.");
        }
    }
}
